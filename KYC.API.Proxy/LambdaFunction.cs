using Flurl.Http;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy;

public class LambdaFunction
{
    public LambdaSettings Settings { get; set; } = new();

    public async Task<APIGatewayProxyResponse> RunAsync(APIGatewayProxyRequest request)
    {
        Console.WriteLine(JsonConvert.SerializeObject(request));

        var referer = request.Headers["Referer"];

        Console.WriteLine("Referer: " + referer);

        var address = request.QueryStringParameters["address"]
            ?? throw new InvalidOperationException("Query string parameters not contain 'address' parameter.");

        if (!referer.Contains(LambdaSettings.DomainName))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 403
            };
        }

        var content = await $"https://kyc.blockpass.org/kyc/1.0/connect/{LambdaSettings.ClientId}/refId/{address}"
            .AllowHttpStatus("404")
            .WithHeader("Authorization", Settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<JToken>();

        return new APIGatewayProxyResponse
        {
            IsBase64Encoded = false,
            StatusCode = 200,
            Body = content.ToString(),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                { "Access-Control-Allow-Methods", "GET" },
                { "Access-Control-Allow-Origin", LambdaSettings.DomainName }
            }
        };
    }
}