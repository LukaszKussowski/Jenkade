using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jenkade.Core;

namespace Jenkade.Presentation;

public sealed partial class MainWindowViewModel(IDialogService dialogs, ConnectionStore connections) : ObservableObject
{
    /// <summary>The one Connection (ADR 0005), or null before the user sets it up.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionName))]
    public partial Connection? Connection { get; private set; } = connections.Load();

    /// <summary>The Connection's display name, or its address when it has none.</summary>
    public string? ConnectionName =>
        Connection is null ? null : string.IsNullOrEmpty(Connection.DisplayName) ? Connection.Address.ToString() : Connection.DisplayName;

    [RelayCommand]
    private async Task ShowConnectionSettingsAsync()
    {
        if (await dialogs.ShowConnectionSettingsAsync() is { } saved)
            Connection = saved;
    }

    [RelayCommand]
    private Task ShowAboutAsync() => dialogs.ShowAboutAsync();
}
