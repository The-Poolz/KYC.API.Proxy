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
    private const int limit = 20;
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
        HttpResponse? response = null;
        var skip = context.Users.Count();
        var url = lambdaSettings.Url
                .SetQueryParam("limit", limit)
                .WithHeader("Authorization", lambdaSettings.SecretApiKey)
                .WithHeader("cache-control", "no-cache");

        while (response == null || response.Data.Records.Length > 0)
        {
            response = await url
               .SetQueryParam("skip", skip)
               .GetJsonAsync<HttpResponse>();

            context.Users.AddRange(response.Data.Records.Where(x =>
                !context.Users.Contains(x)));

            skip += limit;
        }
        await context.SaveChangesAsync();
        return HttpStatusCode.OK;
    }
}