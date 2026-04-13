namespace MetaView.Core.Reporting;

/// <summary>
/// Describes report generation input.
/// </summary>
/// <param name="Title">Report title.</param>
/// <param name="OutputDirectory">Directory where the report should be written.</param>
/// <param name="DataProductPaths">Data product paths included in the report.</param>
public sealed record ReportGenerationRequest(
    string Title,
    string OutputDirectory,
    IReadOnlyList<string> DataProductPaths);
