using GiftCertificateMinimalApi.Contracts.V1.Responses;
using GiftCertificateMinimalApi.Data;
using GiftCertificateMinimalApi.Endpoints.Internal;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;

namespace GiftCertificateMinimalApi.Endpoints
{
    public class ServiceEndpoints : IEndpoints
    {
        private const string Tag = "Monitoring";
        private const string BaseRoute = "api/HealthCheck";

        [RequiresPreviewFeatures]
        public static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        [RequiresPreviewFeatures]
        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet(BaseRoute, GetHealthCheck)
                .Produces<ErrorResponse>(400)
                .Produces<string>(404)
                .Produces<ErrorResponse>(500)
                .WithTags(Tag)
                .AllowAnonymous();
        }

        internal static async Task<IResult> GetHealthCheck(SqlConnectionFactory _connectionFactory)
        {
            DbConnection dbConnection = new();

            try
            {
                dbConnection = await _connectionFactory.CreateConnectionAsync();
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
