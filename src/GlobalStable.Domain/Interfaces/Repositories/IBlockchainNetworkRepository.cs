using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

public interface IBlockchainNetworkRepository
{
    Task<BlockchainNetwork> AddAsync(BlockchainNetwork entity);
    Task<BlockchainNetwork> GetByIdAsync(long id);
    Task<IEnumerable<BlockchainNetwork>> GetAllAsync();
    Task<BlockchainNetwork> UpdateAsync(BlockchainNetwork entity);
    Task RemoveAsync(BlockchainNetwork entity);
}