using System.Reflection;

namespace Jenkade.Core;

/// <summary>
/// Jenkade's own version as computed by MinVer from git tags, e.g. <c>0.1.0-alpha.0.3</c>.
/// </summary>
public sealed class ApplicationVersion
{
    private readonly string semanticVersion;

    private ApplicationVersion(string semanticVersion) => this.semanticVersion = semanticVersion;

    public static ApplicationVersion FromAssembly(Assembly assembly)
    {
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? throw new InvalidOperationException($"{assembly.GetName().Name} has no informational version.");
        return FromInformationalVersion(informationalVersion);
    }

    /// <summary>Drops the build metadata (the <c>+commit</c> suffix the SDK appends).</summary>
    public static ApplicationVersion FromInformationalVersion(string informationalVersion)
    {
        var metadataStart = informationalVersion.IndexOf('+');
        return new ApplicationVersion(metadataStart < 0 ? informationalVersion : informationalVersion[..metadataStart]);
    }

    public override string ToString() => semanticVersion;
}
