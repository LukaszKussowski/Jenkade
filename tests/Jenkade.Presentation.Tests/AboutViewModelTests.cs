using Jenkade.Core;

namespace Jenkade.Presentation.Tests;

public class AboutViewModelTests
{
    [Fact]
    public void Shows_the_application_version()
    {
        var about = new AboutViewModel(ApplicationVersion.FromInformationalVersion("0.3.1+4f2a1b9c0d"));

        Assert.Equal("0.3.1", about.Version);
    }
}
