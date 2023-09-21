using Moq;
using Xunit;
using FluentAssertions;
using Amazon.DynamoDBv2;
using KYC.API.Proxy.Utils;
using Amazon.DynamoDBv2.Model;

namespace KYC.API.Proxy.Tests.Utils;

public class DynamoDbTests
{
    private readonly Mock<IAmazonDynamoDB> client;
    private readonly DynamoDb dynamoDb;

    public DynamoDbTests()
    {
        client = new Mock<IAmazonDynamoDB>();
        dynamoDb = new DynamoDb(client.Object);
    }

    [Fact]
    internal void GetWallets_UserNotFound()
    {
        client.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ReturnsAsync(CreateGetItemResponse(new Dictionary<string, AttributeValue>()));

        var result = dynamoDb.GetWallets("wallet");

        result.Should().BeEmpty();
    }

    [Fact]
    internal void GetWallets_EvmWalletsNotFoundInUser()
    {
        var item = new Dictionary<string, AttributeValue> { { "EvmWallet", new AttributeValue() } };
        client.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ReturnsAsync(CreateGetItemResponse(item));

        var result = dynamoDb.GetWallets("wallet");

        result.Should().BeEmpty();
    }

    [Fact]
    internal void GetWallets_AssociatedUserNotFound()
    {
        var firstItem = CreateGetItemResponse(new Dictionary<string, AttributeValue>
        {
            { "EvmWallet", new AttributeValue { S = "wallet" } },
            { "EvmWallets", new AttributeValue { L = new List<AttributeValue> { new() { S = "associatedWallet" } } } }
        });

        client.SetupSequence(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .Returns(Task.FromResult(firstItem))
            .Returns(Task.FromResult(CreateGetItemResponse(new Dictionary<string, AttributeValue>())));

        var result = dynamoDb.GetWallets("wallet");

        result.Should().BeEmpty();
    }

    [Fact]
    internal void GetWallets_AssociatedUserHasNoEvmWallets()
    {
        var firstItem = CreateGetItemResponse(new Dictionary<string, AttributeValue>
        {
            { "EvmWallet", new AttributeValue { S = "wallet" } },
            { "EvmWallets", new AttributeValue { L = new List<AttributeValue> { new() { S = "associatedWallet" } } } }
        });
        var secondItem = CreateGetItemResponse(new Dictionary<string, AttributeValue>
        {
            { "EvmWallet", new AttributeValue { S = "associatedWallet" } }
        });

        client.SetupSequence(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .Returns(Task.FromResult(firstItem))
            .Returns(Task.FromResult(secondItem));

        var result = dynamoDb.GetWallets("wallet");

        result.Should().BeEmpty();
    }

    [Fact]
    internal void GetWallets_AssociatedWalletFound()
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

        var result = dynamoDb.GetWallets("wallet");

        result.Should().BeEquivalentTo("associatedWallet");
    }

    [Fact]
    internal async Task UpdateItem()
    {
        client.Setup(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), default))
            .ReturnsAsync(new UpdateItemResponse());

        var testCode = new Func<Task>(async () => await dynamoDb.UpdateItemAsync("wallet", "associatedWallet"));

        await testCode.Should().NotThrowAsync();
    }

    [Fact]
    internal void GetProxyAddress_UserNotFound()
    {
        client.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ReturnsAsync(new GetItemResponse());

        var result = dynamoDb.GetProxyAddress("wallet");

        result.Should().BeNull();
    }

    [Fact]
    internal void GetProxyAddress_ProxyAddressForUserNotFound()
    {
        client.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ReturnsAsync(CreateGetItemResponse(new Dictionary<string, AttributeValue>
            {
                { "EvmWallet", new AttributeValue { S = "wallet" } }
            }));

        var result = dynamoDb.GetProxyAddress("wallet");

        result.Should().BeNull();
    }

    [Fact]
    internal void GetProxyAddress_ReceiveProxyAddress()
    {
        client.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ReturnsAsync(CreateGetItemResponse(new Dictionary<string, AttributeValue>
            {
                { "EvmWallet", new AttributeValue { S = "wallet" } },
                { "Proxy", new AttributeValue { S = "proxy wallet" } }
            }));

        var result = dynamoDb.GetProxyAddress("wallet");

        result.Should().BeEquivalentTo("proxy wallet");
    }

    private static GetItemResponse CreateGetItemResponse(Dictionary<string, AttributeValue> item) => new() { Item = item };
}