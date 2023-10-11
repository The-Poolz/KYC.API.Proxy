using Flurl;
using System.Net;
using Flurl.Http;
using KYC.DataBase;
using SecretsManager;
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
        var skip = 0;
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
                continue;
            }

            context.Users.AddRange(response.Data.Records);

            skip += MaxRetries;
            url.SetQueryParam("skip", 0);
        }

        await context.SaveChangesAsync();

        return HttpStatusCode.OK;
    }
}