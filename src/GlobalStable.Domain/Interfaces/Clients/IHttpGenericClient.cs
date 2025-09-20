using Refit;

namespace GlobalStable.Domain.Interfaces.Clients;

public interface IHttpGenericClient
{
    [Get("/api/dados")]
    Task<ApiResponse<string>> ObterDados();
}