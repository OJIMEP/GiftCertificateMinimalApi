using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GiftCertificateMinimalApi.Auth
{
    public class SwaggerSecurityScheme : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
        }
    }
}
