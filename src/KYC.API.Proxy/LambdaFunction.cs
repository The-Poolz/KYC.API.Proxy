using Amazon.Lambda.Core;
using KYC.API.Proxy.Utils;
using KYC.API.Proxy.Models;
using Newtonsoft.Json.Linq;
using KYC.API.Proxy.Models.HttpResponse;

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
            return new OutputData
            {
                RequestStatus = RequestStatus.error
            };
        }
        var response = httpCall.GetBlockPassResponse(request.Address);

        if (response.Status != RequestStatus.error)
        {
            return BuildOutputData(response);
        }

        var wallets = dynamoDb.GetWallets(request.Address);
        foreach (var wallet in wallets)
        {
            response = httpCall.GetBlockPassResponse(wallet);
            if (response.Status != RequestStatus.error)
            {
                return BuildOutputData(response);
            }
        }

        return new OutputData
        {
            RequestStatus = RequestStatus.error
        };
    }

    private static OutputData BuildOutputData(Response response)
    {
        return new OutputData
        {
            RequestStatus = response.Status,
            Status = response.Data.Status,
            Name = response.Data.Identities.GivenName.Value
        };
    }
}
