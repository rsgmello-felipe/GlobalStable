using FluentResults;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.QuoteOrderUseCases
{
    public class AcceptQuoteOrderUseCase(
        IQuoteOrderRepository quoteOrderRepository,
        IAccountRepository accountRepository,
        IOrderStatusRepository orderStatusRepository,
        ILogger<AcceptQuoteOrderUseCase> logger)
    {
        public async Task<Result<QuoteOrder>> ExecuteAsync(long quoteOrderId)
        {
            var quoteOrder = await quoteOrderRepository.GetByIdAsync(quoteOrderId)
                ?? throw new Exception("Quote order not found");

            var quotedStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.Quoted);

            if (quoteOrder.StatusId != quotedStatus!.Id)
            {
                logger.LogInformation("Quote order cannot be accepted in its current status");
                return Result.Fail("Quote order cannot be accepted in its current status");
            }

            var acceptedStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.Accepted);

            quoteOrder.Accept(
                acceptedStatus!.Id,
                "Quote order accepted by user.");

            await quoteOrderRepository.UpdateAsync(quoteOrder);
            return quoteOrder;
        }
    }
}