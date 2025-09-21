using GlobalStable.Application.UseCases.CustomerUseCases;
using GlobalStable.Domain.Entities;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

public class CustomerApiKeyRepository(ServiceDbContext dbContext)
    : Repository<CustomerApiKey>(dbContext), ICustomerApiKeyRepository
{
}