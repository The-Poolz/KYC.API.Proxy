using Flurl;
using System.Net;
using Flurl.Http;
using KYC.DataBase;
using SecretsManager;
using Amazon.Lambda.Core;
using EnvironmentManager;
using KYC.DataBase.Models;
using AdminKycProxy.Models;
using ConfiguredSqlConnection.Extensions;

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
        var envManager = new EnvManager();
        var limit = envManager.GetEnvironmentValue<int>("PAGE_SIZE", true);
        var maxRecords = envManager.GetEnvironmentValue<int>("MAX_RECORDS_PER_INVOCATION", true);
        var skip = _context.Users.Count();
        var url = _lambdaSettings.Url
            .SetQueryParam("limit", limit)
            .WithHeader("Authorization", _lambdaSettings.SecretApiKey)
            .WithHeader("cache-control", "no-cache");

        var newUsers = new List<User>();
        var totalRecordsProcessed = 0;
        do
        {
            var response = await GetHttpResponseAsync(url, skip);
            if (response == null)
            {
                _context.Users.AddRange(newUsers);
                await _context.SaveChangesAsync();
                return HttpStatusCode.TooManyRequests;
            }

            var downloadedUsers = response.Data.Records
                .Where(downloaded => !_context.Users.Any(dbUser => dbUser.RefId == downloaded.RefId))
                .ToList();

            if (!downloadedUsers.Any()) break;

            newUsers.AddRange(downloadedUsers);
            totalRecordsProcessed += downloadedUsers.Count;
            skip += limit;

            if (totalRecordsProcessed >= maxRecords) break;

        } while (true);

        _context.Users.AddRange(newUsers);
        await _context.SaveChangesAsync();

        return HttpStatusCode.OK;
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