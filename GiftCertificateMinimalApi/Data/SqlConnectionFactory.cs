using GiftCertificateMinimalApi.Logging;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace GiftCertificateMinimalApi.Data
{
    public class SqlConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqlConnectionFactory> _logger;

        public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<DbConnection> CreateConnectionAsync()
        {
            DbConnection result = new();

            var watch = Stopwatch.StartNew();

            var connectionParameters = _configuration.GetSection("OneSDatabases")
                .Get<List<DatabaseConnectionParameter>>()
                .Select(x => new DatabaseInfo(x));

            var timeMs = DateTime.Now.Millisecond % 100;

            List<string> failedConnections = new();

            bool firstAvailable = false;

            var resultString = "";

            SqlConnection? connection = null;

            while (true)
            {
                int percentCounter = 0;
                foreach (var connParameter in connectionParameters)
                {
                    if (firstAvailable && failedConnections.Contains(connParameter.Connection))
                        continue;

                    percentCounter += connParameter.Priority;
                    if (timeMs <= percentCounter && connParameter.Priority != 0 || firstAvailable)
                    {
                        try
                        {
                            connection = await GetConnectionByDatabaseInfo(connParameter);

                            resultString = connParameter.Connection;

                            result.Connection = connection;
                            result.DatabaseType = connParameter.DatabaseType;
                            result.ConnectionWithoutCredentials = connParameter.ConnectionWithoutCredentials;
                            break;
                        }
                        catch (Exception ex)
                        {
                            var logElement = new ElasticLogElement(LogStatus.Error)
                            {
                                ErrorDescription = ex.Message,
                                DatabaseConnection = connParameter.ConnectionWithoutCredentials
                            };

                            _logger.LogMessageGen(logElement.ToString());

                            if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                            {
                                _ = connection.CloseAsync();
                            }

                            failedConnections.Add(connParameter.Connection);
                        }
                    }
                }

                if (resultString.Length > 0 || firstAvailable)
                    break;
                else
                    firstAvailable = true;
            }
            watch.Stop();
            result.ConnectTimeInMilliseconds = watch.ElapsedMilliseconds;

            return result;
        }

        private static async Task<SqlConnection?> GetConnectionByDatabaseInfo(DatabaseInfo databaseInfo)
        {
            var queryStringCheck = databaseInfo.DatabaseType switch
            {
                DatabaseType.Main => Queries.DatabaseBalancingMain,
                DatabaseType.ReplicaFull => Queries.DatabaseBalancingReplicaFull,
                DatabaseType.ReplicaTables => Queries.DatabaseBalancingReplicaTables,
                _ => ""
            };

            //sql connection object
            SqlConnection connection = new(databaseInfo.Connection);
            await connection.OpenAsync();

            SqlCommand cmd = new(queryStringCheck, connection)
            {
                CommandTimeout = 1
            };

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            _ = dr.CloseAsync();

            return connection;
        }
    }
}
