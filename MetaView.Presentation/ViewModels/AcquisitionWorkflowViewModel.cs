using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using MetaView.Core.Imaging.Brightfield;
using MetaView.Core.Parameters;
using MetaView.Presentation.Core;
using MetaView.Presentation.Infrastructure;
using MetaView.Presentation.Services;
using MetaView.Services.Interfaces;
using Prism.Events;
using AsyncDelegateCommand = MetaView.Presentation.Infrastructure.AsyncDelegateCommand;
using DelegateCommand = MetaView.Presentation.Infrastructure.DelegateCommand;

namespace MetaView.Presentation.ViewModels;

/// <summary>
/// Coordinates acquisition setup, validation, and simulated task execution.
/// </summary>
public sealed class AcquisitionWorkflowViewModel : MetaView.Presentation.Infrastructure.BindableBase
{
    private readonly IAcquisitionService _acquisitionService;
    private readonly IExperimentWorkflowRunner _workflowRunner;
    private readonly IBrightfieldCameraCapability _brightfieldCameraCapability;
    private readonly IRuntimeParameterProvider _parameterProvider;
    private readonly IEventAggregator _eventAggregator;
    private CancellationTokenSource? _acquisitionCts;
    private ImagingModality _selectedModality = ImagingModality.Srs;
    private AcquisitionState _state = AcquisitionState.Ready;
    private bool _useModal = true;
    private bool _useLargeArea;
    private bool _use3D;
    private bool _useTimeLapse;
    private bool _autoSave = true;
    private string _savePath = @"D:\QLY\20260430";
    private string _saveName = "Z4X-800-round-trip";
    private int _currentFrame;
    private int _totalFrames;
    private string _progressText = "Ready for live preview";
    private int _selectedStepIndex;
    private bool _showAdvancedContent = true;
    private bool _showSaveContent = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcquisitionWorkflowViewModel" /> class.
    /// </summary>
    public AcquisitionWorkflowViewModel(
        IAcquisitionService acquisitionService,
        IExperimentWorkflowRunner workflowRunner,
        IBrightfieldCameraCapability brightfieldCameraCapability,
        IRuntimeParameterProvider parameterProvider,
        IEventAggregator eventAggregator,
        ScanSetupViewModel scan)
    {
        _acquisitionService = acquisitionService;
        _workflowRunner = workflowRunner;
        _brightfieldCameraCapability = brightfieldCameraCapability;
        _parameterProvider = parameterProvider;
        _eventAggregator = eventAggregator;
        Scan = scan;
        Scan.SettingsChanged += (_, _) => RefreshRecipe();

        Windows =
        [
            new SpectralWindow
            {
                Window = "2750-3075",
                Central = 2915,
                Pump = 801,
                Region = "High",
                EffectiveWindow = 2915,
                Step = 3,
                BboOffset = 0,
                IsSelected = true
            },
            new SpectralWindow
            {
                Window = "2800-3020",
                Central = 2920,
                Pump = 801,
                Region = "Medium",
                EffectiveWindow = 2920,
                Step = 5,
                BboOffset = 0,
                IsSelected = false
            }
        ];

        ModalityOptions = [ImagingModality.Srs, ImagingModality.Tpef, ImagingModality.Dc, ImagingModality.Multimodal];
        LivePreviewCommand = new AsyncDelegateCommand(StartLiveAsync, CanStart);
        SingleCaptureCommand = new AsyncDelegateCommand(CaptureSingleAsync, CanStart);
        RunTaskCommand = new AsyncDelegateCommand(RunTaskAsync, CanRunTask);
        RunDemoWorkflowCommand = new AsyncDelegateCommand(RunDemoWorkflowAsync, CanRunDemoWorkflow);
        AbortCommand = new DelegateCommand(Abort, CanAbort);
        BrowseSavePathCommand = new DelegateCommand(BrowseSavePath);
        GenerateSaveNameCommand = new DelegateCommand(GenerateSaveName);
        RefreshRecipe();
    }

    public ScanSetupViewModel Scan { get; }
    public IReadOnlyList<ImagingModality> ModalityOptions { get; }
    public ObservableCollection<SpectralWindow> Windows { get; }
    public ObservableCollection<string> ModalitiesInUse { get; } =
    [
        "Fwd-SRS",
        "Epi-SRS",
        "Fwd-TPEF",
        "Epi-TPEF",
        "Fwd-SRS-DC",
        "Epi-SRS-DC"
    ];

