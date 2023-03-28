using System.Runtime.Versioning;

namespace GiftCertificateMinimalApi.Endpoints.Internal
{
    public interface IEndpoints
    {
        [RequiresPreviewFeatures]
        public static abstract void DefineEndpoints(IEndpointRouteBuilder app);

        [RequiresPreviewFeatures]
        public static abstract void AddServices(IServiceCollection services, IConfiguration configuration);
    }
}
