using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

public interface ICurrencyBlockchainRepository : IRepository<CurrencyBlockchain>
{
    Task<IEnumerable<CurrencyBlockchain>> GetByBlockchainNetworkIdAsync(long blockchainNetworkId);
}