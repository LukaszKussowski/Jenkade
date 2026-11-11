using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jenkade.Core;

namespace Jenkade.Presentation;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IDialogService dialogs;

    public MainWindowViewModel(IDialogService dialogs, ConnectionStore connections)
    {
        this.dialogs = dialogs;
        (Connection, UnreadableSettingsFile) = Shown(connections.Load());
    }

    /// <summary>The one Connection (ADR 0005), or null before the user sets it up.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionName), nameof(NoConnectionMessage))]
    public partial Connection? Connection { get; private set; }

    /// <summary>The settings file that couldn't be read, and why: the user sets the Connection up again.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoConnectionMessage))]
    private partial SavedConnection.Unreadable? UnreadableSettingsFile { get; set; }

    /// <summary>The Connection's display name, or its address when it has none.</summary>
    public string? ConnectionName =>
        Connection is null ? null : string.IsNullOrEmpty(Connection.DisplayName) ? Connection.Address.ToString() : Connection.DisplayName;

    /// <summary>What the main window says while there's no Connection, or null when there is one.</summary>
    public string? NoConnectionMessage =>
        Connection is not null ? null
        : UnreadableSettingsFile is { } unreadable ? Format(Strings.SavedConnectionUnreadable, Explain(unreadable.Fault), unreadable.File)
        : Strings.NoConnection;

    private static (Connection? Connection, SavedConnection.Unreadable? Unreadable) Shown(SavedConnection saved) => saved switch
    {
        Connection connection => (connection, null),
        SavedConnection.None => (null, null),
        SavedConnection.Unreadable unreadable => (null, unreadable),
    };

    private static string Explain(SettingsFileFault fault) => fault switch
    {
        SettingsFileFault.CannotOpen cannotOpen => cannotOpen.SystemReason,
        SettingsFileFault.InvalidJson invalidJson => Format(Strings.SettingsFileInvalidJson, invalidJson.Line),
        SettingsFileFault.MissingAddress => Strings.SettingsFileMissingAddress,
        SettingsFileFault.InvalidAddress invalidAddress => Format(Strings.SettingsFileInvalidAddress, invalidAddress.Address),
        SettingsFileFault.MissingUserName => Strings.SettingsFileMissingUserName,
    };

    private static string Format(string format, params object[] arguments) => string.Format(CultureInfo.CurrentCulture, format, arguments);

    [RelayCommand]
    private async Task ShowConnectionSettingsAsync()
    {
        if (await dialogs.ShowConnectionSettingsAsync() is { } saved)
            (Connection, UnreadableSettingsFile) = (saved, null);
    }

    [RelayCommand]
    private Task ShowAboutAsync() => dialogs.ShowAboutAsync();
}
