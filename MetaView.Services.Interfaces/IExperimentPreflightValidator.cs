using MetaView.Core.Experiments;

namespace MetaView.Services.Interfaces;

/// <summary>
/// Validates an experiment recipe before a workflow starts hardware actions.
/// </summary>
public interface IExperimentPreflightValidator
{
    /// <summary>
    /// Validates recipe structure, required capabilities, and available runtime parameters.
    /// </summary>
    ExperimentPreflightReport Validate(ExperimentRecipe recipe);
}
