using System.ComponentModel.DataAnnotations;

namespace GlobalStable.Application.ApiRequests;

/// <summary>
/// Represents the request for creating a deposit order.
/// </summary>
public class CreateDepositOrderRequest
{
    /// <summary>
    /// The amount to be deposited.
    /// </summary>
    [Range(typeof(decimal), "1", "9999999999999999", ErrorMessage = "The amount must be greater than zero.")]
    public required decimal Amount { get; set; }

    /// <summary>
    /// The name in which the deposit is made.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Expiration in seconds
    /// </summary>
    public int Expiration { get; set; } = 300;

    /// <summary>
    /// Customized webhook url.
    /// </summary>
    public string WebhookUrl { get; set; }

    /// <summary>
    /// The payment details associated with the deposit.
    /// </summary>
    public string? PayerTaxId { get; set; }
}