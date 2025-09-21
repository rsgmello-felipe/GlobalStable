using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

public class Currency : EntityBase
{
    public string Name { get; private set; }

    public string Code { get; private set; }

    public int Precision { get; private set; }

    public CurrencyType Type { get; private set; }
    
    public ICollection<CurrencyBlockchain> BlockchainNetworks { get; private set; }
        = new List<CurrencyBlockchain>();

    public Currency(){}
    
    public Currency(
        long id,
        string code,
        string name,
        int precision,
        CurrencyType type)
    {
        Id = id;
        Code = code;
        Name = name;
        Precision = precision;
        Type = type;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = "System";
    }
}