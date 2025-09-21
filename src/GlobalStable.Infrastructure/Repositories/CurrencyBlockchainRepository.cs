using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.HttpClients.ApiResponses;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class CurrencyBlockchainRepository : ICurrencyBlockchainRepository
{
    private readonly ServiceDbContext _context;

    public CurrencyBlockchainRepository(ServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyBlockchain> AddAsync(CurrencyBlockchain entity)
    {
        await _context.CurrencyBlockchains.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<CurrencyBlockchain>> GetByBlockchainNetworkIdAsync(long blockchainNetworkId)
    {
        return await _context.CurrencyBlockchains
            .Include(x => x.Currency)
            .Where(x => x.BlockchainNetworkId == blockchainNetworkId)
            .ToListAsync();
    }
}