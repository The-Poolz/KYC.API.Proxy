using Flurl.Http;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy;

public class LambdaFunction
{
    private readonly LambdaSettings settings = new();

    public async Task<JToken> RunAsync(JObject request)
    {
        if (!request.ContainsKey("Address") || request["Address"]?.ToString() == "0x0000000000000000000000000000000000000000")
        {
            return new JObject()
            {
                { "StatusCode", 403 }
            };
        }

        return await $"https://kyc.blockpass.org/kyc/1.0/connect/{LambdaSettings.ClientId}/refId/{request["Address"]}"
            .AllowHttpStatus("404")
            .WithHeader("Authorization", settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<JToken>();
    }
}
