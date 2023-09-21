using Amazon.Lambda.Core;
using KYC.API.Proxy.Utils;
using KYC.API.Proxy.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy
{
    public class LambdaFunction : LambdaFunctionScenarios
    {
        public LambdaFunction()
            : this(new HttpCall(), new DynamoDb())
        { }

        public LambdaFunction(HttpCall httpCall, DynamoDb dynamoDb)
            : base(httpCall, dynamoDb)
        { }

        public OutputData Run(InputData request)
        {
            if (!request.Valid)
                return OutputData.Error;

            var scenarios = new List<Func<InputData, OutputData?>>
            {
                HandleBlockPassResponse,
                HandleProxyAddress,
                HandleValidWallet
            };

            foreach (var scenario in scenarios)
            {
                var result = scenario(request);
                if (result != null)
                    return result;
            }

            return OutputData.Error;
        }
    }
}
