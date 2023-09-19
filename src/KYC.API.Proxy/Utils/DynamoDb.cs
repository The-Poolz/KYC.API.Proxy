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

    public virtual string[] GetWallets(string wallet)
    {
        var user = GetItem(wallet);
        if (user == null) return Array.Empty<string>();
        if (!user.ContainsKey("EvmWallets")) return Array.Empty<string>();

        var associatedWallets = user["EvmWallets"].L.Select(x => x.S).ToArray();

        var wallets = new List<string>();
        foreach (var associatedWallet in associatedWallets)
        {
            var associatedUser = GetItem(associatedWallet);
            if (associatedUser == null) continue;
            if (!associatedUser.ContainsKey("EvmWallets")) continue;

            var isAssociatedWallet = associatedUser["EvmWallets"].L.Find(x => x.S == wallet);
            if (isAssociatedWallet == null) continue;

            wallets.Add(associatedUser["EvmWallet"].S);
        }

        return wallets.ToArray();
    }

    public virtual Dictionary<string, AttributeValue>? GetItem(string primaryKey)
    {
        var request = new GetItemRequest
        {
            TableName = "UserData",
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