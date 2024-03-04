using Flurl;
using System.Net;
using Flurl.Http;
using KYC.DataBase;
using SecretsManager;
using EnvironmentManager;
using Amazon.Lambda.Core;
using AdminKycProxy.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminKycProxy;

public class LambdaFunction
{
    private const int MaxRetries = 20;
    private readonly LambdaSettings lambdaSettings;
    private readonly KycDbContext context;

    public LambdaFunction()
        : this(new SecretManager(), new KycDbContext())
    { }

    public LambdaFunction(SecretManager secretManager, KycDbContext context)
    {
        lambdaSettings = new LambdaSettings(secretManager);
        this.context = context;
    }

    public async Task<HttpStatusCode> RunAsync()
    {
        var skip = new EnvManager().GetEnvironmentValue<int>("DOWNLOADED_FROM", true);
        var url = lambdaSettings.Url
                .SetQueryParam("skip", skip)
                .SetQueryParam("limit", MaxRetries);

        var hasMore = true;
        while (hasMore)
        {
            var response = await url
                .WithHeader("Authorization", lambdaSettings.SecretApiKey)
                .WithHeader("cache-control", "no-cache")
                .GetJsonAsync<HttpResponse>();

            if (response.Data.Records.Length == 0)
            {
                hasMore = false;
                skip = response.Data.Total;
                continue;
            }

            context.Users.AddRange(response.Data.Records.Where(x =>
                !context.Users.Contains(x)));

            skip += MaxRetries;
            url.SetQueryParam("skip", skip);
        }

        await context.SaveChangesAsync();

        return HttpStatusCode.OK;
    }
}