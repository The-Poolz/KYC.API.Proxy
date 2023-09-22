using SecretsManager;
using EnvironmentManager;

namespace KYC.API.Proxy;

public class LambdaSettings
{
    private readonly EnvManager envManager = new();
    private readonly SecretManager secretManager;
    private readonly string SecretId;
    private readonly string SecretApiKeyName;
    public readonly string ClientId;
    public virtual string SecretApiKey => secretManager.GetSecretValue(SecretId, SecretApiKeyName);

    public LambdaSettings(SecretManager? secretManager = null)
    {
        this.secretManager = secretManager ?? new SecretManager();
        SecretId = envManager.GetEnvironmentValue<string>("SECRET_ID");
        SecretApiKeyName = envManager.GetEnvironmentValue<string>("SECRET_API_KEY");
        ClientId = envManager.GetEnvironmentValue<string>("CLIENT_ID");
    }
}