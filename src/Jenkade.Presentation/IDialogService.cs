using Jenkade.Core;

namespace Jenkade.Presentation;

/// <summary>Opens secondary windows on behalf of ViewModels; implemented by the App.</summary>
public interface IDialogService
{
    Task ShowAboutAsync();

    /// <summary>The Connection the user saved, or null when they closed the Connection settings without saving.</summary>
    Task<Connection?> ShowConnectionSettingsAsync();
}
