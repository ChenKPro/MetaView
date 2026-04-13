using System.ComponentModel;
using MetaView.Presentation.Infrastructure;

namespace MetaView.Presentation.ViewModels;

/// <summary>
/// Exposes shell-level state that is shared by the main window chrome.
/// </summary>
public sealed class ShellViewModel : MetaView.Presentation.Infrastructure.BindableBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShellViewModel" /> class.
    /// </summary>
    public ShellViewModel(AcquisitionWorkflowViewModel acquisition)
    {
        Acquisition = acquisition;
        Acquisition.PropertyChanged += OnAcquisitionChanged;
    }

    /// <summary>
    /// Gets acquisition state used by shell chrome.
    /// </summary>
    public AcquisitionWorkflowViewModel Acquisition { get; }

    /// <summary>
    /// Gets the current task summary shown in the top bar.
    /// </summary>
    public string TaskSummary => Acquisition.RecipeSummary;

    private void OnAcquisitionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AcquisitionWorkflowViewModel.RecipeSummary))
        {
            RaisePropertyChanged(nameof(TaskSummary));
        }
    }
}
