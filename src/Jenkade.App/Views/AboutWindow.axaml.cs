using Avalonia.Controls;
using Avalonia.Interactivity;
using Jenkade.Presentation;

namespace Jenkade.App.Views;

public partial class AboutWindow : Window
{
    // Required by the Avalonia XAML loader and previewer.
    public AboutWindow() => InitializeComponent();

    public AboutWindow(AboutViewModel viewModel) : this() => DataContext = viewModel;

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
