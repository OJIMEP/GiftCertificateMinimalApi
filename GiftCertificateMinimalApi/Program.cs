using GiftCertificateMinimalApi.Logging;
using GiftCertificateMinimalApi.Endpoints.Internal;
using Microsoft.AspNetCore.HttpOverrides;
using AuthLibrary.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// устанавливаем все наши сервисы
builder.Services.AddEndpoints<Program>(builder.Configuration);

builder.Logging.AddProvider(
    new HttpLoggerProvider(
        builder.Configuration["loggerHost"],
        builder.Configuration.GetValue<int>("loggerPortUdp"),
        builder.Configuration.GetValue<int>("loggerPortHttp"),
        builder.Configuration["loggerEnv"]
    )
);

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                           ForwardedHeaders.XForwardedProto
});

app.UseCors(corsBuilder => corsBuilder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowedToAllowWildcardSubdomains()
    .WithOrigins(builder.Configuration.GetSection("CorsOrigins").Get<List<string>>().ToArray()
    )
);

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// добавляем все наши эндпоинты
app.UseEndpoints<Program>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        //var db = services.GetRequiredService<DateTimeServiceContext>();
        //db.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
    }

    try
    {
        var userManager = services.GetRequiredService<UserManager<DateTimeServiceUser>>();
        var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        await RoleInitializer.InitializeAsync(userManager, rolesManager, configuration);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }

    try
    {
        var db = services.GetRequiredService<DateTimeServiceContext>();
        await RoleInitializer.CleanTokensAsync(db);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while clearing the database.");
    }
}

app.Run();
