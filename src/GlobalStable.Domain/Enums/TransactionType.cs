using System.Text.Json.Serialization;

namespace GlobalStable.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    Credit = 1,
    Debit = 2,
}