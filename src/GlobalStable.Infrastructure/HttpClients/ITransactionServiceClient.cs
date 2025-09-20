using GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using Refit;

namespace GlobalStable.Infrastructure.HttpClients;

public interface ITransactionServiceClient
{
    [Delete("/api/v1/customer/{customerId}/transaction/pendingTransaction/orderId/{orderId}")]
    Task<ApiResponse<BaseApiResponse<DeletePendingTransactionResponse>>> DeletePendingTransactionAsync(
        long customerId,
        long accountId,
        long orderId);

    [Post("/api/v1/customer/{customerId}/transaction/account/{accountId}/pendingTransaction")]
    Task<ApiResponse<BaseApiResponse<CreatePendingTransactionResponse>>> CreatePendingTransactionAsync(
        [Body] CreatePendingTransactionRequest request,
        long customerId,
        long accountId);

    [Get("/api/v1/customer/{customerId}/balance/account/{accountId}")]
    Task<ApiResponse<BaseApiResponse<GetBalanceResponse>>> GetBalanceAsync(
        long customerId,
        long accountId);
}