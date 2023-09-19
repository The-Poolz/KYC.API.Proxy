using Moq;
using Xunit;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using KYC.API.Proxy.Utils;

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
}