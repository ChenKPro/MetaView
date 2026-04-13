using System.Collections.ObjectModel;
using MetaView.Services.Interfaces;
using Prism.Events;

namespace MetaView.Presentation.ViewModels;

/// <summary>
/// Exposes workspace context, task list, and workflow logs.
/// </summary>
public sealed class WorkspaceSidePanelViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceSidePanelViewModel" /> class.
    /// </summary>
    public WorkspaceSidePanelViewModel(IEventAggregator eventAggregator)
    {
        WorkflowLogs =
        [
            new WorkflowLogEntry(DateTimeOffset.Now.AddMinutes(-4), "System ready"),
            new WorkflowLogEntry(DateTimeOffset.Now.AddMinutes(-3), "Stage connected")
        ];

        eventAggregator
            .GetEvent<WorkflowLogPublishedEvent>()
            .Subscribe(OnWorkflowLogPublished, ThreadOption.UIThread, keepSubscriberReferenceAlive: false);
    }

    /// <summary>
    /// Gets workflow log entries shown in the side panel.
    /// </summary>
    public ObservableCollection<WorkflowLogEntry> WorkflowLogs { get; }

    private void OnWorkflowLogPublished(WorkflowLogEntry entry)
    {
        WorkflowLogs.Insert(0, entry);
        while (WorkflowLogs.Count > 64)
        {
            WorkflowLogs.RemoveAt(WorkflowLogs.Count - 1);
        }
    }
}
