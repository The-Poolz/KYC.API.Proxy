using Moq;
using Xunit;
using KYC.API.Proxy.Utils;
using Newtonsoft.Json.Linq;

namespace KYC.API.Proxy.Tests;

public class LambdaFunctionTests
{
    private const string TestAddress = "0x0000000000000000000000000000000000000001";
    private const string AssociatedAddress = "0x0000000000000000000000000000000000000002";
    private readonly JObject response = new()
    {
        { "status", "success" }
    };

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
    internal void Run_ShouldReturnForbidden_WhenAddressIsInvalid()
    {
        var request = new JObject
        {
            { "Address", "0x0000000000000000000000000000000000000000" }
        };
        var lambdaFunction = MockLambdaFunction();

        var result = lambdaFunction.Run(request);

        Assert.Equal(403, result["StatusCode"]);
    }

    [Fact]
    internal void Run_ShouldReturnForbidden_WhenAddressIsMissing()
    {
        var request = new JObject();
        var lambdaFunction = MockLambdaFunction();

        var result = lambdaFunction.Run(request);

        Assert.Equal(403, result["StatusCode"]);
    }

    [Fact]
    internal void Run_ShouldReturnExpectedResponse_WhenAddressIsValid()
    {
        var request = new JObject
        {
            { "Address", TestAddress }
        };
        var lambdaFunction = MockLambdaFunction();

        var result = lambdaFunction.Run(request);

        Assert.Equal(response, result);
    }

    [Fact]
    internal void Run_ReceiveAddressFromAssociatedWallets()
    {
        var mockDynamoDb = new Mock<DynamoDb>();
        mockDynamoDb.Setup(x => x.GetWallets(TestAddress))
            .Returns(new[] { AssociatedAddress });

        var mockHttpCall = new Mock<HttpCall>();
        mockHttpCall.Setup(x => x.GetBlockPassResponse(TestAddress))
            .Returns(new JObject());
        mockHttpCall.Setup(x => x.GetBlockPassResponse(AssociatedAddress))
            .Returns(response);

        var request = new JObject
        {
            { "Address", TestAddress }
        };

        var lambdaFunction = MockLambdaFunction(mockHttpCall, mockDynamoDb);

        var result = lambdaFunction.Run(request);

        Assert.Equal(response, result);
    }

    [Fact]
    internal void Run_ReceiveErrorResponse()
    {
        var mockDynamoDb = new Mock<DynamoDb>();
        mockDynamoDb.Setup(x => x.GetWallets(TestAddress))
            .Returns(new[] { AssociatedAddress });

        var mockHttpCall = new Mock<HttpCall>();
        mockHttpCall.Setup(x => x.GetBlockPassResponse(TestAddress))
            .Returns(new JObject());
        mockHttpCall.Setup(x => x.GetBlockPassResponse(AssociatedAddress))
            .Returns(new JObject());

        var request = new JObject
        {
            { "Address", TestAddress }
        };

        var lambdaFunction = MockLambdaFunction(mockHttpCall, mockDynamoDb);

        var result = lambdaFunction.Run(request);

        var expected = new JObject
        {
            { "status", "error" }
        };
        Assert.Equal(expected, result);
    }

    private LambdaFunction MockLambdaFunction(Mock<HttpCall>? mockHttpCall = null, Mock<DynamoDb>? mockDynamoDb = null)
    {
        if (mockHttpCall == null)
        {
            mockHttpCall = new Mock<HttpCall>();
            mockHttpCall.Setup(x => x.GetBlockPassResponse(It.IsAny<string>()))
                .Returns(response);
        }

        mockDynamoDb ??= new Mock<DynamoDb>();

        return new LambdaFunction(mockHttpCall.Object, mockDynamoDb.Object);
    }
}
