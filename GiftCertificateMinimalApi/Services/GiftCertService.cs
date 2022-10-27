using AutoMapper;
using GiftCertificateMinimalApi.Data;
using GiftCertificateMinimalApi.Exceptions;
using GiftCertificateMinimalApi.Models;
using GiftCertificateMinimalApi.Logging;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace GiftCertificateMinimalApi.Services
{
    public class GiftCertService : IGiftCertService
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly IMapper _mapper;
        private List<string> _barcodesList = default!;
        private ElasticLogElementInternal _logElement;

        public GiftCertService(SqlConnectionFactory connectionFactory, IMapper mapper)
        {
            _connectionFactory = connectionFactory;
            _mapper = mapper;
            _logElement = new();
        }

        public async Task<List<CertGetResponseDto>> GetCertsInfoByListAsync(List<string> barcodes)
        {
            _barcodesList = barcodes;
            _logElement = new();
            var result = new List<CertGetResponseDto>();
            
            SqlConnection connection = await GetSqlConnectionAsync();

            var watch = Stopwatch.StartNew();
            try
            {
                SqlCommand sqlCommand = GetSqlCommandCertInfo(connection);

                result = await GetCertsInfoResult(sqlCommand);

                _logElement.SetResponse(result);
                _logElement.SetStatistics(connection.RetrieveStatistics());
            }
            catch (Exception ex)
            {
                _logElement.SetError(ex.Message);
            }
            watch.Stop();
            _logElement.SetExecutionFact(watch.ElapsedMilliseconds);

            _ = connection.CloseAsync();

            return result;
        }

        private async Task<List<CertGetResponseDto>> GetCertsInfoResult(SqlCommand sqlCommand)
        {
            var resultDto = new List<CertGetResponseDto>();

            await using (var dataReader = await sqlCommand.ExecuteReaderAsync())
            {
                while (await dataReader.ReadAsync())
                {
                    resultDto.Add(_mapper.Map<CertGetResponseDto>(dataReader));
                }
            }

            resultDto.ForEach(x => x.Barcode = _barcodesList.Find(b => b.ToUpper() == x.Barcode) ?? x.Barcode);

            foreach (var barcode in _barcodesList)
            {
                if (resultDto.Find(x => x.Barcode == barcode) == null)
                {
                    resultDto.Add(new CertGetResponseDto
                    {
                        Barcode = barcode,
                        NotFound = true
                    });
                }
            }

            return resultDto;
        }

        private async Task<SqlConnection> GetSqlConnectionAsync()
        {
            bool loadBalancingError = false;
            string loadBalancingErrorDescription = string.Empty;

            DbConnection dbConnection = new();

            try
            {
                dbConnection = await _connectionFactory.CreateConnectionAsync();
            }
            catch (Exception ex)
            {
                loadBalancingError = true;
                loadBalancingErrorDescription = ex.Message;
            }

            if (!loadBalancingError && dbConnection.Connection == null)
            {
                loadBalancingError = true;
                loadBalancingErrorDescription = "Не найдено доступное соединение к БД";
            }

            _logElement.SetDbConnectTime(dbConnection.ConnectTimeInMilliseconds);
            _logElement.SetDatabaseConnection(dbConnection.ConnectionWithoutCredentials);

            if (loadBalancingError)
            {
                _logElement.SetError(loadBalancingErrorDescription);
                throw new DbConnectionNotFoundException(loadBalancingErrorDescription);
            }

            SqlConnection result = dbConnection.Connection!;

            result.StatisticsEnabled = true;

            return result;
        }

        private SqlCommand GetSqlCommandCertInfo(SqlConnection connection)
        {
            List<string> barcodesUpperCase = _barcodesList.Select(x => x.ToUpper()).Distinct().ToList();

            SqlCommand command = new()
            {
                Connection = connection,
                CommandTimeout = 5
            };

            List<string> barcodeParameters = new();
            for (int i = 0; i < barcodesUpperCase.Count; i++)
            {
                var parameterString = $"@Barcode{i}";
                barcodeParameters.Add(parameterString);
                command.Parameters.Add(parameterString, SqlDbType.NVarChar, 12);
                command.Parameters[parameterString].Value = barcodesUpperCase[i];
            }

            command.CommandText = Queries.CertInfo.Replace("@Barcode", string.Join(",", barcodeParameters));

            command.Parameters.Add("@DateNow", SqlDbType.DateTime);
            command.Parameters["@DateNow"].Value = DateTime.Now.AddMonths(24000);

            command.Parameters.Add("@EmptyDate", SqlDbType.DateTime);
            command.Parameters["@EmptyDate"].Value = new DateTime(2001, 1, 1, 0, 0, 0);

            return command;
        }

        public ElasticLogElementInternal GetLog()
        {
            return _logElement;
        }
    }
}
