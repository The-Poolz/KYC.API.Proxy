using KYC.API.Proxy.Models.HttpResponse;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KYC.API.Proxy.Models;

public class OutputData
{
    internal OutputData() { }
    public OutputData(Response response, string? proxy = null)
    {
        RequestStatus = response.Status;
        Status = response.Data.Status;
        Name = response.Data.Identities.GivenName.Value;
        Proxy = proxy;
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public RequestStatus RequestStatus { get; set; }
    public string? Status { get; set; }
    public string? Name { get; set; }
    public string? Proxy { get; set; }
    public static OutputData Error => new()
    {
        RequestStatus = RequestStatus.error
    };
}