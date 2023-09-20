using Newtonsoft.Json;

namespace KYC.API.Proxy.Models.HttpResponse;

public class GivenName
{
    [JsonProperty("value")]
    public string Value { get; set; } = null!;
}