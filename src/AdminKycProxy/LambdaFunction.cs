using Flurl;
using System.Net;
using Flurl.Http;
using KYC.DataBase;
using SecretsManager;
using Amazon.Lambda.Core;
using AdminKycProxy.Models;
using ConfiguredSqlConnection.Extensions;
using Microsoft.EntityFrameworkCore;
using EnvironmentManager;
using EnvironmentManager.Extensions;
using KYC.DataBase.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminKycProxy;

public class LambdaFunction
{
    private readonly LambdaSettings _lambdaSettings;
    private readonly KycDbContext _context;

    public LambdaFunction()
        : this(new SecretManager(), new DbContextFactory<KycDbContext>().Create(ContextOption.Staging, "Stage"))
    { }

    public LambdaFunction(SecretManager secretManager, KycDbContext context)
    {
        _lambdaSettings = new LambdaSettings(secretManager);
        _context = context;
    }

    public async Task<HttpStatusCode> RunAsync(ILambdaContext lambdaContext)
    {
        var limit = Env.PAGE_SIZE.Get<int>();
        var skip = _context.Users.Count();
        var url = _lambdaSettings.Url
            .SetQueryParam("limit", limit)
            .WithHeader("Authorization", _lambdaSettings.SecretApiKey)
            .WithHeader("cache-control", "no-cache");

        var newUsers = new List<User>();
        HttpResponse? response;
        do
        {
            response = await url
                .SetQueryParam("skip", skip)
                .OnError(x =>
                {
                    if (x.HttpResponseMessage.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        lambdaContext.Logger.LogInformation(x.Exception.Message);
                    }
                })
                .GetJsonAsync<HttpResponse>();

            var downloadedUsers = response.Data.Records
                .Where(downloaded => !_context.Users.Any(dbUser => dbUser.RefId == downloaded.RefId))
                .ToList();

            if (!downloadedUsers.Any()) break;

            newUsers.AddRange(downloadedUsers);
            skip += limit;

        } while (response.Data.Records.Length > 0);

        _context.Users.AddRange(newUsers);
        await _context.SaveChangesAsync();

        return HttpStatusCode.OK;
    }
}