using System.Text.Json.Serialization;
using DirectoryService.Core;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Web.Middlewares;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCore();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

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