namespace GlobalStable.Domain.Entities;

public class CurrencyBlockchain : EntityBase
{
    public long CurrencyId { get; private set; }

    public long BlockchainNetworkId { get; private set; }
    
    public Currency Currency { get; private set; }
    
    public BlockchainNetwork BlockchainNetwork { get; private set; }

    public CurrencyBlockchain(){}
    
    public CurrencyBlockchain(long currencyId, long blockchainNetworkId)
    {
        CurrencyId = currencyId;
        BlockchainNetworkId = blockchainNetworkId;
    }
}