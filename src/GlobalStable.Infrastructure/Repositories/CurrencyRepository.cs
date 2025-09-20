using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories;

public class CurrencyRepository(ServiceDbContext dbContext)
    : Repository<Currency>(dbContext), ICurrencyRepository
{
    public async Task<Currency?> GetByCodeAsync(string code)
    {
        return await dbContext.Currencies.FirstOrDefaultAsync(c => c.Code == code);
    }
}