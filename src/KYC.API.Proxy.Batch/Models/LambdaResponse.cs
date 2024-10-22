using KYC.API.Proxy.Models;

namespace KYC.API.Proxy.Batch.Models;

public class LambdaResponse(string address, string requestStatus, string? status, string? proxy)
{
    public LambdaResponse(string address, OutputData data)
        : this(address, data.RequestStatus.ToString(), data.Status, data.Proxy)
    { }

    public string Address { get; } = address;
    public string RequestStatus { get; } = requestStatus;
    public string? Status { get; } = status;
    public string? Proxy { get; } = proxy;
}
