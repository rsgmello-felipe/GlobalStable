using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;

namespace GlobalStable.Application.UseCases.CustomerUseCases;

public interface ICustomerApiKeyRepository : IRepository<CustomerApiKey>
{
}