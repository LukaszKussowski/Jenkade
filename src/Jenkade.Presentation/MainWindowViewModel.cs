using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jenkade.Core;

namespace Jenkade.Presentation;

public sealed partial class MainWindowViewModel(IDialogService dialogs, ConnectionStore connections) : ObservableObject
{
    public ObservableCollection<Connection> Connections { get; } = [.. connections.All()];

    [RelayCommand]
    private async Task AddConnectionAsync()
    {
        if (await dialogs.ShowAddConnectionAsync() is { } connection)
            Connections.Add(connection);
    }

    [RelayCommand]
    private Task ShowAboutAsync() => dialogs.ShowAboutAsync();
}
