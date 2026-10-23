using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace Jenkade.App.Tests;

/// <summary>Finds and clicks controls the way the user reaches them (docs/headless-testing.md).</summary>
public static class Ui
{
    // FindControl can't see controls outside the window's name scope ("Could not find parent name scope");
    // the logical tree finds any control.
    public static T Find<T>(ILogical root, string name) where T : Control =>
        root.GetLogicalDescendants().OfType<T>().Single(control => control.Name == name);

    /// <summary>Clicks a button or menu item so its Command runs, then runs pending UI jobs.</summary>
    public static void Click(Control control)
    {
        // A Button runs its Command only when clicked, not when ClickEvent is raised: click it through its automation
        // peer. A MenuItem runs its Command on ClickEvent, while its peer reports it disabled inside a closed submenu.
        if (control is MenuItem menuItem)
            menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        else
            ControlAutomationPeer.CreatePeerForElement(control).GetProvider<IInvokeProvider>()!.Invoke();
        Dispatcher.UIThread.RunJobs();
    }
}
