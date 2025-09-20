using GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;
using GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using GlobalStable.Infrastructure.HttpClients.ApiResponses.BgpConnector;
using Refit;

namespace GlobalStable.Infrastructure.HttpClients;

public interface IBgpConnectorClient
{
    [Post("/api/v1/withdrawal")]
    Task<ApiResponse<BaseApiResponse<BgpCreateWithdrawalResponse>>> CreateWithdrawalAsync(
        [Body] BgpCreateWithdrawalRequest request);

    [Post("/api/v1/deposit")]
    Task<ApiResponse<BaseApiResponse<BgpCreateDepositResponse>>> CreateDepositOrderAsync(
        [Body] BgpCreateDepositRequest request);

    [Post("/api/v1/deposit/refund/{orderId}")]
    Task<ApiResponse<BaseApiResponse<dynamic>>> RefundDepositAsync(
        long orderId);
}