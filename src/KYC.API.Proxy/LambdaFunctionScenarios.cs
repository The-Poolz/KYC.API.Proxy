using KYC.API.Proxy.Models;
using KYC.API.Proxy.Utils;

namespace KYC.API.Proxy;

public class LambdaFunctionScenarios 
{
    internal LambdaFunctionScenarios(HttpCall httpCall, DynamoDb dynamoDb)
    {
        _httpCall = httpCall;
        _dynamoDb = dynamoDb;
    }

    private readonly HttpCall _httpCall;
    private readonly DynamoDb _dynamoDb;
    internal OutputData? HandleBlockPassResponse(InputData request)
    {
        var response = _httpCall.GetBlockPassResponse(request.Address);
        return response.Status == RequestStatus.success ? new OutputData(response) : null;
    }

    internal OutputData? HandleProxyAddress(InputData request)
    {
        var proxy = _dynamoDb.GetProxyAddress(request.Address);
        if (proxy == null) return null;

        var response = _httpCall.GetBlockPassResponse(proxy);
        return response.Status == RequestStatus.success ? new OutputData(response, proxy) : null;
    }

    internal OutputData? HandleValidWallet(InputData request)
    {
        var proxy = _dynamoDb.GetProxyAddress(request.Address);

        var validWallet = _dynamoDb.GetWallets(request.Address)
            .Select(wallet => new { Wallet = wallet, Response = _httpCall.GetBlockPassResponse(wallet) })
            .FirstOrDefault(w => w.Response.Status == RequestStatus.success);

        if (validWallet == null) return null;

        if (proxy != validWallet.Wallet)
            _dynamoDb.UpdateItem(request.Address, validWallet.Wallet);

        return new OutputData(validWallet.Response, validWallet.Wallet);
    }
}