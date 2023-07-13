using Amazon;
using Flurl.Http;
using Amazon.Lambda;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy;

public class LambdaFunction
{
    private readonly LambdaSettings Settings = new();

    public async Task<JToken> RunAsync(JObject request)
    {
        if (!request.ContainsKey("Address"))
        {
            return new JObject()
            {
                { "StatusCode", 403 }
            };
        }

        var content = await $"https://kyc.blockpass.org/kyc/1.0/connect/{LambdaSettings.ClientId}/refId/{request["Address"]}"
            .AllowHttpStatus("404")
            .WithHeader("Authorization", Settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<JToken>();

        return content;
    }
}