namespace Jenkade.Presentation;

/// <summary>Opens secondary windows on behalf of ViewModels; implemented by the App.</summary>
public interface IDialogService
{
    Task ShowAboutAsync();
}
