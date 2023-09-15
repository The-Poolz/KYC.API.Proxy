using Newtonsoft.Json;
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
        var scanRequest = new ScanRequest
        {
            TableName = "UserData",
            FilterExpression = "EvmWallet = :v_val OR contains(EvmWallets, :v_val)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":v_val", new AttributeValue { S = wallet } }
            }
        };

        var scanResponse = await client.ScanAsync(scanRequest);

        Console.WriteLine(JsonConvert.SerializeObject(scanResponse));

        if (scanResponse.Items.Count <= 0)
            return Array.Empty<string>();

        var evmWallets = scanResponse.Items[0]["EvmWallets"].L.Select(x => x.S).ToList();
        var evmWallet = scanResponse.Items[0]["EvmWallet"].S;

        if (wallet == evmWallet)
            return evmWallets.ToArray();

        if (!evmWallets.Contains(wallet))
            return Array.Empty<string>();

        evmWallets.Remove(wallet);
        evmWallets.Add(evmWallet);
        return evmWallets.ToArray();
    }
}