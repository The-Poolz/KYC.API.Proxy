using Moq;
using Xunit;
using SecretsManager;
using Flurl.Http.Testing;
using Newtonsoft.Json.Linq;

namespace KYC.API.Proxy.Tests;

public class LambdaFunctionTests
{
    private readonly JObject response;
    private readonly LambdaFunction lambdaFunction;

    public LambdaFunctionTests()
    {
        var secretManager = new Mock<SecretManager>();
        secretManager.Setup(x => x.GetSecretValue("SecretId", "SecretValue"))
            .Returns("SecretString");

        var settings = new LambdaSettings(secretManager.Object);
        lambdaFunction = new LambdaFunction(settings);

        response = new JObject()
        {
            { "StatusCode", 200 },
            { "Body", "Ok" }
        };
        var httpTest = new HttpTest();
        httpTest.ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/*").RespondWithJson(response);
    }

    [Fact]
    internal async Task RunAsync_ShouldReturnForbidden_WhenAddressIsInvalid()
    {
        var request = new JObject()
        {
            { "Address", "0x0000000000000000000000000000000000000000" }
        };

        var result = await lambdaFunction.RunAsync(request);

        Assert.Equal(403, result["StatusCode"]);
    }

    [Fact]
    internal async Task RunAsync_ShouldReturnForbidden_WhenAddressIsMissing()
    {
        var request = new JObject();

        var result = await lambdaFunction.RunAsync(request);

        Assert.Equal(403, result["StatusCode"]);
    }

    [Fact]
    internal async Task RunAsync_ShouldReturnExpectedResponse_WhenAddressIsValid()
    {
        var request = new JObject()
        {
            { "Address", "0x0000000000000000000000000000000000000001" }
        };

        var result = await lambdaFunction.RunAsync(request);

        Assert.Equal(response, result);
    }
}