using GlobalStable.Application.ApiRequests;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
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
            QuoteOrderRequest request)
        {
            var customer = await customerRepository.GetByIdAsync(customerId);

            if (!request.BaseAmount.HasValue && !request.QuoteAmount.HasValue)
            {
                throw new Exception("Either base amount or quote amount must be provided");
            }

            var quotedStatus = await orderStatusRepository.GetByNameAsync(OrderStatuses.Quoted);


            var quoteOrder = new QuoteOrder(
                customerId,
                quotedStatus.Id,
                request.BaseCurrencyId,
                request.QuoteCurrencyId,
                request.Side,
                request.BaseAmount,
                request.QuoteAmount,
                0,
                1.001M,
                request.BaseAccountId,
                request.QuoteAccountId
            );

            await quoteOrderRepository.AddAsync(quoteOrder);
            return quoteOrder;
        }
    }
}