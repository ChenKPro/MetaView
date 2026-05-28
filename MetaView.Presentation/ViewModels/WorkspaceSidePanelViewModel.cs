using System.Collections.ObjectModel;
using MetaView.Services.Interfaces;
using Prism.Events;

namespace MetaView.Presentation.ViewModels;

/// <summary>
/// Exposes workspace context, task list, and workflow logs.
/// </summary>
public sealed class WorkspaceSidePanelViewModel : MetaView.Presentation.Infrastructure.BindableBase
{
    private WorkflowLogEntry? _selectedLogEntry;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceSidePanelViewModel" /> class.
    /// </summary>
    public WorkspaceSidePanelViewModel(IEventAggregator eventAggregator)
    {
        WorkflowLogs =
        [
            new WorkflowLogEntry(
                DateTimeOffset.Now.AddMinutes(-2),
                "Camera frame timeout",
                IsError: true,
                DiagnosticHint: "检查相机连接、曝光时间和触发模式；若正在采集，先停止 Live 后重新初始化相机。"),
            new WorkflowLogEntry(DateTimeOffset.Now.AddMinutes(-4), "System ready"),
            new WorkflowLogEntry(DateTimeOffset.Now.AddMinutes(-3), "Stage connected")
        ];

        SelectedLogEntry = WorkflowLogs.FirstOrDefault(entry => entry.IsError);

        eventAggregator
            .GetEvent<WorkflowLogPublishedEvent>()
            .Subscribe(OnWorkflowLogPublished, ThreadOption.UIThread, keepSubscriberReferenceAlive: false);
    }

    /// <summary>
    /// Gets workflow log entries shown in the side panel.
    /// </summary>
    public ObservableCollection<WorkflowLogEntry> WorkflowLogs { get; }

    public WorkflowLogEntry? SelectedLogEntry
    {
        get => _selectedLogEntry;
        private set
        {
            if (SetProperty(ref _selectedLogEntry, value))
            {
                RaisePropertyChanged(nameof(LogSummaryText));
                RaisePropertyChanged(nameof(DiagnosticHintText));
                RaisePropertyChanged(nameof(HasDiagnosticHint));
            }
        }
    }

    public string LogSummaryText => $"{WorkflowLogs.Count(entry => entry.IsError)} Error";

    public string DiagnosticHintText => SelectedLogEntry?.DiagnosticHint ?? string.Empty;

    public bool HasDiagnosticHint => !string.IsNullOrWhiteSpace(DiagnosticHintText);

    public void SelectLogEntry(WorkflowLogEntry entry)
    {
        SelectedLogEntry = ReferenceEquals(SelectedLogEntry, entry) ? null : entry;
    }

    private void OnWorkflowLogPublished(WorkflowLogEntry entry)
    {
        WorkflowLogs.Insert(0, entry);
        if (entry.IsError)
        {
            SelectedLogEntry = entry;
        }

        while (WorkflowLogs.Count > 64)
        {
            WorkflowLogs.RemoveAt(WorkflowLogs.Count - 1);
        }

        RaisePropertyChanged(nameof(LogSummaryText));
    }
}
