using GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using GlobalStable.Infrastructure.HttpClients.ApiResponses.BgpConnector;
using Refit;

namespace GlobalStable.Infrastructure.HttpClients;

public interface IBrlProviderClient
{
    [Post("/api/v1/withdrawal")]
    Task<ApiResponse<BaseApiResponse<BrlProviderCreateWithdrawalResponse>>> CreateWithdrawalAsync(
        [Body] BrlProviderCreateWithdrawalRequest request);

    [Post("/api/v1/deposit")]
    Task<ApiResponse<BaseApiResponse<BrlProviderCreateDepositResponse>>> CreateDepositOrderAsync(
        [Body] BrlProviderCreateDepositRequest request);
}