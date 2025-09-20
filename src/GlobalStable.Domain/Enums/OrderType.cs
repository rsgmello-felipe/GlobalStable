using System.Text.Json.Serialization;

namespace GlobalStable.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderType
{
    Deposit = 1,
    Withdraw = 2,
}