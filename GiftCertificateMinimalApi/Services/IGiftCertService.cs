using GiftCertificateMinimalApi.Logging;
using GiftCertificateMinimalApi.Models;

namespace GiftCertificateMinimalApi.Services
{
    public interface IGiftCertService
    {
        Task<List<CertGetResponseDto>> GetCertsInfoByListAsync(List<string> barcodes);

        ElasticLogElementInternal GetLog();
    }
}
