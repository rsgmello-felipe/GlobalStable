namespace GlobalStable.Domain.Entities;

public class BlockchainNetwork
{
    public long Id { get; private set; }
    
    public string Name { get; private set; }

    public long? NativeCurrencyId { get; private set; }
    
    public BlockchainNetwork(){}

    public BlockchainNetwork(string name, long? nativeCurrencyId = null)
    {
        Name = name;
        NativeCurrencyId = nativeCurrencyId;
    }
}