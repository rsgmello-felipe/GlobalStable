using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.CustomerUseCases
{
    public class DeleteCustomerUseCase(ICustomerRepository customerRepository)
    {
        private readonly ICustomerRepository _customerRepository = customerRepository;

        public async Task ExecuteAsync(long customerId)
        {
            await _customerRepository.RemoveByIdAsync(customerId);
        }
    }
}