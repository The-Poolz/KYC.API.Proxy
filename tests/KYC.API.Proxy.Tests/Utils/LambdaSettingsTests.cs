using Xunit;

namespace KYC.API.Proxy.Tests.Utils;

public class LambdaSettingsTests
{
    [Fact]
    internal void GetValue_ThrowExceptionOnKeyNotFound()
    {
        var lambdaSettings = new LambdaSettings();

        void TestCode() => lambdaSettings.GetValue("invalid key");

        var exception = Assert.Throws<ArgumentException>(TestCode);
        Assert.Equal("Unknown key: invalid key", exception.Message);
    }
}