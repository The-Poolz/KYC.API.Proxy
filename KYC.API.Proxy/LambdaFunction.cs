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

    public async Task<JToken> RunAsync(JToken request)
    {
        Console.WriteLine(JsonConvert.SerializeObject(request));

        var userAddress = await CallSqlQueryExecutorLambdaAsync("PASTE_USER_TOKEN_HERE");

        var content = await $"https://kyc.blockpass.org/kyc/1.0/connect/{LambdaSettings.ClientId}/refId/{userAddress}"
            .AllowHttpStatus("404")
            .WithHeader("Authorization", Settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<JToken>();

        return content;
    }

    public async Task<string> CallSqlQueryExecutorLambdaAsync(string userToken)
    {
        var lambdaClient = new AmazonLambdaClient(RegionEndpoint.USEast1);

        var lambdaRequest = new JObject
        {
            { "DbName", "AuthDB" },
            { "Body", $"SELECT TOP(1) [Wallet] FROM [dbo].[AuthorizationTokens] WHERE UserToken = '{userToken}'" }
        };

        var request = new InvokeRequest
        {
            FunctionName = "SqlQueryExecutor",
            InvocationType = InvocationType.RequestResponse,
            LogType = LogType.None,
            Payload = $"{lambdaRequest}"
        };

        var response = await lambdaClient.InvokeAsync(request);

        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
        {
            using var sr = new StreamReader(response.Payload);
            return await sr.ReadToEndAsync();
        }

        throw new InvalidOperationException($"Failed to invoke function: {response.StatusCode}");
    }
}