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

    public virtual string[] GetWallets(string wallet)
    {
        var user = GetItem(wallet);
        if (user == null || !user.ContainsKey("EvmWallets"))
            return Array.Empty<string>();

        var associatedWallets = user["EvmWallets"].L.Select(x => x.S).ToArray();

        var wallets = new List<string>();
        foreach (var associatedWallet in associatedWallets)
        {
            var associatedUser = GetItem(associatedWallet);
            if (associatedUser == null || !associatedUser.ContainsKey("EvmWallets"))
                continue;

            if (associatedUser["EvmWallets"].L.Exists(x => x.S == wallet))
            {
                wallets.Add(associatedUser["EvmWallet"].S);
            }
        }

        return wallets.ToArray();
    }

    public virtual string? GetProxyAddress(string wallet)
    {
        var user = GetItem(wallet);
        if (user == null)
            return null;

        return user.TryGetValue("Proxy", out var proxy) ? proxy.S : null;
    }

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