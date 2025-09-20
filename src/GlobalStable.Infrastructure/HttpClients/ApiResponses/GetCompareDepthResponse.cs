using System.Text.Json.Serialization;

namespace GlobalStable.Infrastructure.HttpClients.ApiResponses;

public class GetCompareDepthResponse
{
    public int DepthCompare { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? LinageCompare { get; set; }
}
