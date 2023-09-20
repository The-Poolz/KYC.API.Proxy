using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KYC.API.Proxy.Models;

public class OutputData
{
    [JsonConverter(typeof(StringEnumConverter))]
    public RequestStatus RequestStatus { get; set; }
    public string? Status { get; set; }
    public string? Name { get; set; }
}