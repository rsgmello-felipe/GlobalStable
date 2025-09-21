using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

public interface IBlockchainNetworkRepository : IRepository<BlockchainNetwork>
{
    Task<BlockchainNetwork?> GetByNameAsync(string name);
}