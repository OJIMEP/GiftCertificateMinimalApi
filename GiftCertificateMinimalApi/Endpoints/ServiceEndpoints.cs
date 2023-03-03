using GiftCertificateMinimalApi.Contracts.V1.Responses;
using GiftCertificateMinimalApi.Data;
using GiftCertificateMinimalApi.Endpoints.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using GiftCertificateMinimalApi.Services;

namespace GiftCertificateMinimalApi.Endpoints
{
    public class ServiceEndpoints : IEndpoints
    {
        private const string Tag = "Monitoring";
        private const string BaseRoute = "api/HealthCheck";

        public static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IGiftCertService, GiftCertService>();            
            services.AddSingleton<SqlConnectionFactory>();            
        }

        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet(BaseRoute, GetHealthCheck)
                .Produces<ErrorResponse>(400)
                .Produces<string>(404)
                .Produces<ErrorResponse>(500)
                .WithTags(Tag)
                .AllowAnonymous();
        }

        internal static async Task<IResult> GetHealthCheck(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
        {
            DbConnection dbConnection = new();
            SqlConnectionFactory ConnectionFactory = new(configuration, logger);

            try
            {
                dbConnection = await ConnectionFactory.CreateConnectionAsync();
            }
            catch (Exception ex)
            {
                var Problem = new ProblemDetails();
                Problem.Detail = ex.Message;
                Problem.Status = 500;

                return Results.Problem(Problem);
            }

            if (dbConnection.Connection == null)
            {
                var Problem = new ProblemDetails();
                Problem.Detail = "No database connection available";
                Problem.Status = 500;

                return Results.Problem(Problem);
            }

            await dbConnection.Connection.CloseAsync();

            return Results.Ok(200);

        }

    }
}