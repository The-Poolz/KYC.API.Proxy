using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace KYC.API.Proxy.Utils;

public class DynamoDb
{
    public const string TableName = "UserData";

    private readonly IAmazonDynamoDB client;

    public DynamoDb()
        : this(new AmazonDynamoDBClient())
    { }

    public DynamoDb(IAmazonDynamoDB client)
    {
        this.client = client;
    }

    public virtual IEnumerable<string> GetWallets(string wallet)
    {
        var user = GetItem(wallet);
        if (user == null || !user.ContainsKey("EvmWallets"))
            return Array.Empty<string>();

        var associatedWallets = user["EvmWallets"].L.Select(x => x.S).ToArray();

        return associatedWallets
            .Select(associatedWallet => GetItem(associatedWallet))
            .Where(associatedUser => associatedUser != null && associatedUser.ContainsKey("EvmWallets"))
            .Where(associatedUser => associatedUser!["EvmWallets"].L.Exists(x => x.S == wallet))
            .Select(associatedUser => associatedUser!["EvmWallet"].S);
    }

    public virtual string? GetProxyAddress(string wallet)
    {
        var user = GetItem(wallet);
        if (user == null)
            return null;

        return user.TryGetValue("Proxy", out var proxy) ? proxy.S : null;
    }

    public void UpdateItem(string primaryKey, string proxyAddress) => UpdateItemAsync(primaryKey, proxyAddress).GetAwaiter().GetResult();

    public virtual async Task UpdateItemAsync(string primaryKey, string proxyAddress)
    {
        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "EvmWallet", new AttributeValue { S = primaryKey } }
            },
            AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
            {
                {
                    "Proxy", new AttributeValueUpdate
                    {
                        Value = new AttributeValue { S = proxyAddress },
                        Action = AttributeAction.PUT
                    }
                }
            }
        };

        await client.UpdateItemAsync(request);
    }

    public virtual Dictionary<string, AttributeValue>? GetItem(string primaryKey)
    {
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "EvmWallet", new AttributeValue { S = primaryKey } }
            }
        };

        var response = client.GetItemAsync(request)
            .GetAwaiter()
            .GetResult();

        return response.Item.Count == 0 ? null : response.Item;
    }
}