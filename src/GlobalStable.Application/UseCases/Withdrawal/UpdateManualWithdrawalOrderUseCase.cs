using GlobalStable.Application.ApiRequests;
using GlobalStable.Application.ApiResponses;
using GlobalStable.Domain.Helpers;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.Withdrawal;

public class UpdateManualWithdrawalOrderUseCase(
    IWithdrawalOrderRepository withdrawalOrderRepository,
    IOrderStatusRepository orderStatusRepository,
    IOrderEventPublisher orderEventPublisher,
    ICustomerServiceClient customerServiceClient,
    ILogger<UpdateManualWithdrawalOrderUseCase> logger)
{
    public async Task<Result<WithdrawalOrderResponse>> ExecuteAsync(
        UpdateManualWithdrawalOrderRequest request,
        string username)
    {
        try
        {
            var withdrawalOrder = await withdrawalOrderRepository.GetByIdWithAccountAsync(request.OrderId, request.AccountId);
            if (withdrawalOrder == null)
            {
                logger.LogError("WithdrawalOrder not found. WithdrawalOrderId: {Id}", request.OrderId);
                return Result.Fail("WithdrawalOrder not found");
            }

            var newStatus = await orderStatusRepository.GetByNameAsync(request.Status);

            if (newStatus == null)
            {
                logger.LogError("Status not found. Status: {Status}", request.Status);
                return Result.Fail("Status not found");
            }

            if (!OrderStateValidator.ValidManualWithdrawalTransition(withdrawalOrder.StatusId, newStatus.Id))
            {
                logger.LogCritical(
                    "Invalid status transition from {CurrentStatusId} to {NewStatusId} for WithdrawalOrderId {Id}",
                    withdrawalOrder.StatusId,
                    newStatus.Id,
                    withdrawalOrder.Id);

                return Result.Fail($"Invalid transition from status {withdrawalOrder.StatusId} to {newStatus.Id}");
            }

            if (!await ValidCustomerAsync(request.RequestingCustomerId, withdrawalOrder.CustomerId))
            {
                logger.LogWarning(
                    "Unauthorized customer {RequestingCustomerId} attempted to update WithdrawalOrder {OrderId}",
                    request.RequestingCustomerId,
                    request.OrderId);
                return Result.Fail("Unauthorized customer");
            }

            withdrawalOrder.UpdateBankTransactionInformation(
                null,
                request.Reason,
                request.TransactionHash);

            withdrawalOrder.UpdateStatus(newStatus, username, request.Reason);
            await withdrawalOrderRepository.UpdateAsync(withdrawalOrder);

            await orderEventPublisher.PublishWithdrawalOrderEvent(withdrawalOrder);

            var statuses = await orderStatusRepository.GetAllAsDictionaryAsync();
            var response = new WithdrawalOrderResponse(withdrawalOrder, withdrawalOrder.Currency.Code, statuses);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error processing withdrawal status update for WithdrawalOrderId {Id}", request.OrderId);
            return Result.Fail($"Error processing withdrawal status update: {ex.Message}");
        }
    }

    private async Task<bool> ValidCustomerAsync(long requestingCustomerId, long orderCustomerId)
    {
        var requestingCustomerResult = await customerServiceClient.GetCustomerByIdAsync(requestingCustomerId);
        var requestingCustomer = requestingCustomerResult?.Content?.Result;
        if (requestingCustomerResult == null || !requestingCustomerResult.IsSuccessful || requestingCustomer == null)
        {
            logger.LogWarning(
                "Failed to fetch customer details for CustomerId: {CustomerId}. Error: {Error}",
                requestingCustomerId,
                requestingCustomerResult?.Error?.Content ?? requestingCustomerResult?.Error?.Message);
            return false;
        }

        if (requestingCustomer.ChildDepth.Equals(0)) return true;   // For institutional customers, no further checks needed
        if (requestingCustomerId == orderCustomerId) return false;      // The customer that created the order cannot accept it

        var compareDepthResult = await customerServiceClient.CompareCustomerDepthAsync(requestingCustomerId, orderCustomerId);
        var compareDepth = compareDepthResult.Content?.Result;
        if (compareDepthResult == null || !compareDepthResult.IsSuccessful || compareDepth == null)
        {
            logger.LogWarning(
                "Failed to compare customer depth for CustomerId: {CustomerId}. Error: {Error}",
                requestingCustomerId,
                compareDepthResult?.Error?.Content ?? compareDepthResult?.Error?.Message);
            return false;
        }

        // -1 means that the depth of requesting customer is lesser than the order customer
        // Lesser depth means that the customer is from a higher hierarchy level
        return compareDepth.DepthCompare.Equals(-1);
    }
}
