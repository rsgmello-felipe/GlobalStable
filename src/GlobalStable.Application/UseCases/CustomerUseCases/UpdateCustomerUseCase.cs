using FluentResults;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.CustomerUseCases
{
    public class UpdateCustomerUseCase(
        ICustomerRepository customerRepository,
        ILogger<UpdateCustomerUseCase> logger
        )
    {
        public async Task<Result<Customer>> ExecuteAsync(
            long customerId,
            UpdateCustomerRequest request)
        {
            var customer = await customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                logger.LogInformation(
                    "Customer with id '{customerId}' not found.",
                    customerId);
                return Result.Fail("Customer not found.");
            }

            customer.Update(
                request.Name,
                request.QuoteSpread,
                request.Enabled);

            await customerRepository.UpdateAsync(customer);
            return customer;
        }
    }
}