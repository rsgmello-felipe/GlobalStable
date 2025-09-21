using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

public interface ICurrencyBlockchainRepository
{
    Task<CurrencyBlockchain> AddAsync(CurrencyBlockchain entity);
    Task<IEnumerable<CurrencyBlockchain>> GetByBlockchainNetworkIdAsync(long blockchainNetworkId);
    Task<CurrencyBlockchain> GetByIdAsync(long id);
    Task RemoveAsync(CurrencyBlockchain entity);
}