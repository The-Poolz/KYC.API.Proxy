using SecretsManager;
using EnvironmentManager;

namespace AdminKycProxy;

internal class LambdaSettings
{
    private readonly SecretManager secretManager;
    private readonly string secretId;
    private readonly string secretApiKey;
    public string Url { get; }
    public virtual string SecretApiKey => secretManager.GetSecretValue(secretId, secretApiKey);

    public LambdaSettings(SecretManager secretManager)
    {
        this.secretManager = secretManager;
        secretId = EnvManager.GetEnvironmentValue<string>("SECRET_ID", true);
        secretApiKey = EnvManager.GetEnvironmentValue<string>("SECRET_API_KEY", true);
        Url = EnvManager.GetEnvironmentValue<string>("KYC_URL", true);
    }
}