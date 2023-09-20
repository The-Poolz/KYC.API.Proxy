using Moq;
using Xunit;
using SecretsManager;
using Flurl.Http.Testing;
using KYC.API.Proxy.Models;
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
            { "status", "success" },
            {
                "data", new JObject
                {
                    { "status", "approved" },
                    {
                        "identities", new JObject
                        {
                            "given_name", new JObject
                            {
                                { "value", "NAME OF USER" }
                            }
                        }
                    }
                }
            }

        };
        var httpTest = new HttpTest();
        httpTest
            .ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/*")
            .RespondWithJson(response);
        var httpCall = new HttpCall(lambdaSettings);

        var result = httpCall.GetBlockPassResponse("address");

        var expected = new OutputData
        {
            RequestStatus = "success",
            Status = "approved",
            Name = "NAME OF USER"
        };
        Assert.Equal(expected, result);
    }
}