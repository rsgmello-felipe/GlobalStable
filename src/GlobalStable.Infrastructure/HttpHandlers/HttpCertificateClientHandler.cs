using System.Security.Cryptography.X509Certificates;

namespace GlobalStable.Infrastructure.HttpHandlers
{
    public class HttpCertificateClientHandler : HttpClientHandler
    {
        public HttpCertificateClientHandler()
        {
            var certificate = new X509Certificate2("caminho-do-certificado.pfx", "senha-do-certificado");

            ClientCertificates.Add(certificate);
        }
    }
}