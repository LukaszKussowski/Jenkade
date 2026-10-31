namespace Jenkade.Core;

/// <summary>Where Jenkade keeps its settings without secrets: <c>%APPDATA%\Jenkade</c> on Windows.</summary>
public sealed record SettingsFolder(string Path)
{
    public static SettingsFolder ForCurrentUser() =>
        new(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Jenkade"));
}
