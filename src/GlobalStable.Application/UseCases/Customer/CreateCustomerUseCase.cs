using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Repositories;

namespace GlobalStable.Application.UseCases.Customers
{
    public class CreateCustomerUseCase
    {
        private readonly ICustomerRepository _customerRepository;

        public CreateCustomerUseCase(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> ExecuteAsync(
            string name, 
            string document, 
            string email, 
            string phoneNumber,
            string status,
            string type)
        {
            // Verifica se já existe um customer com o mesmo documento
            var existingCustomer = await _customerRepository.GetByDocumentAsync(document);
            if (existingCustomer != null)
            {
                throw new Exception("Customer with this document already exists");
            }

            var customer = new Customer(
                name, 
                document, 
                email, 
                phoneNumber,
                status,
                type);

            await _customerRepository.AddAsync(customer);
            return customer;
        }
    }
}