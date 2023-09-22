using UrlFiller.Resolver;

namespace KYC.API.Proxy.Utils.Resolvers;

public class AddressResolver : IValueResolver
{
    private readonly string address;

    public AddressResolver(string address)
    {
        this.address = address;
    }

    public string GetValue(string input)
    {
        return address;
    }
}