using Amazon.Lambda.Core;
using KYC.API.Proxy.Utils;
using KYC.API.Proxy.Models;

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
                RequestStatus = "error"
            };
        }
        var response = httpCall.GetBlockPassResponse(request.Address);

        if (response.RequestStatus != "error")
        {
            return response;
        }

        var wallets = dynamoDb.GetWallets(request.Address);
        foreach (var wallet in wallets)
        {
            response = httpCall.GetBlockPassResponse(wallet);
            if (response.RequestStatus != "error")
            {
                return response;
            }
        }

        return new OutputData
        {
            RequestStatus = "error"
        };
    }
}
