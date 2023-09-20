﻿using Flurl.Http;
using Newtonsoft.Json.Linq;

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

    public virtual JObject GetBlockPassResponse(string address) =>
        $"https://kyc.blockpass.org/kyc/1.0/connect/{LambdaSettings.ClientId}/refId/{address}"
            .AllowHttpStatus("404")
            .WithHeader("Authorization", settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<JObject>()
            .GetAwaiter()
            .GetResult();
}