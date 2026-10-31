using Jenkade.Core;

namespace Jenkade.Testing;

/// <summary>Stands in for the per-user application data folder; deleted on dispose.</summary>
public sealed class TemporarySettingsFolder : IDisposable
{
    public TemporarySettingsFolder() =>
        Folder = new SettingsFolder(Directory.CreateTempSubdirectory("jenkade-settings-").FullName);

    public SettingsFolder Folder { get; }

    public void Dispose() => Directory.Delete(Folder.Path, recursive: true);
}
