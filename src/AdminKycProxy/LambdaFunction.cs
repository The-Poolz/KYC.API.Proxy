using Flurl;
using Flurl.Http;
using SecretsManager;
using Amazon.Lambda.Core;
using AdminKycProxy.Models;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminKycProxy;

public class LambdaFunction
{
    private readonly LambdaSettings lambdaSettings;

    public LambdaFunction()
        : this(new SecretManager())
    { }

    public LambdaFunction(SecretManager secretManager)
    {
        lambdaSettings = new LambdaSettings(secretManager);
    }

    public JObject Run(InputData input)
    {
        var url = new Url(lambdaSettings.Url);
        if (!string.IsNullOrWhiteSpace(input.Status))
            url = url.AppendPathSegment(input.Status);
        if (input.Skip.HasValue)
            url = url.SetQueryParam("skip", input.Skip);
        if (input.Limit.HasValue)
            url = url.SetQueryParam("limit", input.Limit);

        var response = url
            .WithHeader("Authorization", lambdaSettings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<JObject>()
            .GetAwaiter()
            .GetResult();

        return response;
    }
}