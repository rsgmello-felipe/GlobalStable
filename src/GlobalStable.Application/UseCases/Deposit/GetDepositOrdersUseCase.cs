using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.Deposit;

public class GetDepositOrdersUseCase(
    ILogger<GetDepositOrdersUseCase> logger,
    IDepositOrderRepository depositOrderRepository,
    IOrderStatusRepository orderStatusRepository)
{
    public async Task<Result<GetDepositOrdersResponse>> ExecuteAsync(
        long customerId,
        long? orderId,
        long? accountId,
        string? status,
        string? name,
        string? E2EId,
        string? taxId,
        DateTime? beginDate,
        DateTime? endDate,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize)
    {
        try
        {
            var allowedSortFields = new[]
            {
                "Id",
                "AccountId",
                "CustomerId",
                "Name",
                "E2eId",
                "CreatedAt",
            };

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (!allowedSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                    return Result.Fail($"Invalid sort field: {sortBy}");
            }

            var statusId = await orderStatusRepository.GetStatusIdByNameAsync(status);

            var result = await depositOrderRepository.GetFilteredAsync(
                customerId,
                orderId,
                accountId,
                statusId,
                name,
                E2EId,
                taxId,
                beginDate,
                endDate,
                sortBy,
                sortOrder,
                page,
                pageSize);

            var statuses = await orderStatusRepository.GetAllAsDictionaryAsync();

            var responses = (result.Data ?? [])
                .Select(order => new DepositOrderResponse(order, order.Currency.Code, statuses))
                .ToList();

            return Result.Ok(new GetDepositOrdersResponse(
                responses,
                result.Pagination.Page,
                result.Pagination.PageSize,
                result.Pagination.TotalItems));
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "Error fetching DepositOrders. AccountId: {AccountId}",
                accountId);
            return Result.Fail<GetDepositOrdersResponse>($"Error fetching DepositOrders: {ex.Message}");
        }
    }
}