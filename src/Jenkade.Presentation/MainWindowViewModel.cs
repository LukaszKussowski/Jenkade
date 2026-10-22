using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Jenkade.Presentation;

public sealed partial class MainWindowViewModel(IDialogService dialogs) : ObservableObject
{
    [RelayCommand]
    private Task ShowAboutAsync() => dialogs.ShowAboutAsync();
}
