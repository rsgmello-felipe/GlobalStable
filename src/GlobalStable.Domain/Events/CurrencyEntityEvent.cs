using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Events;

public class CurrencyEntityEvent
{
    public long Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public int Precision { get; set; }

    public CurrencyType Type { get; set; }
}