using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories
{
    public class BlockchainNetworkRepository(ServiceDbContext context) 
        : Repository<BlockchainNetwork>(context), IBlockchainNetworkRepository
    {
        public async Task<List<BlockchainNetwork?>> GetAllAsync()
        {
            return await context.BlockchainNetworks
                .ToListAsync();
        }

        public async Task<BlockchainNetwork?> GetByNameAsync(string name)
        {
            return await context.BlockchainNetworks
                .FirstOrDefaultAsync(bn => bn.Name.ToLower() == name.ToLower());
        }
    }
}