    public ObservableCollection<string> LargeAreaPoints { get; } =
    [
        "ROI 1 / P1 / X 4892.67 / Y -630.24 / Z 22921.20",
        "ROI 1 / P2 / X 5120.32 / Y -630.24 / Z 22921.20",
        "ROI 1 / P3 / X 5120.32 / Y -402.59 / Z 22921.20"
    ];

    public ObservableCollection<string> DepthPoints { get; } =
    [
        "Top / Z 22927.20 um",
        "Bottom / Z 22912.20 um"
    ];

    public ObservableCollection<string> SavedPositions { get; } =
    [
        "Pos 01 / Live field / Ready",
        "Pos 02 / Brightfield ROI / Waiting",
        "Pos 03 / Calibration area / Captured"
    ];

    public ICommand LivePreviewCommand { get; }
    public ICommand SingleCaptureCommand { get; }
    public ICommand RunTaskCommand { get; }
    public ICommand RunDemoWorkflowCommand { get; }
    public ICommand AbortCommand { get; }
    public ICommand BrowseSavePathCommand { get; }
    public ICommand GenerateSaveNameCommand { get; }

    public ImagingModality SelectedModality
    {
        get => _selectedModality;
        set
        {
            if (SetProperty(ref _selectedModality, value))
            {
                RefreshRecipe();
            }
        }
    }

    public AcquisitionState State
    {
        get => _state;
        private set
        {
            if (SetProperty(ref _state, value))
            {
                RaisePropertyChanged(nameof(IsBusy));
                RaisePropertyChanged(nameof(StateText));
                RaiseCommandStates();
            }
        }
    }

    public bool UseModal { get => _useModal; set { if (SetProperty(ref _useModal, value)) RefreshRecipe(); } }
    public bool UseLargeArea
    {
        get => _useLargeArea;
        set
        {
            if (SetProperty(ref _useLargeArea, value))
            {
                RaisePropertyChanged(nameof(ShowScanSequence));
                RefreshRecipe();
            }
        }
    }

