using System.Text.Json.Serialization;

namespace GlobalStable.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionOrderType
{
    Deposit = 1,
    DepositFee = 2,
    Withdrawal = 3,
    WithdrawalFee = 4,
    Trade = 5,
    TradeFee = 6,
    DepositReturned = 7,
    WithdrawalReturned = 8,
}