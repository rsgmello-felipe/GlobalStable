using System.ComponentModel.DataAnnotations;
using GlobalStable.Domain.Entities;

namespace GlobalStable.Application.ApiRequests;

/// <summary>
/// Represents the request for creating a withdrawal order.
/// </summary>
public class CreateWithdrawalOrderRequest
{
    /// <summary>
    /// Gets or sets the amount to be withdrawn.
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the receiver account id.
    /// </summary>
    public string? ReceiverTaxId { get; set; }

    /// <summary>
    /// Gets or sets the receiver account key.
    /// </summary>
    public string? ReceiverAccountKey { get; set; }

    /// <summary>
    /// Gets or sets the receiver wallet address.
    /// </summary>
    public string? ReceiverWalletAddress { get; set; }

    /// <summary>
    /// Gets or sets the receiver blockchain.
    /// </summary>
    public string? ReceiverBlockchain { get; set; }

    /// <summary>
    /// Gets or sets the payment details associated with the withdrawal.
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the webhook url.
    /// </summary>
    public string? WebhookUrl { get; set; }
}