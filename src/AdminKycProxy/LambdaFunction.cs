using Flurl;
using System.Net;
using Flurl.Http;
using KYC.DataBase;
using SecretsManager;
using Amazon.Lambda.Core;
using KYC.DataBase.Models;
using AdminKycProxy.Models;
using KYC.DataBase.Models.Types;
using EnvironmentManager.Extensions;
using ConfiguredSqlConnection.Extensions;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminKycProxy;

public class LambdaFunction(SecretManager secretManager, KycDbContext context)
{
    public LambdaFunction()
        : this(new SecretManager(), new DbContextFactory<KycDbContext>().Create(ContextOption.Staging, "Stage"))
    { }

    public HttpStatusCode Run()
    {
        var url = Env.KYC_URL.Get<string>()
            .SetQueryParam("limit", Env.PAGE_SIZE.Get<int>())
            .WithHeader("Authorization", secretManager.GetSecretValue(Env.SECRET_ID.Get<string>(), Env.SECRET_API_KEY.Get<string>()))
            .WithHeader("cache-control", "no-cache");

        var totalApiCalls = 0;

        Parallel.ForEach(Enum.GetValues<Status>(), status => ProcessStatus(url, status, ref totalApiCalls));

        return HttpStatusCode.OK;
    }

    private void ProcessStatus(IFlurlRequest url, Status status, ref int totalApiCalls)
    {
        var newUsers = new List<User>();
        var skip = context.Users.Count(x => x.Status == status);
        do
        {
            var response = GetHttpResponse(url, status, skip);
            if (response == null)
            {
                SaveNewUsers(newUsers);
                break;
            }

            var downloadedUsers = response.Data.Records
                .Where(downloaded => !context.Users.Any(dbUser => dbUser.RefId == downloaded.RefId))
                .ToList();

            if (downloadedUsers.Count == 0) break;

            newUsers.AddRange(downloadedUsers);
            totalApiCalls++;
            skip += Env.PAGE_SIZE.Get<int>();

            if (totalApiCalls >= Env.MAX_API_CALLS_PER_INVOCATION.Get<int>()) break;

        } while (true);

        SaveNewUsers(newUsers);
    }

    private void SaveNewUsers(List<User> newUsers)
    {
        if (newUsers.Count == 0) return;

        context.Users.AddRange(newUsers);
        context.SaveChanges();
    }

    private static HttpResponse? GetHttpResponse(IFlurlRequest request, Status status, int skip)
    {
        var response = request
            .SetQueryParam("skip", skip)
            .AppendPathSegment(status)
            .AllowHttpStatus(HttpStatusCode.TooManyRequests)
            .GetAsync()
            .GetAwaiter()
            .GetResult();

        return response.StatusCode != (int)HttpStatusCode.OK ? null : response
            .GetJsonAsync<HttpResponse>()
            .GetAwaiter()
            .GetResult();
    }
}