using Jenkade.Core;

namespace Jenkade.Presentation;

/// <summary>Opens secondary windows on behalf of ViewModels; implemented by the App.</summary>
public interface IDialogService
{
    Task ShowAboutAsync();

    /// <summary>The Connection the user added, or null when they closed the form without saving one.</summary>
    Task<Connection?> ShowAddConnectionAsync();
}
