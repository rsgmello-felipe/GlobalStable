using GlobalStable.Domain.Repositories;

namespace GlobalStable.Application.UseCases.Customers
{
    public class DeleteCustomerUseCase
    {
        private readonly ICustomerRepository _customerRepository;

        public DeleteCustomerUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task ExecuteAsync(long customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new Exception("Customer not found");
            }

            // Verificar se o customer pode ser deletado (por exemplo, se não tem relacionamentos ativos)
            // Aqui você pode adicionar validações específicas do seu negócio

            await _customerRepository.RemoveAsync(customer);
        }
    }
}