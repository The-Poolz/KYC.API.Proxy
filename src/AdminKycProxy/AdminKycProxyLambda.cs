using Flurl;
using System.Net;
using Flurl.Http;
using KYC.DataBase;
using SecretsManager;
using Amazon.Lambda.Core;
using AdminKycProxy.Models;
using KYC.DataBase.Models.Types;
using EnvironmentManager.Extensions;
using Microsoft.EntityFrameworkCore;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminKycProxy;

public class AdminKycProxyLambda(SecretManager secretManager, IDbContextFactory<KycDbContext> contextFactory)
{
    public AdminKycProxyLambda()
        : this(
            secretManager: new SecretManager(),
            contextFactory: new KycDbContextFactory()
        )
    { }

    public async Task<HttpStatusCode> RunAsync()
    {
        await Task.WhenAll(
            Enum.GetValues<Status>()
                .Select(ProcessStatusAsync)
                .ToList()
        );

        return HttpStatusCode.OK;
    }

    private async Task ProcessStatusAsync(Status status)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var skip = context.Users.Count(x => x.Status == status);

        var url = Env.KYC_URL.Get<string>()
            .AppendPathSegment(status)
            .WithHeader("Authorization", secretManager.GetSecretValue(Env.SECRET_ID.Get<string>(), Env.SECRET_API_KEY.Get<string>()))
            .WithHeader("cache-control", "no-cache")
            .SetQueryParam("limit", 1);

        var users = context.Users.Where(x => x.Status == status).ToArray();
        var response = await GetHttpResponseAsync(url, skip - 1);
        if (response?.Data.Records.Length > 0 && users.Length > 0 && users.Last().RefId != response.Data.Records.First().RefId)
        {
            context.Users.RemoveRange(context.Users.Where(x => x.Status == status));
            var deleted = await context.SaveChangesAsync();
            LambdaLogger.Log($"DELETED: For '{status}' status: {deleted}");
        }

        url = url.SetQueryParam("limit", Env.PAGE_SIZE.Get<int>());
        do
        {
            response = await GetHttpResponseAsync(url, skip);
            if (response == null) break;

            var downloadedUsers = response.Data.Records
                .Where(downloaded => users.All(dbUser => dbUser.RefId != downloaded.RefId))
                .ToList();

            if (downloadedUsers.Count == 0) break;

            context.Users.AddRange(downloadedUsers);
            skip += Env.PAGE_SIZE.Get<int>();
        } while (true);

        var added = await context.SaveChangesAsync();
        LambdaLogger.Log($"ADDED: For '{status}' status: {added}");
    }

    private static async Task<HttpResponse?> GetHttpResponseAsync(IFlurlRequest url, int skip)
    {
        var response = await url
            .SetQueryParam("skip", skip)
            .AllowHttpStatus(HttpStatusCode.TooManyRequests)
            .GetAsync();

        return response.StatusCode != (int)HttpStatusCode.OK ? null : await response.GetJsonAsync<HttpResponse>();
    }
}