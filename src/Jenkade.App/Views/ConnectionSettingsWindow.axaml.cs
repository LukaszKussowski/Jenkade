using Avalonia.Controls;
using Avalonia.Interactivity;
using Jenkade.Presentation;

namespace Jenkade.App.Views;

public partial class ConnectionSettingsWindow : Window
{
    // Required by the Avalonia XAML loader and previewer.
    public ConnectionSettingsWindow() => InitializeComponent();

    public ConnectionSettingsWindow(ConnectionSettingsViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.Saved += (_, connection) => Close(connection);
        Closing += (_, _) => viewModel.Close();
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
