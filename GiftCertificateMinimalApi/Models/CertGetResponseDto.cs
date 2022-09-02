namespace GiftCertificateMinimalApi.Models
{
    public class CertGetResponseDto
    {
        public string Barcode { get; set; } = default!;
        
        public decimal Sum { get; set; }
    }
}
