using Asp.Versioning;
using GlobalStable.API.DependencyInjection;
using GlobalStable.Application.DependencyInjection;
using GlobalStable.Infrastructure.DependencyInjection;
using GlobalStable.Infrastructure.HttpHandlers;
using GlobalStable.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<CallbackSettings>(builder.Configuration.GetSection("CallbackSettings"));

builder.Services.AddAuthentication(ApiKeyAuthenticationHandler.Scheme)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.Scheme, _ => {});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SameCustomer", policy =>
        policy.AddRequirements(new SameCustomerRequirement())
            .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.Scheme)
            .RequireAuthenticatedUser());
});

builder.Services.AddSingleton<IAuthorizationHandler, SameCustomerHandler>();

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
