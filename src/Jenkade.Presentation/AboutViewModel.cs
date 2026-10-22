using Jenkade.Core;

namespace Jenkade.Presentation;

public sealed class AboutViewModel(ApplicationVersion version)
{
    public string Version { get; } = version.ToString();
}
