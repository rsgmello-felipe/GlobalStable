using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Repositories;
using GlobalStable.Domain.Enums;
using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.QuoteOrders
{
    public class AcceptQuoteOrderUseCase
    {
        private readonly IQuoteOrderRepository _quoteOrderRepository;
        private readonly IAccountRepository _accountRepository;

        public AcceptQuoteOrderUseCase(
            IQuoteOrderRepository quoteOrderRepository,
            IAccountRepository accountRepository)
        {
            _quoteOrderRepository = quoteOrderRepository;
            _accountRepository = accountRepository;
        }

        public async Task<QuoteOrder> ExecuteAsync(
            long quoteOrderId, 
            decimal price,
            decimal feeAmount,
            string updatedBy)
        {
            var quoteOrder = await _quoteOrderRepository.GetByIdAsync(quoteOrderId)
                ?? throw new Exception("Quote order not found");

            // Validar se o quote order está em um estado que permite aceitação
            if (quoteOrder.StatusId != (int)OrderStatus.Pending)
            {
                throw new Exception("Quote order cannot be accepted in its current status");
            }

            // Validar se há saldo suficiente nas contas
            var baseAccount = await _accountRepository.GetByIdAsync(quoteOrder.BaseAccountId);
            var quoteAccount = await _accountRepository.GetByIdAsync(quoteOrder.QuoteAccountId);

            if (quoteOrder.Side == OrderSide.Buy)
            {
                var totalQuoteAmount = (quoteOrder.BaseAmount * price) + feeAmount;
                if (quoteAccount.Balance < totalQuoteAmount)
                {
                    throw new Exception("Insufficient quote currency balance");
                }
            }
            else // Sell
            {
                if (baseAccount.Balance < quoteOrder.BaseAmount)
                {
                    throw new Exception("Insufficient base currency balance");
                }
            }

            // Atualizar o quote order
            quoteOrder.Accept(
                price,
                feeAmount,
                updatedBy);

            await _quoteOrderRepository.UpdateAsync(quoteOrder);
            return quoteOrder;
        }
    }
}