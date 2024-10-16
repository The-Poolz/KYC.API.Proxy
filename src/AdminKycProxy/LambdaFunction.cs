using Flurl;
using System.Net;
using Flurl.Http;
using KYC.DataBase;
using SecretsManager;
using Amazon.Lambda.Core;
using KYC.DataBase.Models;
using AdminKycProxy.Models;
using EnvironmentManager.Extensions;
using ConfiguredSqlConnection.Extensions;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminKycProxy;

public class LambdaFunction(SecretManager secretManager, KycDbContext context)
{
    public LambdaFunction()
        : this(new SecretManager(), new DbContextFactory<KycDbContext>().Create(ContextOption.Staging, "Stage"))
    { }

    public async Task<HttpStatusCode> RunAsync(ILambdaContext lambdaContext)
    {
        var skip = context.Users.Count();
        var url = Env.KYC_URL.Get<string>()
            .SetQueryParam("limit", Env.PAGE_SIZE.Get<int>())
            .WithHeader("Authorization", secretManager.GetSecretValue(Env.SECRET_ID.Get<string>(), Env.SECRET_API_KEY.Get<string>()))
            .WithHeader("cache-control", "no-cache");

        var newUsers = new List<User>();
        var totalRecordsProcessed = 0;
        do
        {
            var response = await GetHttpResponseAsync(url, skip);
            if (response == null)
            {
                await SaveNewUsersAsync(newUsers);
                return HttpStatusCode.TooManyRequests;
            }

            var downloadedUsers = response.Data.Records
                .Where(downloaded => !context.Users.Any(dbUser => dbUser.RefId == downloaded.RefId))
                .ToList();

            if (downloadedUsers.Count == 0) break;

            newUsers.AddRange(downloadedUsers);
            totalRecordsProcessed += downloadedUsers.Count;
            skip += Env.PAGE_SIZE.Get<int>();

            if (totalRecordsProcessed >= Env.MAX_RECORDS_PER_INVOCATION.Get<int>()) break;

        } while (true);

        await SaveNewUsersAsync(newUsers);

        return HttpStatusCode.OK;
    }

    private async Task SaveNewUsersAsync(List<User> newUsers)
    {
        if (newUsers.Count == 0) return;

        context.Users.AddRange(newUsers);
        await context.SaveChangesAsync();
    }

    private static async Task<HttpResponse?> GetHttpResponseAsync(IFlurlRequest request, int skip)
    {
        var response = await request
            .SetQueryParam("skip", skip)
            .AllowHttpStatus(HttpStatusCode.TooManyRequests)
            .GetAsync();

        return response.StatusCode == (int)HttpStatusCode.OK ? await response.GetJsonAsync<HttpResponse>() : null;
    }
}