namespace GiftCertificateMinimalApi.Exceptions
{
    class DbConnectionNotFoundException : SystemException
    {
        public DbConnectionNotFoundException(string message) : base(message)
        {
        }
    }
}
