using GlobalStable.Domain.Common;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using Refit;

namespace GlobalStable.Infrastructure.HttpClients;

public interface ICustomerServiceClient
{
    [Get("/api/v1/customer/currencies")]
    Task<ApiResponse<BaseApiResponse<PagedResult<GetCurrencyResponse>>>> GetCurrencyAsync(
        [Query] string code,
        [Query] string includes);

    [Get("/api/v1/customer/{customerId}")]
    public Task<ApiResponse<BaseApiResponse<GetCustomerResponse>>> GetCustomerByIdAsync(long customerId);

    [Get("/api/v1/customer/{baseCustomerId}/comparDeptheWith/{toCompareCustomerId}")]
    public Task<ApiResponse<BaseApiResponse<GetCompareDepthResponse>>> CompareCustomerDepthAsync(long baseCustomerId, long toCompareCustomerId);
}