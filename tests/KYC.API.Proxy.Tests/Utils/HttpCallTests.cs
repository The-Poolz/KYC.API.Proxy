using Xunit;
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
        Environment.SetEnvironmentVariable("API_KEY", "very-secret-api-key");
        var lambdaSettings = new LambdaSettings();
        var response = new Response
        {
            Status = RequestStatus.success,
            Data = new Data
            {
                Status = "approved"
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
    }
}