    public bool Use3D
    {
        get => _use3D;
        set
        {
            if (SetProperty(ref _use3D, value))
            {
                RaisePropertyChanged(nameof(ShowScanSequence));
                RefreshRecipe();
            }
        }
    }
    public bool UseTimeLapse { get => _useTimeLapse; set { if (SetProperty(ref _useTimeLapse, value)) RefreshRecipe(); } }
    public bool AutoSave { get => _autoSave; set { if (SetProperty(ref _autoSave, value)) RefreshRecipe(); } }
    public string SavePath { get => _savePath; set { if (SetProperty(ref _savePath, value)) RefreshRecipe(); } }
    public string SaveName { get => _saveName; set { if (SetProperty(ref _saveName, value)) RefreshRecipe(); } }
    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            if (SetProperty(ref _currentFrame, value))
            {
                RaisePropertyChanged(nameof(ProgressPercent));
            }
        }
    }

    public int TotalFrames
    {
        get => _totalFrames;
        set
        {
            if (SetProperty(ref _totalFrames, value))
            {
                RaisePropertyChanged(nameof(ProgressPercent));
            }
        }
    }
    public string ProgressText { get => _progressText; set => SetProperty(ref _progressText, value); }
    public int SelectedStepIndex { get => _selectedStepIndex; set => SetProperty(ref _selectedStepIndex, value); }
    public bool ShowAdvancedContent { get => _showAdvancedContent; set => SetProperty(ref _showAdvancedContent, value); }
    public bool ShowSaveContent { get => _showSaveContent; set => SetProperty(ref _showSaveContent, value); }
    public bool ShowScanSequence => UseLargeArea || Use3D;
    public bool IsBusy => State is AcquisitionState.LivePreview or AcquisitionState.Capturing or AcquisitionState.Acquiring;
    public string StateText => State.ToString();
    public string ValidationMessage => string.IsNullOrWhiteSpace(SaveName) ? "Save name is required" : "Recipe ready";
    public string RecipeSummary { get; private set; } = string.Empty;
    public double ProgressPercent => TotalFrames <= 0 ? 0 : CurrentFrame * 100.0 / TotalFrames;

    /// <summary>
    /// Creates the current acquisition recipe.
    /// </summary>
    public AcquisitionRecipe CreateRecipe()
    {
        return new AcquisitionRecipe(Scan.ToSettings(), SelectedModality, SavePath, SaveName, AutoSave, UseModal, UseLargeArea, Use3D, UseTimeLapse);
    }

    private void BrowseSavePath()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select save folder",
            InitialDirectory = string.IsNullOrWhiteSpace(SavePath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : SavePath
        };

        if (dialog.ShowDialog() == true)
        {
            SavePath = dialog.FolderName;
        }
    }

    private void GenerateSaveName()
    {
        var feature = Use3D ? "3D" : UseLargeArea ? "LargeArea" : UseModal ? "Modal" : "Single";
        var zoom = string.IsNullOrWhiteSpace(Scan.Zoom)
            ? "Zoom"
            : Scan.Zoom.Replace("/", "-").Replace(" ", string.Empty);
        SaveName = $"{SelectedModality}-{feature}-{zoom}-{DateTime.Now:yyyyMMdd-HHmmss}";
    }

    private async Task StartLiveAsync()
    {
        State = AcquisitionState.LivePreview;
        ProgressText = "Starting brightfield live preview";
        PublishLog("Brightfield camera: initializing live preview");

        var settingsResult = _parameterProvider.GetBrightfieldCameraSettings();
        if (!settingsResult.Success || settingsResult.Data is null)
        {
            ProgressText = settingsResult.Message;
            PublishLog($"Parameters: {settingsResult.Message}");
            State = AcquisitionState.Ready;
            return;
        }

        var initializeResult = await _brightfieldCameraCapability
            .InitializeAsync(settingsResult.Data, CancellationToken.None)
            .ConfigureAwait(true);
        if (!initializeResult.Success)
        {
            ProgressText = initializeResult.Message;
            PublishLog($"Brightfield camera: {initializeResult.Message}");
            State = AcquisitionState.Ready;
            return;
        }

        var startResult = await _brightfieldCameraCapability.StartLiveAsync(CancellationToken.None).ConfigureAwait(true);
        ProgressText = startResult.Success ? "Brightfield live preview active" : startResult.Message;
        PublishLog(startResult.Success ? "Brightfield camera: live preview active" : $"Brightfield camera: {startResult.Message}");
        if (!startResult.Success)
        {
            State = AcquisitionState.Ready;
        }
    }

    private async Task CaptureSingleAsync()
    {
        State = AcquisitionState.Capturing;
        CurrentFrame = 1;
        TotalFrames = 1;
        ProgressText = "Capturing brightfield frame";
        PublishLog("Brightfield camera: capturing single frame");

        var settingsResult = _parameterProvider.GetBrightfieldCameraSettings();
        if (!settingsResult.Success || settingsResult.Data is null)
        {
            ProgressText = settingsResult.Message;
            PublishLog($"Parameters: {settingsResult.Message}");
            State = AcquisitionState.Ready;
            return;
        }

        var initializeResult = await _brightfieldCameraCapability.InitializeAsync(settingsResult.Data, CancellationToken.None).ConfigureAwait(true);
        if (!initializeResult.Success)
        {
            ProgressText = initializeResult.Message;
            PublishLog($"Brightfield camera: {initializeResult.Message}");
            State = AcquisitionState.Ready;
            return;
        }

        var result = await _brightfieldCameraCapability.CaptureSingleAsync(CancellationToken.None).ConfigureAwait(true);
        ProgressText = result.Success ? "Brightfield frame captured" : result.Message;
        PublishLog(result.Success ? "Brightfield camera: single frame captured" : $"Brightfield camera: {result.Message}");
        State = AcquisitionState.Ready;
    }

    private async Task RunTaskAsync()
    {
        _acquisitionCts = new CancellationTokenSource();
        State = AcquisitionState.Acquiring;
        CurrentFrame = 0;
        TotalFrames = Math.Max(1, Scan.Captures);
        var progress = new Progress<AcquisitionProgress>(value =>
        {
            CurrentFrame = value.CurrentFrame;
            TotalFrames = value.TotalFrames;
            ProgressText = $"{value.Message} 路 {value.Elapsed:mm\\:ss}";
            RaisePropertyChanged(nameof(ProgressPercent));
        });

        try
        {
            await _acquisitionService.RunTaskAsync(CreateRecipe(), progress, _acquisitionCts.Token).ConfigureAwait(true);
            ProgressText = $"Task complete 路 {TotalFrames}/{TotalFrames} frames";
            State = AcquisitionState.Ready;
        }
        catch (OperationCanceledException)
        {
            ProgressText = "Task aborted";
            State = AcquisitionState.Ready;
        }
        finally
        {
            _acquisitionCts.Dispose();
            _acquisitionCts = null;
        }
    }

    private async Task RunDemoWorkflowAsync()
    {
        _acquisitionCts = new CancellationTokenSource();
        State = AcquisitionState.Acquiring;
        CurrentFrame = 0;
        TotalFrames = 7;
        ProgressText = "Running motion + DAQ demo";

        try
        {
            var templateId = SelectedModality == ImagingModality.Multimodal
                ? ExperimentRecipeCatalog.SrsBrightfieldTwoDTemplateId
                : ExperimentRecipeCatalog.SrsTwoDTemplateId;
            var result = await _workflowRunner
                .RunAsync(ExperimentRecipeCatalog.Create(templateId), _acquisitionCts.Token)
                .ConfigureAwait(true);
            var workflowResult = result.Data;

            CurrentFrame = workflowResult?.Steps.Count(step => step.Success) ?? 0;
            ProgressText = workflowResult?.Message ?? result.Message;
            State = AcquisitionState.Ready;
        }
        catch (OperationCanceledException)
        {
            ProgressText = "Demo workflow aborted";
            State = AcquisitionState.Ready;
        }
        finally
        {
            _acquisitionCts.Dispose();
            _acquisitionCts = null;
        }
    }

    private void Abort()
    {
        if (State is AcquisitionState.LivePreview)
        {
            _ = StopBrightfieldLiveAsync();
            return;
        }

        if (_acquisitionCts is null)
        {
            State = AcquisitionState.Ready;
            return;
        }

        State = AcquisitionState.Aborting;
        _acquisitionCts.Cancel();
    }

    private async Task StopBrightfieldLiveAsync()
    {
        State = AcquisitionState.Aborting;
        var result = await _brightfieldCameraCapability.StopLiveAsync(CancellationToken.None).ConfigureAwait(true);
        ProgressText = result.Success ? "Brightfield live preview stopped" : result.Message;
        PublishLog(result.Success ? "Brightfield camera: live preview stopped" : $"Brightfield camera: {result.Message}");
        State = AcquisitionState.Ready;
    }

    private void PublishLog(string message)
    {
        _eventAggregator.GetEvent<WorkflowLogPublishedEvent>().Publish(new WorkflowLogEntry(DateTimeOffset.Now, message));
    }

    private bool CanStart() => State is AcquisitionState.Ready or AcquisitionState.LivePreview;
    private bool CanRunTask() => (State is AcquisitionState.Ready or AcquisitionState.LivePreview) && !string.IsNullOrWhiteSpace(SaveName);
    private bool CanRunDemoWorkflow() => State is AcquisitionState.Ready or AcquisitionState.LivePreview;
    private bool CanAbort() => State is AcquisitionState.LivePreview or AcquisitionState.Capturing or AcquisitionState.Acquiring;

    private void RefreshRecipe()
    {
        RecipeSummary = $"{SelectedModality} | 801 nm | {Scan.Zoom} | {Scan.Width} x {Scan.Height} | {Scan.ScanMode} | {(AutoSave ? "Auto Save On" : "Manual Save")}";
        RaisePropertyChanged(nameof(RecipeSummary));
        RaisePropertyChanged(nameof(ValidationMessage));
        RaiseCommandStates();
    }

    private void RaiseCommandStates()
    {
        (LivePreviewCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
        (SingleCaptureCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
        (RunTaskCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
        (RunDemoWorkflowCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
        (AbortCommand as DelegateCommand)?.RaiseCanExecuteChanged();
    }
}

