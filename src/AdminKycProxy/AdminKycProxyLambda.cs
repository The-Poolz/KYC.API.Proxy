using Flurl;
using Polly;
using System.Net;
using Flurl.Http;
using KYC.DataBase;
using SecretsManager;
using Polly.RateLimit;
using Amazon.Lambda.Core;
using KYC.DataBase.Models;
using AdminKycProxy.Models;
using KYC.DataBase.Models.Types;
using EnvironmentManager.Extensions;
using ConfiguredSqlConnection.Extensions;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminKycProxy;

public class AdminKycProxyLambda(SecretManager secretManager, KycDbContext context, AsyncRateLimitPolicy rateLimiter)
{
    public AdminKycProxyLambda()
        : this(
            secretManager: new SecretManager(),
            context: new DbContextFactory<KycDbContext>().Create(ContextOption.Staging, "Stage"),
            rateLimiter: Policy.RateLimitAsync(Env.MAX_API_CALLS_PER_INVOCATION.Get<int>(), TimeSpan.FromMinutes(1))
        )
    { }

    public async Task<HttpStatusCode> RunAsync()
    {
        var url = Env.KYC_URL.Get<string>()
            .SetQueryParam("limit", Env.PAGE_SIZE.Get<int>())
            .WithHeader("Authorization", secretManager.GetSecretValue(Env.SECRET_ID.Get<string>(), Env.SECRET_API_KEY.Get<string>()))
            .WithHeader("cache-control", "no-cache");

        await Parallel.ForEachAsync(Enum.GetValues<Status>(), async (status, _) => await ProcessStatusAsync(url, status));

        return HttpStatusCode.OK;
    }

    private async Task ProcessStatusAsync(IFlurlRequest url, Status status)
    {
        var newUsers = new List<User>();
        var skip = context.Users.Count(x => x.Status == status);
        do
        {
            var response = await GetHttpResponseAsync(url, status, skip);
            if (response == null)
            {
                await SaveNewUsersAsync(newUsers);
                break;
            }

            var downloadedUsers = response.Data.Records
                .Where(downloaded => !context.Users.Any(dbUser => dbUser.RefId == downloaded.RefId))
                .ToList();

            if (downloadedUsers.Count == 0) break;

            newUsers.AddRange(downloadedUsers);
            skip += Env.PAGE_SIZE.Get<int>();
        } while (true);

        await SaveNewUsersAsync(newUsers);
    }

    private async Task SaveNewUsersAsync(List<User> newUsers)
    {
        if (newUsers.Count == 0) return;

        context.Users.AddRange(newUsers);
        await context.SaveChangesAsync();
    }

    private async Task<HttpResponse?> GetHttpResponseAsync(IFlurlRequest request, Status status, int skip)
    {
        return await rateLimiter.ExecuteAsync(async () =>
        {
            var response = await request
                .SetQueryParam("skip", skip)
                .AppendPathSegment(status)
                .AllowHttpStatus(HttpStatusCode.TooManyRequests)
                .GetAsync();

            return response.StatusCode != (int)HttpStatusCode.OK ? null : await response.GetJsonAsync<HttpResponse>();
        });
    }
}