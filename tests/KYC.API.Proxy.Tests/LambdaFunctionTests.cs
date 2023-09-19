using Moq;
using Xunit;
using SecretsManager;
using Flurl.Http.Testing;
using KYC.API.Proxy.Utils;
using Newtonsoft.Json.Linq;

namespace KYC.API.Proxy.Tests;

public class LambdaFunctionTests
{
    private readonly JObject response;
    private readonly LambdaFunction lambdaFunction;

    public LambdaFunctionTests()
    {
        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-2");

        var secretManager = new Mock<SecretManager>();
        secretManager.Setup(x => x.GetSecretValue("SecretId", "SecretValue"))
            .Returns("SecretString");

        var dynamoDb = new Mock<DynamoDb>();
        dynamoDb.Setup(x => x.GetWallets(It.IsAny<string>()))
            .Returns(Array.Empty<string>());

        var settings = new LambdaSettings(secretManager.Object);
        lambdaFunction = new LambdaFunction(settings, dynamoDb.Object);

        response = new JObject()
        {
            { "StatusCode", 200 },
            { "Body", "Ok" }
        };
        var httpTest = new HttpTest();
        httpTest.ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/*").RespondWithJson(response);
    }

    [Fact]
    internal void Run_ShouldReturnForbidden_WhenAddressIsInvalid()
    {
        var request = new JObject
        {
            { "Address", "0x0000000000000000000000000000000000000000" }
        };

        var result = lambdaFunction.Run(request);

        Assert.Equal(403, result["StatusCode"]);
    }

    [Fact]
    internal void Run_ShouldReturnForbidden_WhenAddressIsMissing()
    {
        var request = new JObject();

        var result = lambdaFunction.Run(request);

        Assert.Equal(403, result["StatusCode"]);
    }

    [Fact]
    internal void Run_ShouldReturnExpectedResponse_WhenAddressIsValid()
    {
        var request = new JObject
        {
            { "Address", "0x0000000000000000000000000000000000000001" }
        };

        var result = lambdaFunction.Run(request);

        Assert.Equal(response, result);
    }
}
