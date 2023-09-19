using Moq;
using Xunit;
using SecretsManager;
using Flurl.Http.Testing;
using KYC.API.Proxy.Utils;
using Newtonsoft.Json.Linq;
using Amazon.DynamoDBv2.Model;

namespace KYC.API.Proxy.Tests;

public class LambdaFunctionTests
{
    private readonly JObject response;
    private readonly Mock<DynamoDb> dynamoDb;
    private readonly LambdaFunction lambdaFunction;

    public LambdaFunctionTests()
    {
        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-2");

        var secretManager = new Mock<SecretManager>();
        secretManager.Setup(x => x.GetSecretValue("SecretId", "SecretValue"))
            .Returns("SecretString");

        dynamoDb = new Mock<DynamoDb>();

        var settings = new LambdaSettings(secretManager.Object);
        lambdaFunction = new LambdaFunction(settings, dynamoDb.Object);

        response = new JObject
        {
            { "StatusCode", 200 },
            { "Body", "Ok" }
        };
        var httpTest = new HttpTest();
        httpTest.ForCallsTo("https://kyc.blockpass.org/kyc/1.0/connect/*").RespondWithJson(response);
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

    [Fact]
    internal void Run_ShouldReturnExpectedResponse()
    {


        var firstItem = CreateGetItemResponse(new Dictionary<string, AttributeValue>
        {
            { "EvmWallet", new AttributeValue { S = "wallet" } },
            { "EvmWallets", new AttributeValue { L = new List<AttributeValue> { new() { S = "associatedWallet" } } } }
        });
        var secondItem = CreateGetItemResponse(new Dictionary<string, AttributeValue>
        {
            { "EvmWallet", new AttributeValue { S = "associatedWallet" } },
            { "EvmWallets", new AttributeValue { L = new List<AttributeValue> { new() { S = "wallet" } } } }
        });

        client.SetupSequence(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .Returns(Task.FromResult(firstItem))
            .Returns(Task.FromResult(secondItem));

        var request = new JObject
        {
            { "Address", "0x0000000000000000000000000000000000000001" }
        };

        var result = lambdaFunction.Run(request);

        Assert.Equal(response, result);
    }

    private static GetItemResponse CreateGetItemResponse(Dictionary<string, AttributeValue> item) => new() { Item = item };
}
