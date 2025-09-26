using Amazon.Lambda.Core;
using KYC.API.Proxy.Utils;
using KYC.API.Proxy.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy;

public class LambdaFunction(HttpCall httpCall, DynamoDb dynamoDb) : LambdaFunctionScenarios(httpCall, dynamoDb)
{
    public LambdaFunction()
        : this(new HttpCall(), new DynamoDb())
    { }

    public OutputData Run(InputData request)
    {
        if (!request.Valid) return OutputData.Error;

        var scenarios = new List<Func<InputData, OutputData?>>
        {
            HandleBlockPassResponse,
            HandleProxyAddress,
            HandleValidWallet
        };

        foreach (var result in scenarios.Select(scenario => scenario(request)).OfType<OutputData>())
        {
            return result;
        }

        return OutputData.Error;
    }
}