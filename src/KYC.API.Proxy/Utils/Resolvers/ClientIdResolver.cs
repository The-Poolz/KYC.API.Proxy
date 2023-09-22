using UrlFiller.Resolver;

namespace KYC.API.Proxy.Utils.Resolvers;

public class ClientIdResolver : IValueResolver
{
    public string GetValue(string input)
    {
        return LambdaSettings.ClientId;
    }
}