using Flurl.Http;
using EnvironmentManager.Extensions;
using KYC.API.Proxy.Models.HttpResponse;

namespace KYC.API.Proxy.Utils;

public class HttpCall(LambdaSettings settings)
{
    public HttpCall()
        : this(new LambdaSettings())
    { }

    public virtual Response GetBlockPassResponse(string address) => settings.Url(address)
        .AllowHttpStatus("404")
        .WithHeader("Authorization", Env.API_KEY.GetRequired())
        .WithHeader("cache-control", "no-cache")
        .GetAsync()
        .ReceiveJson<Response>()
        .GetAwaiter()
        .GetResult();  
}