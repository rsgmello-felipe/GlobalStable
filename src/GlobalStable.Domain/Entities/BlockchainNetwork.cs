namespace GlobalStable.Domain.Entities;

public class BlockchainNetwork
{
    public long Id { get; private set; }
    
    public string Name { get; private set; }

    public long? NativeCurrencyId { get; private set; }
    
    public Currency? NativeCurrency { get; private set; }
    
    public ICollection<CurrencyBlockchain> SupportedCurrencies { get; private set; } = new List<CurrencyBlockchain>();

    
    public BlockchainNetwork(){}

    public BlockchainNetwork(string name, long? nativeCurrencyId = null)
    {
        Name = name;
        NativeCurrencyId = nativeCurrencyId;
    }
}