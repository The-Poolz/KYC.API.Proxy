using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace KYC.API.Proxy.Utils;

public class DynamoDb
{
    private readonly IAmazonDynamoDB client;

    public DynamoDb()
        : this(new AmazonDynamoDBClient())
    { }

    public DynamoDb(IAmazonDynamoDB client)
    {
        this.client = client;
    }

    public async Task<string[]> GetWalletsAsync(string wallet)
    {
        var request = new QueryRequest
        {
            TableName = "UserData",
            KeyConditionExpression = "EvmWallet = :v_key",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_key", new AttributeValue { S = wallet } }
            }
        };

        var response = await client.QueryAsync(request);

        if (response.Items.Count == 0)
        {
            return Array.Empty<string>();
        }

        var evmWallets = response.Items[0]["EvmWallets"].L.Select(x => x.S).ToList();
        var evmWallet = response.Items[0]["EvmWallet"].S;

        return wallet == evmWallet ? evmWallets.ToArray() : 
            new List<string>(evmWallets)
            {
                evmWallet
            }.ToArray();
    }
}