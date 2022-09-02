using Microsoft.Data.SqlClient;

namespace GiftCertificateMinimalApi.Data
{
    public class DbConnection
    {
        public SqlConnection? Connection { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public string ConnectionWithoutCredentials { get; set; } = "";
        public long ConnectTimeInMilliseconds { get; set; }
    }
}
