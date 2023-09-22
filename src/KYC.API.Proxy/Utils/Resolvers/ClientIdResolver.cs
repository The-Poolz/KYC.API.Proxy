using UrlFiller.Resolver;

namespace KYC.API.Proxy.Utils.Resolvers;

public class ClientIdResolver : IValueResolver
{
    private readonly LambdaSettings settings;

    public ClientIdResolver()
        : this(new LambdaSettings())
    { }
    public ClientIdResolver(LambdaSettings settings)
    {
        this.settings = settings;
    }

    public string GetValue(string input)
    {
        return settings.ClientId;
    }
}