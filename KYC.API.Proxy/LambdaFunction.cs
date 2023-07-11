using Flurl.Http;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy;

public class LambdaFunction
{
    public LambdaSettings Settings { get; set; } = new();

    public async Task<JToken> RunAsync(JToken request)
    {
        Console.WriteLine(JsonConvert.SerializeObject(request));

        var content = await $"https://kyc.blockpass.org/kyc/1.0/connect/{LambdaSettings.ClientId}/refId/{"GET_ADDRESS_FROM_TOKEN_HERE"}"
            .AllowHttpStatus("404")
            .WithHeader("Authorization", Settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<JToken>();

        return content;
    }
}