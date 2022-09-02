using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GiftCertificateMinimalApi.Auth
{
    public abstract class SwaggerSecurityScheme : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
        }
    }
}
