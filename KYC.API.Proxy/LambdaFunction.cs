using Flurl.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy;

public class LambdaFunction
{
    public LambdaSettings Settings { get; set; } = new();

    public async Task<APIGatewayProxyResponse> RunAsync(APIGatewayProxyRequest request)
    {
        var referer = request.Headers["Referer"];
        var hashHeader = request.Headers[LambdaSettings.HashHeader];

        var address = request.QueryStringParameters["address"]
            ?? throw new InvalidOperationException("Query string parameters not contain 'address' parameter.");
        var hashValue = SHA256.HashData(Encoding.UTF8.GetBytes($"{address}{Settings.ExpectedHashValue}"));

        if (!referer.Contains(LambdaSettings.DomainName) || hashHeader != Convert.ToBase64String(hashValue))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 403
            };
        }

        var content = await $"https://kyc.blockpass.org/kyc/1.0/connect/{LambdaSettings.ClientId}/refId/{address}"
            .WithHeader("Authorization", Settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<JToken>();

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = content.ToString()
        };
    }
}