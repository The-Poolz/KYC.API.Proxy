using Moq;
using Xunit;
using SecretsManager;
using Flurl.Http.Testing;
using KYC.API.Proxy.Utils;
using KYC.API.Proxy.Models;
using KYC.API.Proxy.Models.HttpResponse;

namespace KYC.API.Proxy.Tests.Utils;

public class HttpCallTests
{
    [Fact]
    internal void GetBlockPassResponse()
    {
        Environment.SetEnvironmentVariable("BLOCKPASS_URI", "https://kyc.blockpass.org/kyc/1.0/connect/[ClientId]/refId/[UserAddress]");
        Environment.SetEnvironmentVariable("CLIENT_ID", "ClientId");
        var secretManager = new Mock<SecretManager>();
        secretManager.Setup(x => x.GetSecretValue("SecretId", "SecretValue"))
            .Returns("SecretString");
        var lambdaSettings = new LambdaSettings(secretManager.Object);
        var response = new Response
        {
            Status = RequestStatus.success,
            Data = new Data
            {
                Status = "approved",
                Identities = new Identities
                {
                    GivenName = new GivenName
                    {
                        Value = "USER NAME"
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

        Assert.Equal(response.Status, result.Status);
        Assert.Equal(response.Data.Status, result.Data.Status);
        Assert.Equal(response.Data.Identities.GivenName.Value, result.Data.Identities.GivenName.Value);
    }
}