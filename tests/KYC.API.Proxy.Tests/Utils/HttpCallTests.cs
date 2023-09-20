using Moq;
using Xunit;
using SecretsManager;
using Flurl.Http.Testing;
using KYC.API.Proxy.Utils;
using Newtonsoft.Json.Linq;

namespace KYC.API.Proxy.Tests.Utils;

public class HttpCallTests
{
    [Fact]
    internal void GetBlockPassResponse()
    {
        var secretManager = new Mock<SecretManager>();
        secretManager.Setup(x => x.GetSecretValue("SecretId", "SecretValue"))
            .Returns("SecretString");
        var lambdaSettings = new LambdaSettings(secretManager.Object);
        var response = new JObject
        {
            { "StatusCode", 200 },
            { "Body", "Ok" }
        };
        var httpTest = new HttpTest();
        httpTest
            .ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/*")
            .RespondWithJson(response);
        var httpCall = new HttpCall(lambdaSettings);

        var result = httpCall.GetBlockPassResponse("address");

        Assert.Equal(response, result);
    }
}