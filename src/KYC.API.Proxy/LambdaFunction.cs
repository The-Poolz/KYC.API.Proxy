using Amazon.Lambda.Core;
using KYC.API.Proxy.Models;
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

    public OutputData Run(InputData request)
    {
        if (string.IsNullOrWhiteSpace(request.Address) || request.Address == "0x0000000000000000000000000000000000000000")
        {
            return new OutputData
            {
                Status = "error"
            };
        }
        var response = httpCall.GetBlockPassResponse(request.Address);

        if (response.ContainsKey("status") && response["status"]!.ToString() != "error")
        {
            return BuildOutputData(response);
        }

        var wallets = dynamoDb.GetWallets(request.Address);
        foreach (var wallet in wallets)
        {
            response = httpCall.GetBlockPassResponse(wallet);
            if (response.ContainsKey("status") && response["status"]!.ToString() != "error")
            {
                return BuildOutputData(response);
            }
        }

        return new OutputData
        {
            Status = "error"
        };
    }

    private OutputData BuildOutputData(JObject response)
    {
        return new OutputData
        {
            RequestStatus = response["status"]!.ToString(),
            Status = response["data"]?["status"]?.ToString(),
            Name = response["data"]?["identities"]?["given_name"]?["value"]?.ToString()
        };
    }
}
