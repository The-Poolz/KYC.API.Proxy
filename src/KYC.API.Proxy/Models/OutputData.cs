using Newtonsoft.Json;

namespace KYC.API.Proxy.Models;

public class OutputData
{
    [JsonProperty("status")]
    public string? RequestStatus { get; set; }

    [JsonProperty("data.status")]
    public string? Status { get; set; }

    [JsonProperty("data.identities.given_name.value")]
    public string? Name { get; set; }
}
