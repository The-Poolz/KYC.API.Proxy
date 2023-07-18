using Xunit;
using Flurl.Http.Testing;
using Newtonsoft.Json.Linq;
using KYC.API.Proxy;
using KYC.API.Proxy.Tests;

namespace LambdaFunctionTests;

public class LambdaFunctionTests
{
    private readonly LambdaFunction _lambdaFunction;
    private readonly HttpTest _httpTest;
    private readonly JObject _response;
    private readonly string _invalidAddress = "0x0000000000000000000000000000000000000000";
    private readonly string _validAddress = "0x0000000000000000000000000000000000000001";

    public LambdaFunctionTests()
    {
        _lambdaFunction = new LambdaFunction(new Settings());
        _httpTest = new HttpTest();
        _response = new JObject()
        {
            { "StatusCode", 200 },
            { "Body", "Ok" }
        };
        _httpTest.ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/*").RespondWithJson(_response);
    }

    [Fact]
    public async Task RunAsync_ShouldReturnForbidden_WhenAddressIsInvalid()
    {
        // Arrange
        JObject request = new JObject()
        {
            { "Address", _invalidAddress }
        };

        // Act
        var result = await _lambdaFunction.RunAsync(request);

        // Assert
        Assert.Equal(403, result["StatusCode"]);
    }

    [Fact]
    public async Task RunAsync_ShouldReturnForbidden_WhenAddressIsMissing()
    {
        // Arrange
        JObject request = new JObject();

        // Act
        var result = await _lambdaFunction.RunAsync(request);

        // Assert
        Assert.Equal(403, result["StatusCode"]);
    }
    [Fact]
    public async Task RunAsync_ShouldReturnExpectedResponse_WhenAddressIsValid()
    {
        // Arrange
        JObject request = new JObject()
        {
            { "Address", _validAddress }
        };

        // Act
        var result = await _lambdaFunction.RunAsync(request);

        // Assert
        Assert.Equal(_response, result);
    }
}
