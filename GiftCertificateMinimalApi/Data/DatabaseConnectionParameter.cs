namespace GiftCertificateMinimalApi.Data
{
    public class DatabaseConnectionParameter
    {
        public string Connection { get; protected set; } = "";
        public int Priority { get; protected set; }
        public string Type { get; set; } = ""; //main, replica_full, replica_tables 
    }
}
