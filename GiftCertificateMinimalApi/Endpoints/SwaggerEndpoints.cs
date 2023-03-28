using GiftCertificateMinimalApi.Auth;
using GiftCertificateMinimalApi.Endpoints.Internal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Runtime.Versioning;

namespace GiftCertificateMinimalApi.Endpoints
{
    public class SwaggerEndpoints : IEndpoints
    {
        [RequiresPreviewFeatures]
        public static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(setup =>
            {
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Put **_ONLY_** your JWT Bearer token on text-box below!",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                setup.OperationFilter<SwaggerSecurityScheme>();

                setup.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Gift Certificates Info Minimal API",
                    Description = "Simple service to get info about valid gift certificates",
                    Contact = new OpenApiContact
                    {
                        Name = "Vasily Levkovsky",
                        Email = "v.levkovskiy@21vek.by"
                    }
                });
            });
        }

        [RequiresPreviewFeatures]
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
        }
    }
}
