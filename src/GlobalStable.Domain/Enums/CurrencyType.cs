using System.Text.Json.Serialization;

namespace GlobalStable.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CurrencyType
{
    Fiat = 1,
    Crypto = 2,
}