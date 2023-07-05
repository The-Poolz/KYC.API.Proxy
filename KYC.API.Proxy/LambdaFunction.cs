using Flurl.Http;
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
        var referer = request.Headers["Referer"];
        var hashHeader = request.Headers[LambdaSettings.HashHeader];

        if (!referer.Contains(LambdaSettings.DomainName) || hashHeader != Settings.ExpectedHashValue)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 403
            };
        }

        var address = request.QueryStringParameters["address"]
            ?? throw new InvalidOperationException("Query string parameters not contain 'address' parameter.");

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