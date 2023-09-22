using UrlFiller;
using Flurl.Http;
using EnvironmentManager;
using UrlFiller.Resolver;
using KYC.API.Proxy.Utils.Resolvers;
using KYC.API.Proxy.Models.HttpResponse;

namespace KYC.API.Proxy.Utils;

public class HttpCall
{
    private readonly string blockpassUri;
    private readonly LambdaSettings settings;

    public HttpCall()
        : this(new LambdaSettings())
    { }

    public HttpCall(LambdaSettings settings)
    {
        var envManager = new EnvManager();

        blockpassUri = envManager.GetEnvironmentValue<string>("BLOCKPASS_URI");
        this.settings = settings;
    }

    public virtual Response GetBlockPassResponse(string address)
    {
        var valueResolvers = new Dictionary<string, IValueResolver>
        {
            ["ClientId"] = new ClientIdResolver(settings),
            ["UserAddress"] = new AddressResolver()
        };
        var parser = new URLParser(valueResolvers);
        var url = parser.ParseUrl(blockpassUri);

        return url
            .AllowHttpStatus("404")
            .WithHeader("Authorization", settings.SecretApiKey)
            .WithHeader("cache-control", "no-cache")
            .GetAsync()
            .ReceiveJson<Response>()
            .GetAwaiter()
            .GetResult();
    }
}