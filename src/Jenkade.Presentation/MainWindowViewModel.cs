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
        (Connection, SavedConnectionUnreadable) = Shown(connections.Load());
    }

    /// <summary>The one Connection (ADR 0005), or null before the user sets it up.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionName), nameof(NoConnectionMessage))]
    public partial Connection? Connection { get; private set; }

    /// <summary>The settings file was damaged: the user sets the Connection up again.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoConnectionMessage))]
    public partial bool SavedConnectionUnreadable { get; private set; }

    /// <summary>The Connection's display name, or its address when it has none.</summary>
    public string? ConnectionName =>
        Connection is null ? null : string.IsNullOrEmpty(Connection.DisplayName) ? Connection.Address.ToString() : Connection.DisplayName;

    /// <summary>What the main window says while there's no Connection, or null when there is one.</summary>
    public string? NoConnectionMessage =>
        Connection is not null ? null : SavedConnectionUnreadable ? Strings.SavedConnectionUnreadable : Strings.NoConnection;

    private static (Connection? Connection, bool Unreadable) Shown(SavedConnection saved) => saved switch
    {
        Connection connection => (connection, false),
        SavedConnection.None => (null, false),
        SavedConnection.Unreadable => (null, true),
    };

    [RelayCommand]
    private async Task ShowConnectionSettingsAsync()
    {
        if (await dialogs.ShowConnectionSettingsAsync() is { } saved)
            (Connection, SavedConnectionUnreadable) = (saved, false);
    }

    [RelayCommand]
    private Task ShowAboutAsync() => dialogs.ShowAboutAsync();
}
