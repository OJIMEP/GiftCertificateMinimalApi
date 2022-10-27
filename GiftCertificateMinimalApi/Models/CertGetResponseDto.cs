namespace GiftCertificateMinimalApi.Models
{
    public class CertGetResponseDto
    {
        public string Barcode { get; set; } = default!;
        
        public decimal Sum { get; set; }

        public bool IsActive { get; set; }

        public bool IsValid { get; set; }

        public bool NotFound { get; set; }
    }
}
