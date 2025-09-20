using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalStable.Domain.Entities;

public class BlockchainNetwork : EntityBase
{
    public string Name { get; private set; }

    public long? NativeCurrencyId { get; private set; }

    public BlockchainNetwork(string name, long? nativeCurrencyId = null)
    {
        Name = name;
        NativeCurrencyId = nativeCurrencyId;
    }
}