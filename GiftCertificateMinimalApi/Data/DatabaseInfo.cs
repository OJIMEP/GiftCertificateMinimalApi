namespace GiftCertificateMinimalApi.Data
{
    public class DatabaseInfo : DatabaseConnectionParameter
    {
        public string ConnectionWithoutCredentials { get; set; }
        public DatabaseType DatabaseType { get; set; }  

        public DatabaseInfo(DatabaseConnectionParameter connectionParameter)
        {
            Connection = connectionParameter.Connection;
            ConnectionWithoutCredentials = RemoveCredentialsFromConnectionString(Connection);
            Priority = connectionParameter.Priority;
            DatabaseType = connectionParameter.Type switch
            {
                "main" => DatabaseType.Main,
                "replica_full" => DatabaseType.ReplicaFull,
                "replica_tables" => DatabaseType.ReplicaTables,
                _ => DatabaseType.Main
            };
        }

        private static string RemoveCredentialsFromConnectionString(string connectionString)
        {
            return string.Join(";",
                connectionString.Split(";")
                    .Where(item => !item.Contains("Uid") && !item.Contains("User") && !item.Contains("Pwd") && !item.Contains("Password") && item.Length > 0));
        }
    }
}
