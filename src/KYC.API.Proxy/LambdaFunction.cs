using Flurl.Http;
using Amazon.Lambda.Core;
using KYC.API.Proxy.Utils;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy;

public class LambdaFunction
{
    private readonly LambdaSettings settings;
    private readonly DynamoDb dynamoDb;

    public LambdaFunction()
        : this(new LambdaSettings(), new DynamoDb())
    { }

    public LambdaFunction(LambdaSettings settings, DynamoDb dynamoDb)
    {
        this.settings = settings;
        this.dynamoDb = dynamoDb;
    }

    public async Task<JToken> RunAsync(JObject request)
    {
        if (!request.ContainsKey("Address") || request["Address"]!.ToString() == "0x0000000000000000000000000000000000000000")
        {
            return new JObject
            {
                { "StatusCode", 403 }
            };
        }

        var address = request["Address"]!.ToString();

        var wallets = await dynamoDb.GetWalletsAsync(address);
        foreach (var wallet in wallets)
        {
            Console.WriteLine(wallet);
        }

        return await $"https://kyc.blockpass.org/kyc/1.0/connect/{LambdaSettings.ClientId}/refId/{address}"
            .AllowHttpStatus("404")
            .WithHeader("Authorization", settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<JToken>();
    }
}
