using Amazon.Lambda.Core;
using KYC.API.Proxy.Utils;
using KYC.API.Proxy.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy;

public class LambdaFunction
{
    public const string ZeroAddress = "0x0000000000000000000000000000000000000000";
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
        if (string.IsNullOrWhiteSpace(request.Address) || request.Address == ZeroAddress)
        {
            return OutputData.Error;
        }
        var response = httpCall.GetBlockPassResponse(request.Address);

        if (response.Status != RequestStatus.error)
        {
            return new(response);
        }

        var wallets = dynamoDb.GetWallets(request.Address);
        var validResponse = wallets
            .Select(wallet => httpCall.GetBlockPassResponse(wallet))
            .FirstOrDefault(response => response.Status != RequestStatus.error);

        return validResponse == null? OutputData.Error : new(validResponse);
    }
}
