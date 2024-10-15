using EnvironmentManager.Attributes;

namespace AdminKycProxy;

public enum Env
{
    [EnvironmentVariable(type: typeof(int), isRequired: true)]
    PAGE_SIZE
}