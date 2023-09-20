using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KYC.API.Proxy.Models.HttpResponse;

public class Response
{
    [JsonProperty("status")]
    [JsonConverter(typeof(StringEnumConverter))]
    public RequestStatus Status { get; set; }

    [JsonProperty("data")]
    public Data Data { get; set; } = new();
}