using Newtonsoft.Json;

namespace KYC.API.Proxy.Models.HttpResponse;

public class Identities
{
    [JsonProperty("given_name")]
    public GivenName GivenName { get; set; } = null!;
}