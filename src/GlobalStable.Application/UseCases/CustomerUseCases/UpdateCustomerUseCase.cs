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
        public async Task<Result<Customer>> ExecuteAsync(UpdateCustomerRequest request)
        {
            var customer = await customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                logger.LogInformation(
                    "Customer with id '{customerId}' not found.",
                    request.CustomerId);
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