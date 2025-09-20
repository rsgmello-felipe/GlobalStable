using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;

namespace GlobalStable.API.DependencyInjection;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate(options =>
            {
                options.AllowedCertificateTypes = CertificateTypes.All;
                options.RevocationMode = X509RevocationMode.NoCheck;
                options.ValidateCertificateUse = true;
                options.ValidateValidityPeriod = true;
                options.Events = new CertificateAuthenticationEvents
                {
                    OnCertificateValidated = context =>
                    {
                        var cert = context.ClientCertificate;
                        if (cert == null || !cert.Subject.Contains("CN=MinhaEmpresa"))
                        {
                            context.Fail("Certificado inválido.");
                            return Task.CompletedTask;
                        }

                        context.Success();
                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }
}