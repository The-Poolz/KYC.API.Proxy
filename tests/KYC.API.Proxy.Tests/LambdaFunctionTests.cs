using Moq;
using Xunit;
using KYC.API.Proxy.Models;
using KYC.API.Proxy.Models.HttpResponse;
using KYC.API.Proxy.Utils;
using Newtonsoft.Json.Linq;

namespace KYC.API.Proxy.Tests;

public class LambdaFunctionTests
{
    private const string TestAddress = "0x0000000000000000000000000000000000000001";
    private const string AssociatedAddress = "0x0000000000000000000000000000000000000002";

    public LambdaFunctionTests()
    {
        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-2");
    }

    [Fact]
    internal void Ctor_Default()
    {
        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-2");

        var function = new LambdaFunction();

        Assert.NotNull(function);
    }

    [Fact]
    internal async Task RunAsync_ShouldReturnForbidden_WhenAddressIsInvalid()
    {
        var request = new InputData
        {
            Address = LambdaFunction.ZeroAddress
        };
        var lambdaFunction = MockLambdaFunction();

        var result = await lambdaFunction.RunAsync(request);

        Assert.Equal(RequestStatus.error, result.RequestStatus);
    }

    [Fact]
    internal async Task RunAsync_ShouldReturnForbidden_WhenAddressIsMissing()
    {
        var request = new InputData();
        var lambdaFunction = MockLambdaFunction();

        var result = await lambdaFunction.RunAsync(request);

        Assert.Equal(RequestStatus.error, result.RequestStatus);
    }

    [Fact]
    internal async Task RunAsync_ShouldReturnExpectedResponse_WhenAddressIsValid()
    {
        var request = new InputData
        {
            Address = TestAddress
        };
        var lambdaFunction = MockLambdaFunction();

        var result = await lambdaFunction.RunAsync(request);

        Assert.Equal(RequestStatus.success, result.RequestStatus);
    }

    [Fact]
    internal async Task RunAsync_ReceiveAddressFromAssociatedWallets()
    {
        var mockDynamoDb = new Mock<DynamoDb>();
        mockDynamoDb.Setup(x => x.GetWallets(TestAddress))
            .Returns(new[] { AssociatedAddress });

        var mockHttpCall = new Mock<HttpCall>();
        mockHttpCall.Setup(x => x.GetBlockPassResponse(TestAddress))
            .Returns(new Response());
        mockHttpCall.Setup(x => x.GetBlockPassResponse(AssociatedAddress))
            .Returns(new Response
            {
                Status = RequestStatus.success
            });

        var request = new InputData
        {
            Address = TestAddress
        };
        var lambdaFunction = MockLambdaFunction(mockHttpCall, mockDynamoDb);

        var result = await lambdaFunction.RunAsync(request);

        Assert.Equal(RequestStatus.success, result.RequestStatus);
    }

    [Fact]
    internal async Task RunAsync_ReceiveErrorResponse()
    {
        var mockDynamoDb = new Mock<DynamoDb>();
        mockDynamoDb.Setup(x => x.GetWallets(TestAddress))
            .Returns(new[] { AssociatedAddress });

        var mockHttpCall = new Mock<HttpCall>();
        mockHttpCall.Setup(x => x.GetBlockPassResponse(TestAddress))
            .Returns(new Response());
        mockHttpCall.Setup(x => x.GetBlockPassResponse(AssociatedAddress))
            .Returns(new Response());

        var request = new InputData
        {
            Address = TestAddress
        };

        var lambdaFunction = MockLambdaFunction(mockHttpCall, mockDynamoDb);

        var result = await lambdaFunction.RunAsync(request);

        Assert.Equal(RequestStatus.error, result.RequestStatus);
    }

    [Fact]
    internal async Task RunAsync_WhenStatusBeError()
    {
        var request = new InputData
        {
            Address = TestAddress
        };
        var errorResponse = new Response
        {
            Status = RequestStatus.error
        };
        var mockDynamoDb = new Mock<DynamoDb>();
        mockDynamoDb.Setup(x => x.GetWallets(TestAddress))
            .Returns(new[] { AssociatedAddress });

        var mockHttpCall = new Mock<HttpCall>();
        mockHttpCall.Setup(x => x.GetBlockPassResponse(TestAddress))
            .Returns(errorResponse);
        mockHttpCall.Setup(x => x.GetBlockPassResponse(AssociatedAddress))
            .Returns(errorResponse);
        var lambdaFunction = MockLambdaFunction(mockHttpCall);

        var result = await lambdaFunction.RunAsync(request);

        Assert.Equal(RequestStatus.error, result.RequestStatus);
    }

    private static LambdaFunction MockLambdaFunction(Mock<HttpCall>? mockHttpCall = null, Mock<DynamoDb>? mockDynamoDb = null)
    {
        if (mockHttpCall == null)
        {
            mockHttpCall = new Mock<HttpCall>();
            mockHttpCall.Setup(x => x.GetBlockPassResponse(It.IsAny<string>()))
                .Returns(new Response
                {
                    Status = RequestStatus.success
                });
        }

        mockDynamoDb ??= new Mock<DynamoDb>();

        return new LambdaFunction(mockHttpCall.Object, mockDynamoDb.Object);
    }
}
