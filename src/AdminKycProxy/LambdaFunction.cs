using Flurl;
using System.Net;
using Flurl.Http;
using KYC.DataBase;
using Amazon.Lambda;
using SecretsManager;
using EnvironmentManager;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using AdminKycProxy.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminKycProxy;

public class LambdaFunction
{
    private const string LambdaFunctionName = "AdminKycProxy";
    private const int MaxRetries = 20;
    private readonly LambdaSettings lambdaSettings;
    private readonly KycDbContext context;
    private readonly AmazonLambdaClient client;

    public LambdaFunction()
        : this(new SecretManager(), new KycDbContext(), new AmazonLambdaClient())
    { }

    public LambdaFunction(SecretManager secretManager, KycDbContext context, AmazonLambdaClient client)
    {
        lambdaSettings = new LambdaSettings(secretManager);
        this.context = context;
        this.client = client;
    }

    public async Task<HttpStatusCode> RunAsync()
    {
        var skip = EnvManager.GetEnvironmentValue<int>("DOWNLOADED_TO");
        var url = new Url(lambdaSettings.Url);
        url = url.SetQueryParam("skip", skip);
        url = url.SetQueryParam("limit", MaxRetries);

        var hasMore = true;
        while (hasMore)
        {
            var response = url
                .WithHeader("Authorization", lambdaSettings.SecretApiKey)
                .WithHeader("cache-control", "no-cache")
                .GetAsync()
                .ReceiveJson<HttpResponse>()
                .GetAwaiter()
                .GetResult();

            if (response.Data.Records.Length == 0)
            {
                hasMore = false;
                skip = response.Data.Total;
                continue;
            }

            context.Users.AddRange(response.Data.Records);

            skip += MaxRetries;
            url.SetQueryParam("skip", skip);
        }

        await context.SaveChangesAsync();
        await UpdateFunctionEnvironmentsAsync(skip);

        return HttpStatusCode.OK;
    }

    private async Task UpdateFunctionEnvironmentsAsync(int downloadedTo)
    {
        var request = new UpdateFunctionConfigurationRequest
        {
            FunctionName = LambdaFunctionName,
            Environment = new Amazon.Lambda.Model.Environment
            {
                Variables = new Dictionary<string, string>
                {
                    { "DOWNLOADED_TO", downloadedTo.ToString() }
                }
            }
        };

        await client.UpdateFunctionConfigurationAsync(request);
    }
}