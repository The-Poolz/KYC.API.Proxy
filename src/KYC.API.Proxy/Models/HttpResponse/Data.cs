using Newtonsoft.Json;

namespace KYC.API.Proxy.Models.HttpResponse;

public class Data
{
    [JsonProperty("status")]
    public string Status { get; set; } = null!;
}