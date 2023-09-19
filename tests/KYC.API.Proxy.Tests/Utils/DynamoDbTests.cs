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
        client.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .ReturnsAsync(new GetItemResponse());
        var dynamoDb = new DynamoDb(client.Object);

        var result = dynamoDb.GetWallets("wallet");

        result.Should().BeEmpty();
    }

    [Fact]
    internal void GetWallets_EvmWalletsNotFoundInUser()
    {
        var client = new Mock<IAmazonDynamoDB>();
        client.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
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

    [Fact]
    internal void GetWallets_AssociatedUserNotFound()
    {
        const string wallet = "wallet";
        var client = new Mock<IAmazonDynamoDB>();

        var firstResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                {
                    "EvmWallet",
                    new AttributeValue 
                    {
                        S = wallet
                    }
                },
                {
                    "EvmWallets",
                    new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new() { S = "associatedWallet" }
                        }
                    }
                }
            }
        };
        var emptyResponse = new GetItemResponse { Item = new Dictionary<string, AttributeValue>() };

        client.SetupSequence(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .Returns(Task.FromResult(firstResponse))
            .Returns(Task.FromResult(emptyResponse));

        var dynamoDb = new DynamoDb(client.Object);

        var result = dynamoDb.GetWallets(wallet);

        result.Should().BeEmpty();
    }

    [Fact]
    internal void GetWallets_EvmWalletsNotFoundInAssociatedUser()
    {
        const string wallet = "wallet";
        const string associatedWallet = "associatedWallet";
        var client = new Mock<IAmazonDynamoDB>();

        var firstResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                {
                    "EvmWallet",
                    new AttributeValue
                    {
                        S = wallet
                    }
                },
                {
                    "EvmWallets",
                    new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new() { S = associatedWallet }
                        }
                    }
                }
            }
        };
        var secondResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                {
                    "EvmWallet",
                    new AttributeValue
                    {
                        S = associatedWallet
                    }
                }
            }
        };

        client.SetupSequence(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .Returns(Task.FromResult(firstResponse))
            .Returns(Task.FromResult(secondResponse));

        var dynamoDb = new DynamoDb(client.Object);

        var result = dynamoDb.GetWallets(wallet);

        result.Should().BeEmpty();
    }

    [Fact]
    internal void GetWallets_AssociatedWalletFound()
    {
        const string wallet = "wallet";
        const string associatedWallet = "associatedWallet";
        var client = new Mock<IAmazonDynamoDB>();

        var firstResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                {
                    "EvmWallet",
                    new AttributeValue
                    {
                        S = wallet
                    }
                },
                {
                    "EvmWallets",
                    new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new() { S = associatedWallet }
                        }
                    }
                }
            }
        };
        var secondResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                {
                    "EvmWallet",
                    new AttributeValue
                    {
                        S = associatedWallet
                    }
                },
                {
                    "EvmWallets",
                    new AttributeValue
                    {
                        L = new List<AttributeValue>
                        {
                            new() { S = wallet }
                        }
                    }
                }
            }
        };

        client.SetupSequence(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), default))
            .Returns(Task.FromResult(firstResponse))
            .Returns(Task.FromResult(secondResponse));

        var dynamoDb = new DynamoDb(client.Object);

        var result = dynamoDb.GetWallets("wallet");

        result.Should().BeEquivalentTo(associatedWallet);
    }
}