namespace MetaView.Core.Imaging.Signal;

/// <summary>
/// Represents the image and trace output of one signal imaging processing pass.
/// </summary>
public sealed record SignalImagingResult(SignalImageFrame ImageFrame, SignalTraceFrame TraceFrame);
