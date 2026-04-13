using MetaView.Presentation.Core;
using MetaView.Presentation.Infrastructure;
using MetaView.Services.Interfaces;
using Prism.Events;

namespace MetaView.Presentation.ViewModels;

/// <summary>
/// Exposes status bar content for the shell.
/// </summary>
public sealed class StatusBarViewModel : MetaView.Presentation.Infrastructure.BindableBase
{
    private AcquisitionState _state = AcquisitionState.Ready;
    private string _message = "System ready";

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusBarViewModel" /> class.
    /// </summary>
    public StatusBarViewModel(AcquisitionWorkflowViewModel acquisition, IEventAggregator eventAggregator)
    {
        acquisition.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(AcquisitionWorkflowViewModel.State))
            {
                State = acquisition.State;
                Message = acquisition.State switch
                {
                    AcquisitionState.LivePreview => "Live preview is active",
                    AcquisitionState.Capturing => "Capturing a single frame",
                    AcquisitionState.Acquiring => "Task acquisition in progress",
                    AcquisitionState.Aborting => "Stopping acquisition",
                    _ => "System ready"
                };
            }
        };

        eventAggregator
            .GetEvent<WorkflowLogPublishedEvent>()
            .Subscribe(entry => Message = entry.Message, ThreadOption.UIThread, keepSubscriberReferenceAlive: false);
    }

    /// <summary>
    /// Gets or sets the current acquisition state.
    /// </summary>
    public AcquisitionState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
}

