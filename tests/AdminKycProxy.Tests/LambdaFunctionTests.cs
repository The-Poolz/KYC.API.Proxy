using Moq;
using Xunit;
using System.Net;
using KYC.DataBase;
using SecretsManager;
using FluentAssertions;
using Flurl.Http.Testing;
using KYC.DataBase.Models;
using AdminKycProxy.Models;
using Amazon.Lambda.TestUtilities;
using ConfiguredSqlConnection.Extensions;

namespace AdminKycProxy.Tests;

public class LambdaFunctionTests
{
    public LambdaFunctionTests()
    {
        Environment.SetEnvironmentVariable("DOWNLOADED_FROM", "0");
        Environment.SetEnvironmentVariable("SECRET_ID", "SecretId");
        Environment.SetEnvironmentVariable("SECRET_API_KEY", "SecretApiKey");
        Environment.SetEnvironmentVariable("KYC_URL", "https://kyc.blockpass.org/kyc/1.0/connect/ClientId/applicants");
    }

    [Fact]
    internal void Ctor_Default()
    {
        var lambda = new LambdaFunction();

        lambda.Should().NotBeNull();
    }

    [Fact]
    internal async Task RunAsync()
    {
        var secretManager = new Mock<SecretManager>();
        secretManager
            .Setup(s => s.GetSecretValue("SecretId", "SecretApiKey"))
            .Returns("SecretValue");

        var response = new HttpResponse
        {
            Data = new Data
            {
                Limit = 20,
                Skip = 0,
                Total = 1,
                Records = new[]
                {
                    new User
                    {
                        RecordId = Guid.NewGuid().ToString(),
                        Status = "approved"
                    }
                }
            }
        };
        using var httpTest = new HttpTest();
        httpTest
            .ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/ClientId/applicants?limit=20&skip=0")
            .RespondWithJson(response);
        httpTest
            .ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/ClientId/applicants?limit=20&skip=20")
            .RespondWithJson(new HttpResponse());

        var context = new DbContextFactory<KycDbContext>().Create(ContextOption.InMemory, Guid.NewGuid().ToString());

        var lambda = new LambdaFunction(secretManager.Object, context);

        var result = await lambda.RunAsync(new TestLambdaContext());

        result.Should().Be(HttpStatusCode.OK);
        context.Users.Should().HaveCount(1);
    }
}