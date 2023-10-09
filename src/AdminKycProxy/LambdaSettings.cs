using SecretsManager;
using EnvironmentManager;

namespace AdminKycProxy;

internal class LambdaSettings
{
    private readonly EnvManager envManager = new();
    private readonly SecretManager secretManager;
    private readonly string secretId;
    private readonly string secretApiKey;
    public string ClientId { get; }
    public virtual string SecretApiKey => secretManager.GetSecretValue(secretId, secretApiKey);

    public LambdaSettings(SecretManager secretManager)
    {
        this.secretManager = secretManager;
        secretId = envManager.GetEnvironmentValue<string>("SECRET_ID");
        secretApiKey = envManager.GetEnvironmentValue<string>("SECRET_API_KEY");
        ClientId = envManager.GetEnvironmentValue<string>("CLIENT_ID");
    }
}