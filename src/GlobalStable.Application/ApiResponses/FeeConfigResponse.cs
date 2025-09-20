using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Application.ApiResponses;

/// <summary>
/// Represents the response for a fee configuration.
/// </summary>
public class FeeConfigResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the fee configuration.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the account ID associated with the fee configuration.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets the account ID associated with the fee configuration.
    /// </summary>
    public TransactionOrderType TransactionOrderType { get; set; }

    /// <summary>
    /// Gets or sets the trade fee percentage.
    /// </summary>
    public decimal FeePercentage { get; set; }

    /// <summary>
    /// Gets or sets the withdrawal fee amount.
    /// </summary>
    public decimal FlatFee { get; set; }

    public FeeConfigResponse() { }

    public FeeConfigResponse(
        FeeConfig config)
    {
        Id = config.Id;
        AccountId = config.AccountId;
        TransactionOrderType = config.TransactionOrderType;
        FeePercentage = config.FeePercentage;
        FlatFee = config.FlatFee;
    }
}