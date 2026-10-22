namespace Jenkade.Core.Tests;

public class ApplicationVersionTests
{
    [Theory]
    [InlineData("0.2.0", "0.2.0")]
    [InlineData("0.1.0-alpha.0.3+4f2a1b9c0d", "0.1.0-alpha.0.3")]
    [InlineData("1.0.0-rc.1+4f2a1b9c0d", "1.0.0-rc.1")]
    public void Shows_the_semantic_version_without_build_metadata(string informationalVersion, string expected)
    {
        var version = ApplicationVersion.FromInformationalVersion(informationalVersion);

        Assert.Equal(expected, version.ToString());
    }
}
