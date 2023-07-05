using SecretsManager;
using EnvironmentManager;

namespace KYC.API.Proxy;

public class LambdaSettings
{
    private readonly SecretManager SecretManager;
    private readonly string SecretId = EnvManager.GetEnvironmentValue<string>("SECRET_ID");
    private readonly string SecretApiKeyName = EnvManager.GetEnvironmentValue<string>("SECRET_API_KEY");

    public static string DomainName => EnvManager.GetEnvironmentValue<string>("DOMAIN_NAME");
    public static string ClientId => EnvManager.GetEnvironmentValue<string>("CLIENT_ID");

    public string SecretApiKey => SecretManager.GetSecretValue(SecretId, SecretApiKeyName);

    public LambdaSettings(SecretManager? secretManager = null)
    {
        SecretManager = secretManager ?? new SecretManager();
    }
}