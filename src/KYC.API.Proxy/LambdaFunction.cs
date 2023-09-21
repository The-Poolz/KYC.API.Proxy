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

    public async Task<OutputData> RunAsync(InputData request)
    {
        if (!request.Valid)
        {
            return OutputData.Error;
        }
        var response = httpCall.GetBlockPassResponse(request.Address);

        if (response.Status != RequestStatus.error)
        {
            return new(response);
        }

        var proxy = dynamoDb.GetProxyAddress(request.Address);
        if (proxy != null)
        {
            response = httpCall.GetBlockPassResponse(proxy);

            if (response.Status != RequestStatus.error)
            {
                return BuildOutputData(response, proxy);
            }
        }

        var wallets = dynamoDb.GetWallets(request.Address);
        foreach (var wallet in wallets)
        {
            response = httpCall.GetBlockPassResponse(wallet);
            if (response.Status != RequestStatus.error)
            {
                if (proxy != wallet)
                {
                    await dynamoDb.UpdateItemAsync(request.Address, wallet);
                }
                return BuildOutputData(response, wallet);
            }
        }

        return new OutputData
        {
            RequestStatus = RequestStatus.error
        };
    }

    private static OutputData BuildOutputData(Response response, string? proxy = null)
    {
        return new OutputData
        {
            RequestStatus = response.Status,
            Status = response.Data.Status,
            Name = response.Data.Identities.GivenName.Value,
            Proxy = proxy
        };
    }
}
