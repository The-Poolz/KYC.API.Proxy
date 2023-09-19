using Moq;
using Xunit;
using Amazon.DynamoDBv2;
using FluentAssertions;
using KYC.API.Proxy.Utils;
using Amazon.DynamoDBv2.Model;

namespace KYC.API.Proxy.Tests.Utils;

public class DynamoDbTests
{
    [Fact]
    internal void GetWallets_UserNotFound()
    {
        var client = new Mock<IAmazonDynamoDB>();
        client.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse());
        var dynamoDb = new DynamoDb(client.Object);

        var result = dynamoDb.GetWallets("wallet");

        result.Should().BeEmpty();
    }

    [Fact]
    internal void GetWallets_EvmWalletsNotFoundInUser()
    {
        var client = new Mock<IAmazonDynamoDB>();
        client.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse
            {
                Item = new Dictionary<string, AttributeValue>
                {
                    { "EvmWallet", new AttributeValue() }
                }
            });
        var dynamoDb = new DynamoDb(client.Object);

        var result = dynamoDb.GetWallets("wallet");

        result.Should().BeEmpty();
    }
}