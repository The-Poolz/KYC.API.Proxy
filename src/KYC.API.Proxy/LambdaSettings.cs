using SecretsManager;
using EnvironmentManager;

namespace KYC.API.Proxy;

public class LambdaSettings
{
    private readonly SecretManager secretManager;
    private readonly string SecretId = EnvManager.GetEnvironmentValue<string>("SECRET_ID");
    private readonly string SecretApiKeyName = EnvManager.GetEnvironmentValue<string>("SECRET_API_KEY");
    public static string ClientId => EnvManager.GetEnvironmentValue<string>("CLIENT_ID");
    public virtual string SecretApiKey => secretManager.GetSecretValue(SecretId, SecretApiKeyName);

    public LambdaSettings(SecretManager? secretManager = null)
    {
        this.secretManager = secretManager ?? new SecretManager();
    }
}