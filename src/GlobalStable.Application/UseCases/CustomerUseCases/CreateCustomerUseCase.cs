using FluentResults;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.CustomerUseCases
{
    public class CreateCustomerUseCase(
        ICustomerRepository customerRepository,
        ILogger<CreateCustomerUseCase> logger
        )
    {
        public async Task<Result<Customer>> ExecuteAsync(CreateCustomerRequest request)
        {
            var existingCustomer = await customerRepository.GetByTaxIdAsync(request.TaxId, request.Country);
            if (existingCustomer != null)
            {
                logger.LogInformation("Customer already exists.");
                return Result.Fail("Customer already exists.");
            }

            var customer = new Customer(
                request.Name,
                request.TaxId,
                request.Country,
                request.QuoteSpread);

            await customerRepository.AddAsync(customer);
            return customer;
        }
    }
}