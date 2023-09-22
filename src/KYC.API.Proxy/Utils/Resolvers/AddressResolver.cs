using UrlFiller.Resolver;

namespace KYC.API.Proxy.Utils.Resolvers;

public class AddressResolver : IValueResolver
{
    public string GetValue(string input)
    {
        return input;
    }
}