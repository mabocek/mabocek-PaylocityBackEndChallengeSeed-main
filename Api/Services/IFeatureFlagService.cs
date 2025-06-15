namespace Api.Services;

/// <summary>
/// Service interface for feature flag operations
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Checks if a specific feature is enabled by its name.
    /// </summary>
    Task<bool> IsFeatureEnabledAsync(string featureName);

    /// <summary>
    /// Checks if the paycheck calculation feature is enabled.
    /// </summary>
    Task<bool> IsPaycheckCalculationEnabledAsync();

    /// <summary>
    /// Checks if operations related to dependents are enabled.
    /// </summary>
    Task<bool> AreDependentOperationsEnabledAsync();

    /// <summary>
    /// Checks if the high salary calculation feature is enabled.
    /// </summary>
    Task<bool> IsHighSalaryCalculationEnabledAsync();

    /// <summary>
    /// Checks if the senior dependent surcharge feature is enabled.
    /// </summary>
    Task<bool> IsSeniorDependentSurchargeEnabledAsync();

    /// <summary>
    /// Checks if the detailed paycheck breakdown feature is enabled.
    /// </summary>
    Task<bool> IsDetailedPaycheckBreakdownEnabledAsync();
}