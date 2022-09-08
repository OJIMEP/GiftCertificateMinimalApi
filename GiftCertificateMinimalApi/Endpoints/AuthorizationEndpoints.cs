using AuthLibrary.Data;
using DateTimeService.Areas.Identity.Models;
using GiftCertificateMinimalApi.Contracts.V1.Responses;
using GiftCertificateMinimalApi.Endpoints.Internal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GiftCertificateMinimalApi.Endpoints
{
    public class AuthorizationEndpoints : IEndpoints
    {
        public static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DateTimeServiceContext>(
                options => options.UseSqlServer(configuration.GetConnectionString("DateTimeServiceContextConnection"))
                );

            services.AddDefaultIdentity<DateTimeServiceUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<DateTimeServiceContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Adding Jwt Bearer  
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
                    ValidateLifetime = true
                };
            });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                  .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                  .RequireAuthenticatedUser()
                  .Build();
            });

            services.AddScoped<IUserService, UserService>();
        }

        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Authenticate/login",
            async (LoginModel model, IUserService userService, UserManager<DateTimeServiceUser> userManager, HttpContext context) =>
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                {
                    var response = await userService.AuthenticateAsync(model, context.Connection.RemoteIpAddress?.ToString() ?? "");

                    return Results.Ok(new LoginResponse(response));
                }
                return Results.Unauthorized();
            })
            .Accepts<LoginModel>("application/json")
            .Produces<LoginResponse>()
            .Produces(400)
            .Produces(401)
            .WithTags("Authorization")
            .AllowAnonymous();
        }
    }
}
