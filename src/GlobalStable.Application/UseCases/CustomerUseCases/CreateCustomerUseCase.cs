using System.Security.Cryptography;
using System.Text;
using FluentResults;
using GlobalStable.Application.ApiRequests;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GlobalStable.Application.UseCases.CustomerUseCases
{
    public class CreateCustomerUseCase(
        ICustomerRepository customerRepository,
        ICustomerApiKeyRepository apiKeyRepository,
        IConfiguration configuration,
        ILogger<CreateCustomerUseCase> logger
        )
    {
        public async Task<Result<CreateCustomerResponse>> ExecuteAsync(CreateCustomerRequest request)
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

            var apiKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            var pepper = configuration["Security:ApiKeyPepper"] ?? string.Empty;
            var hash = ComputeSha256(apiKey + pepper);

            var customerApiKey = new CustomerApiKey(customer.Id, hash);
            await apiKeyRepository.AddAsync(customerApiKey);

            await customerRepository.AddAsync(customer);
            var response = new CreateCustomerResponse(
                customer.Id,
                customer.Name,
                customer.TaxId,
                customer.Country,
                customer.QuoteSpread,
                apiKey);
            return Result.Ok(response);
        }

        private static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}