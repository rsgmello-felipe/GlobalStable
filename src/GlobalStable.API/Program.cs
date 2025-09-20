using Asp.Versioning;
using GlobalStable.API.DependencyInjection;
using GlobalStable.Application.DependencyInjection;
using GlobalStable.Domain.Constants;
using GlobalStable.Infrastructure.DependencyInjection;
using GlobalStable.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<CallbackSettings>(builder.Configuration.GetSection("CallbackSettings"));

builder.Services.AddCustomAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddUseCases();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddLogging();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.Services.ApplyDatabaseMigrations();

app.UseAuditMiddleware();

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();
app.MapHealthChecks("/check_server_status").AllowAnonymous();


await app.RunAsync();
