using Avalonia.Controls;
using Jenkade.Presentation;

namespace Jenkade.App.Views;

public partial class MainWindow : Window
{
    // Required by the Avalonia XAML loader and previewer.
    public MainWindow() => InitializeComponent();

    public MainWindow(MainWindowViewModel viewModel) : this() => DataContext = viewModel;
}
