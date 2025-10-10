using UrlFiller;
using UrlFiller.Resolver;
using EnvironmentManager.Extensions;

namespace KYC.API.Proxy;

public class LambdaSettings : IValueResolver
{
    private const string ClientIdKey = "ClientId";
    private const string UserAddressKey = "UserAddress";
    private readonly URLParser parser;
    public virtual string Url(string address)
    {
        tempUserAddress = address;
        return parser.ParseUrl(Env.BLOCKPASS_URI.GetRequired());
    }

    private string tempUserAddress = string.Empty;

    public LambdaSettings()
    {
        var valueResolvers = new Dictionary<string, IValueResolver>
        {
            [ClientIdKey] = this,
            [UserAddressKey] = this
        };
        parser = new URLParser(valueResolvers);
    }

    public string GetValue(string input)
    {
        return input switch
        {
            ClientIdKey => Env.CLIENT_ID.GetRequired(),
            UserAddressKey => tempUserAddress,
            _ => throw new ArgumentException($"Unknown key: {input}")
        };
    }
}