using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.QuoteOrderUseCases
{
    public class RequestQuoteOrderUseCase(
        IQuoteOrderRepository quoteOrderRepository,
        ICustomerRepository customerRepository,
        IOrderStatusRepository orderStatusRepository)
    {
        public async Task<QuoteOrder> ExecuteAsync(
            long customerId,
            long baseCurrencyId,
            long quoteCurrencyId,
            Side side,
            decimal? baseAmount,
            decimal? quoteAmount,
            long baseAccountId,
            long quoteAccountId)
        {
            var customer = await customerRepository.GetByIdAsync(customerId);

            if (!baseAmount.HasValue && !quoteAmount.HasValue)
            {
                throw new Exception("Either base amount or quote amount must be provided");
            }
            
            var quotedStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.Quoted);

            var quoteOrder = new QuoteOrder(
                customerId,
                quotedStatus.Id,
                baseCurrencyId,
                quoteCurrencyId,
                side,
                baseAmount,
                quoteAmount,
                0,
                1.001M,
                baseAccountId,
                quoteAccountId
            );

            await quoteOrderRepository.AddAsync(quoteOrder);
            return quoteOrder;
        }
    }
}