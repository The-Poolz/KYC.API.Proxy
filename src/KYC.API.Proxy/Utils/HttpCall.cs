﻿using Flurl.Http;
using KYC.API.Proxy.Models.HttpResponse;

namespace KYC.API.Proxy.Utils;

public class HttpCall
{
    private readonly LambdaSettings settings;

    public HttpCall()
        : this(new LambdaSettings())
    { }

    public HttpCall(LambdaSettings settings)
    {
        this.settings = settings;
    }

    public virtual Response GetBlockPassResponse(string address) =>
             settings.Url(address)
            .AllowHttpStatus("404")
            .WithHeader("Authorization", settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<Response>()
            .GetAwaiter()
            .GetResult();  
}