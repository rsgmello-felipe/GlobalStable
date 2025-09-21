using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.CustomerUseCases;

public class GetCustomerUseCase(
    ICustomerRepository customerRepository,
    ILogger<GetCustomerUseCase> logger)
{
    public async Task<Customer?> ExecuteAsync(long customerId)
    {
        var customer = await customerRepository.GetByIdAsync(customerId);

        if (customer == null)
        {
            logger.LogInformation(
                "Customer with id '{customerId}' not found.",
                customerId);
        }

        return customer;
    }
}