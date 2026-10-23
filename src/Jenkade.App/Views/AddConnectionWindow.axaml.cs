using Avalonia.Controls;
using Avalonia.Interactivity;
using Jenkade.Presentation;

namespace Jenkade.App.Views;

public partial class AddConnectionWindow : Window
{
    // Required by the Avalonia XAML loader and previewer.
    public AddConnectionWindow() => InitializeComponent();

    public AddConnectionWindow(AddConnectionViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.Saved += (_, connection) => Close(connection);
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
