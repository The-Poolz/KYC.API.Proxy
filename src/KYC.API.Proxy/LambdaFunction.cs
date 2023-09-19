using Amazon.Lambda.Core;
using KYC.API.Proxy.Utils;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy;

public class LambdaFunction
{
    private readonly HttpCall httpCall;
    private readonly DynamoDb dynamoDb;

    public LambdaFunction()
        : this(new HttpCall(), new DynamoDb())
    { }

    public LambdaFunction(HttpCall httpCall, DynamoDb dynamoDb)
    {
        this.httpCall = httpCall;
        this.dynamoDb = dynamoDb;
    }

    public JToken Run(JObject request)
    {
        if (!request.ContainsKey("Address") || request["Address"]!.ToString() == "0x0000000000000000000000000000000000000000")
        {
            return new JObject
            {
                { "StatusCode", 403 }
            };
        }
        var address = request["Address"]!.ToString();

        var response = httpCall.GetBlockPassResponse(address);

        if (response.ContainsKey("status") && response["status"]?.ToString() != "error")
        {
            return response;
        }

        var wallets = dynamoDb.GetWallets(address);
        foreach (var wallet in wallets)
        {
            response = httpCall.GetBlockPassResponse(wallet);
            if (response.ContainsKey("status") && response["status"]?.ToString() != "error")
            {
                return response;
            }
        }

        return new JObject
        {
            new JProperty("status", "error")
        };
    }
}
