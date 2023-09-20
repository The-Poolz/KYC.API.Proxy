using Newtonsoft.Json;

namespace KYC.API.Proxy.Models.HttpResponse;

public class Response
{
    [JsonProperty("status")]
    public string Status { get; set; } = null!;

    [JsonProperty("data")]
    public Data Data { get; set; } = null!;
}