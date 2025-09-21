using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Repositories;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Application.UseCases.QuoteOrders
{
    public class RequestQuoteOrderUseCase
    {
        private readonly IQuoteOrderRepository _quoteOrderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IAccountRepository _accountRepository;

        public RequestQuoteOrderUseCase(
            IQuoteOrderRepository quoteOrderRepository,
            ICustomerRepository customerRepository,
            ICurrencyRepository currencyRepository,
            IAccountRepository accountRepository)
        {
            _quoteOrderRepository = quoteOrderRepository;
            _customerRepository = customerRepository;
            _currencyRepository = currencyRepository;
            _accountRepository = accountRepository;
        }

        public async Task<QuoteOrder> ExecuteAsync(
            long customerId,
            long baseCurrencyId,
            long quoteCurrencyId,
            OrderSide side,
            decimal? baseAmount,
            decimal? quoteAmount,
            long baseAccountId,
            long quoteAccountId)
        {
            // Validações
            var customer = await _customerRepository.GetByIdAsync(customerId)
                ?? throw new Exception("Customer not found");

            var baseCurrency = await _currencyRepository.GetByIdAsync(baseCurrencyId)
                ?? throw new Exception("Base currency not found");

            var quoteCurrency = await _currencyRepository.GetByIdAsync(quoteCurrencyId)
                ?? throw new Exception("Quote currency not found");

            var baseAccount = await _accountRepository.GetByIdAsync(baseAccountId)
                ?? throw new Exception("Base account not found");

            var quoteAccount = await _accountRepository.GetByIdAsync(quoteAccountId)
                ?? throw new Exception("Quote account not found");

            // Validar se pelo menos um dos valores (base ou quote) foi fornecido
            if (!baseAmount.HasValue && !quoteAmount.HasValue)
            {
                throw new Exception("Either base amount or quote amount must be provided");
            }

            var quoteOrder = new QuoteOrder(
                customerId,
                (int)QuoteOrderStatus.Pending, // Status inicial como Pending
                "Quote requested", // Descrição inicial
                baseCurrencyId,
                quoteCurrencyId,
                side,
                baseAmount,
                quoteAmount,
                null, // Price será definido quando aceito
                null, // FeeAmount será definido quando aceito
                baseAccountId,
                quoteAccountId,
                DateTime.UtcNow,
                null // LastUpdatedBy opcional no momento da criação
            );

            await _quoteOrderRepository.AddAsync(quoteOrder);
            return quoteOrder;
        }
    }
}