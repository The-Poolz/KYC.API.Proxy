using Moq;
using Xunit;
using System.Net;
using KYC.DataBase;
using SecretsManager;
using FluentAssertions;
using Flurl.Http.Testing;
using KYC.DataBase.Models;
using AdminKycProxy.Models;
using KYC.DataBase.Models.Types;
using Microsoft.EntityFrameworkCore;
using ConfiguredSqlConnection.Extensions;

namespace AdminKycProxy.Tests;

public class LambdaFunctionTests
{
    public LambdaFunctionTests()
    {
        Environment.SetEnvironmentVariable("PAGE_SIZE", "20");
        Environment.SetEnvironmentVariable("SECRET_ID", "SecretId");
        Environment.SetEnvironmentVariable("SECRET_API_KEY", "SecretApiKey");
        Environment.SetEnvironmentVariable("KYC_URL", "https://kyc.blockpass.org/kyc/1.0/connect/ClientId/applicants");
    }

    [Fact]
    internal void Ctor_Default()
    {
        var lambda = new AdminKycProxyLambda();

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
                Records = [
                    new User
                    {
                        RecordId = Guid.NewGuid().ToString(),
                        Status = Status.approved
                    }
                ]
            }
        };
        using var httpTest = new HttpTest();
        httpTest
            .ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/ClientId/applicants?limit=20&skip=0")
            .RespondWithJson(response);
        httpTest
            .ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/ClientId/applicants?limit=20&skip=20")
            .RespondWithJson(new HttpResponse());

        var contextFactory = new Mock<IDbContextFactory<KycDbContext>>();
        contextFactory.Setup(x => x.CreateDbContextAsync(default))
            .ReturnsAsync(() => new DbContextFactory<KycDbContext>().Create(ContextOption.InMemory, Guid.NewGuid().ToString()));

        var lambda = new AdminKycProxyLambda(secretManager.Object, contextFactory.Object);

        var result = await lambda.RunAsync();

        result.Should().Be(HttpStatusCode.OK);
    }
}