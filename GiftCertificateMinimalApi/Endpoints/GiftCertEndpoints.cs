using FluentValidation;
using GiftCertificateMinimalApi.Contracts.V1.Responses;
using GiftCertificateMinimalApi.Data;
using GiftCertificateMinimalApi.Endpoints.Internal;
using GiftCertificateMinimalApi.Exceptions;
using GiftCertificateMinimalApi.Logging;
using GiftCertificateMinimalApi.Mapping;
using GiftCertificateMinimalApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiftCertificateMinimalApi.Endpoints
{
    public class GiftCertEndpoints : IEndpoints
    {
        private const string Tag = "Gift certificates";
        private const string BaseRoute = "api/GiftCert";

        public static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssemblyContaining<Program>();
            ValidatorOptions.Global.LanguageManager.Enabled = false;

            services.AddSingleton<IGiftCertService, GiftCertService>();
            services.AddSingleton<SqlConnectionFactory>();
            services.AddAutoMapper(typeof(MapperProfile));

            services.AddCors();
        }

        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet(BaseRoute, GetCertInfoAsync)
                .Produces<CertGetResponse>()
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(500)
                .Produces(401)
                .WithTags(Tag);

            app.MapPost(BaseRoute, GetCertsInfoAsync)
                .Accepts<string[]>("application/json")
                .Produces<CertGetResponse[]>()
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(500)
                .Produces(401)
                .WithTags(Tag);
        }

        internal static async Task<IResult> GetCertInfoAsync(
            string barcode, IGiftCertService service, IValidator<List<string>> validator, 
            ILogger<GiftCertEndpoints> logger, HttpContext context)
        {
            var barcodesList = new List<string>
            {
                barcode
            };

            return await GetInfoByListAsync(barcodesList, service, validator, logger, context, true);
        }

        internal static async Task<IResult> GetCertsInfoAsync(
            [FromBody] List<string> barcodeList, IGiftCertService service, IValidator<List<string>> validator, 
            ILogger<GiftCertEndpoints> logger, HttpContext context)
        {
            var result = await GetInfoByListAsync(barcodeList, service, validator, logger, context);
            return result;
        }


        internal static async Task<IResult> GetInfoByListAsync(
            List<string> barcodeList, 
            IGiftCertService service, 
            IValidator<List<string>> validator,
            ILogger<GiftCertEndpoints> logger,
            HttpContext context,
            bool single = false)
        {
            var validationResult = await validator.ValidateAsync(barcodeList);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new ErrorResponse { Error = validationResult.ToString() });
            }

            IEnumerable<CertGetResponse> result = default!;

            try
            {
                result = await service.GetCertsInfoByListAsync(barcodeList);
            }
            catch (DbConnectionNotFoundException)
            {
                logger.LogErrorMessage("Available database connection not found", null);
                return Results.StatusCode(500);
            }
            catch (Exception ex)
            {
                logger.LogErrorMessage(ex.Message, ex);
                return Results.StatusCode(500);
            }
            finally
            {
                var logElement = new ElasticLogElement(context, service.GetLog());
                logElement.SetRequest(barcodeList);
                logElement.SetResponse(result);
                logger.LogMessageGen(logElement.ToString());
            }

            if (!result.Any())
            {
                return Results.BadRequest(new ErrorResponse { Error = "Certs aren't valid" });
            }

            return single ? Results.Ok(result.First()) : Results.Ok(result.ToArray());
        }
    }
}
