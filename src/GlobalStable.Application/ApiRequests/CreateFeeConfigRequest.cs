using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Application.ApiRequests;

public class CreateFeeConfigRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required TransactionOrderType TransactionOrderType { get; set; }

    /// <summary>
    /// Percentage fee in decimal format. Ex: 0.015 = 1.5%
    /// </summary>
    [Range(typeof(decimal), "0", "1", ErrorMessage = "FeePercentage must be between 0 (0%) and 1 (100%).")]
    public required decimal FeePercentage { get; set; }

    /// <summary>
    /// Flat fee in absolute value. Ex: 5.00 = R$5,00
    /// </summary>
    [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "FlatFee cannot be negative.")]
    public required decimal FlatFee { get; set; }
}