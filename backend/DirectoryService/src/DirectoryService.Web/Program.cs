using System.Globalization;
using System.Text.Json.Serialization;
using DirectoryService.Core;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Web;
using DirectoryService.Web.Middlewares;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Запуск DirectoryService.Web");
    
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName());

    builder.Services.AddOpenApi();

    builder.Services.AddControllers();

    builder.Services.AddHealthChecks();

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddCore();

    builder.Services.AddWebErrorHandling();

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseMiddleware<ExceptionMiddleware>();

    app.MapGet("/", () => "DirectoryService is running!");

    app.MapHealthChecks("/health");

    app.MapControllers();

    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DirectoryService.Web неожиданно завершился");
}
finally
{
    await Log.CloseAndFlushAsync();
}