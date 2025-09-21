using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using GlobalStable.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

public class CurrencyBlockchainRepository(ServiceDbContext context)
    : Repository<CurrencyBlockchain>(context), ICurrencyBlockchainRepository
{
    public async Task<IEnumerable<CurrencyBlockchain>> GetByBlockchainNetworkIdAsync(long blockchainNetworkId)
    {
        return await context.CurrencyBlockchains
            .AsNoTracking()
            .Include(x => x.Currency)
            .Include(x => x.BlockchainNetwork)
            .Where(x => x.BlockchainNetworkId == blockchainNetworkId)
            .OrderBy(x => x.Currency.Code)
            .ToListAsync();
    }
}