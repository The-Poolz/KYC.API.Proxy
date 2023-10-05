using Moq;
using Xunit;
using SecretsManager;
using FluentAssertions;
using AdminKycProxy.Models;
using Flurl.Http.Testing;
using Newtonsoft.Json.Linq;

namespace AdminKycProxy.Tests;

public class LambdaFunctionTests
{
    [Fact]
    internal void Ctor_Default()
    {
        var lambda = new LambdaFunction();

        lambda.Should().NotBeNull();
    }

    [Fact]
    internal void Run()
    {
        Environment.SetEnvironmentVariable("SECRET_ID", "SecretId");
        Environment.SetEnvironmentVariable("SECRET_API_KEY", "SecretApiKey");
        Environment.SetEnvironmentVariable("CLIENT_ID", "ClientId");
        var secretManager = new Mock<SecretManager>();
        secretManager
            .Setup(s => s.GetSecretValue("SecretId", "SecretApiKey"))
            .Returns("SecretValue");

        var response = new JObject
        {
            { "message", "hello world!" }
        };
        using var httpTest = new HttpTest();
        httpTest.ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/ClientId/applicants/waiting?skip=20&limit=20")
            .RespondWithJson(response);

        var lambda = new LambdaFunction(secretManager.Object);

        var result = lambda.Run(new InputData
        {
            Status = "waiting",
            Limit = 20,
            Skip = 20
        });

        result.Should().BeEquivalentTo(response);
    }
}