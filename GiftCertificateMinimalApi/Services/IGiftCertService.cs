using GiftCertificateMinimalApi.Contracts.V1.Responses;
using GiftCertificateMinimalApi.Logging;

namespace GiftCertificateMinimalApi.Services
{
    public interface IGiftCertService
    {
        Task<IEnumerable<CertGetResponse>> GetCertsInfoByListAsync(List<string> barcodes);

        ElasticLogElementInternal GetLog();
    }
}
