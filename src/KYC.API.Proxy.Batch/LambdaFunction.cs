using Flurl.Http;
using Amazon.Lambda.Core;
using KYC.API.Proxy.Models;
using KYC.API.Proxy.Batch.Models;
using Net.Web3.EthereumWallet.Extensions;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KYC.API.Proxy.Batch;

public class LambdaFunction(Proxy.LambdaFunction kycFunc)
{
    public LambdaFunction() : this(new Proxy.LambdaFunction()) { }

    public async Task<IEnumerable<LambdaResponse>> RunAsync(IEnumerable<string> listOfAddress)
    {
        var responses = new List<LambdaResponse>();

        foreach (var address in listOfAddress)
        {
            var success = false;

            while (!success)
            {
                try
                {
                    var checksumAddress = address.ConvertToChecksumAddress();
                    var response = kycFunc.Run(new InputData { Address = checksumAddress });

                    responses.Add(new LambdaResponse(checksumAddress, response));
                    success = true;
                }
                catch (FlurlHttpException ex)
                {
                    Console.WriteLine($"Error processing address: {address}. Retrying...");
                    Console.WriteLine(ex.Message);
                    await Task.Delay(2500);
                }
            }
        }

        return responses;
    }
}