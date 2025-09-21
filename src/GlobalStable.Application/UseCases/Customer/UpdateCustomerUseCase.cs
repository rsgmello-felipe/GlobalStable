using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Repositories;

namespace GlobalStable.Application.UseCases.Customers
{
    public class UpdateCustomerUseCase
    {
        private readonly ICustomerRepository _customerRepository;

        public UpdateCustomerUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> ExecuteAsync(
            long customerId,
            string name,
            string email,
            string phoneNumber,
            string status,
            string type)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new Exception("Customer not found");
            }

            customer.Update(
                name,
                email,
                phoneNumber,
                status,
                type);

            await _customerRepository.UpdateAsync(customer);
            return customer;
        }
    }
}