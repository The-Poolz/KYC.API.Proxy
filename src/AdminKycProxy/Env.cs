using EnvironmentManager.Attributes;

namespace AdminKycProxy;

public enum Env
{
    [EnvironmentVariable(isRequired: true)]
    SECRET_ID,
    [EnvironmentVariable(isRequired: true)]
    SECRET_API_KEY,
    [EnvironmentVariable(isRequired: true)]
    KYC_URL,
    [EnvironmentVariable(isRequired: true, type: typeof(int))]
    PAGE_SIZE
}