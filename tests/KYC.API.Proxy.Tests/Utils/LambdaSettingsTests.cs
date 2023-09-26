using Moq;
using Xunit;
using SecretsManager;

namespace KYC.API.Proxy.Tests.Utils;

public class LambdaSettingsTests
{
    [Fact]
    internal void GetValue_ThrowExceptionOnKeyNotFound()
    {
        var secretManager = new Mock<SecretManager>();
        secretManager.Setup(x => x.GetSecretValue("SecretId", "SecretValue"))
            .Returns("SecretString");
        var lambdaSettings = new LambdaSettings(secretManager.Object);

        void TestCode() => lambdaSettings.GetValue("invalid key");

        var exception = Assert.Throws<ArgumentException>(TestCode);
        Assert.Equal("Unknown key: invalid key", exception.Message);
    }
}