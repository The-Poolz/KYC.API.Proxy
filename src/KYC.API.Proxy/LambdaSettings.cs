using SecretsManager;
using EnvironmentManager;
using UrlFiller;
using UrlFiller.Resolver;

namespace KYC.API.Proxy;

public class LambdaSettings : IValueResolver
{
    private const string ClientIdKey = "ClientId";
    private const string UserAddressKey = "UserAddress";
    private readonly EnvManager envManager = new();
    private readonly SecretManager secretManager;
    private readonly string SecretId;
    private readonly string SecretApiKeyName;
    private readonly string ClientId;
    private readonly string blockpassUri;
    private readonly URLParser parser;
    public virtual string SecretApiKey => secretManager.GetSecretValue(SecretId, SecretApiKeyName);
    public virtual string Url(string address)
    {
        tempUserAddress = address;
        return parser.ParseUrl(blockpassUri);
    }

    private string tempUserAddress = string.Empty;

    public LambdaSettings(SecretManager? secretManager = null)
    {
        this.secretManager = secretManager ?? new SecretManager();
        SecretId = envManager.GetEnvironmentValue<string>("SECRET_ID");
        SecretApiKeyName = envManager.GetEnvironmentValue<string>("SECRET_API_KEY");
        ClientId = envManager.GetEnvironmentValue<string>("CLIENT_ID");
        blockpassUri = envManager.GetEnvironmentValue<string>("BLOCKPASS_URI");
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
            ClientIdKey => ClientId,
            UserAddressKey => tempUserAddress,
            _ => throw new ArgumentException($"Unknown key: {input}")
        };
    }
}