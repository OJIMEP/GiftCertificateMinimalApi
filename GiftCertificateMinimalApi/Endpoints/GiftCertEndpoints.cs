using FluentValidation;
using GiftCertificateMinimalApi.Contracts.V1.Responses;
using GiftCertificateMinimalApi.Data;
using GiftCertificateMinimalApi.Endpoints.Internal;
using GiftCertificateMinimalApi.Exceptions;
using GiftCertificateMinimalApi.Logging;
using GiftCertificateMinimalApi.Mapping;
using GiftCertificateMinimalApi.Models;
using GiftCertificateMinimalApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
                .Produces<string>(404)
                .Produces<ErrorResponse>(500)
                .Produces(401)
                .WithTags(Tag);

            app.MapPost(BaseRoute, GetCertsInfoAsync)
                .Accepts<string[]>("application/json")
                .Produces<CertPostResponse>()
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

            var certsInfoResult = await GetInfoByListAsync(barcodesList, service, validator, logger, context);

            if (certsInfoResult.IsError)
            {
                return certsInfoResult.ErrorResult;
            }

            var certInfoDto = certsInfoResult.CertsInfo.First();

            if (certInfoDto.NotFound)
            {
                return Results.NotFound("Сертификат не существует");
            }

            if (!certInfoDto.IsValid)
            {
                return Results.BadRequest(new ErrorResponse { Error = "Срок действия сертификата истек" });
            }

            if (!certInfoDto.IsActive)
            {
                return Results.BadRequest(new ErrorResponse { Error = "Сертификат не активен" });
            }

            var certGetResponse = new CertGetResponse
            {
                Barcode = certInfoDto.Barcode,
                Sum = certInfoDto.Sum
            };

            return Results.Ok(certGetResponse);
        }

        internal static async Task<IResult> GetCertsInfoAsync(
            [FromBody] List<string> barcodeList, IGiftCertService service, IValidator<List<string>> validator, 
            ILogger<GiftCertEndpoints> logger, HttpContext context)
        {
            var certsInfoResult = await GetInfoByListAsync(barcodeList, service, validator, logger, context);

            if (certsInfoResult.IsError)
            {
                return certsInfoResult.ErrorResult!;
            }

            var result = new CertPostResponse();

            foreach (var certInfoDto in certsInfoResult.CertsInfo)
            {
                if (certInfoDto.NotFound)
                {
                    result.AddError(certInfoDto.Barcode, 404, "Сертификат не существует");
                    continue;
                }

                if (!certInfoDto.IsValid)
                {
                    result.AddError(certInfoDto.Barcode, 400, "Срок действия сертификата истек");
                    continue;
                }

                if (!certInfoDto.IsActive)
                {
                    result.AddError(certInfoDto.Barcode, 400, "Сертификат не активен");
                    continue;
                }

                result.AddCertificate(certInfoDto.Barcode, certInfoDto.Sum);
            }

            return Results.Ok(result);
        }

        internal static async Task<GiftCertInfoResult> GetInfoByListAsync(
            List<string> barcodeList, 
            IGiftCertService service, 
            IValidator<List<string>> validator,
            ILogger<GiftCertEndpoints> logger,
            HttpContext context)
        {
            var certsInfoResult = new GiftCertInfoResult();

            var watch = Stopwatch.StartNew();

            var validationResult = await validator.ValidateAsync(barcodeList);

            if (!validationResult.IsValid)
            {
                certsInfoResult.SetError(Results.BadRequest(new ErrorResponse { Error = validationResult.ToString() }));
                return certsInfoResult;
            }

            try
            {
                certsInfoResult.CertsInfo = await service.GetCertsInfoByListAsync(barcodeList);
            }
            catch (DbConnectionNotFoundException)
            {
                logger.LogErrorMessage("Available database connection not found", null);
                certsInfoResult.SetError(Results.StatusCode(500));
            }
            catch (Exception ex)
            {
                logger.LogErrorMessage(ex.Message, ex);
                certsInfoResult.SetError(Results.StatusCode(500));
            }
            finally
            {
                watch.Stop();
                var logElement = new ElasticLogElement(context, service.GetLog());
                logElement.SetRequest(barcodeList);
                logElement.SetResponse(certsInfoResult.CertsInfo);
                logElement.TimeFullExecution = watch.ElapsedMilliseconds;
                logger.LogMessageGen(logElement.ToString());
            }

            if (!certsInfoResult.CertsInfo.Any())
            {
                certsInfoResult.SetError(Results.StatusCode(500));
            }

            return certsInfoResult;
        }

        internal class GiftCertInfoResult
        {
            public List<CertGetResponseDto> CertsInfo { get; set; }

            public bool IsError { get; set; }

            public IResult? ErrorResult { get; set; }

            public GiftCertInfoResult()
            {
                CertsInfo = new List<CertGetResponseDto>();
            }

            public void SetError(IResult result)
            {
                IsError = true;
                ErrorResult = result;
            }
        }
    }
}
