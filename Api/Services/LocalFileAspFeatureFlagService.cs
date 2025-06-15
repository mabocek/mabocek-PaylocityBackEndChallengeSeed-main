using Microsoft.FeatureManagement;
using Api.Features;

namespace Api.Services;

/// <summary>
/// Service for feature flag operations
/// </summary>
public class LocalFileAspFeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<LocalFileAspFeatureFlagService> _logger;

    public LocalFileAspFeatureFlagService(IFeatureManager featureManager, ILogger<LocalFileAspFeatureFlagService> logger)
    {
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        try
        {
            var isEnabled = await _featureManager.IsEnabledAsync(featureName);
            _logger.LogDebug("Feature flag '{FeatureName}' is {Status}", featureName, isEnabled ? "enabled" : "disabled");
            return isEnabled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature flag '{FeatureName}'", featureName);
            return false; // Default to disabled on error
        }
    }

    public async Task<bool> IsPaycheckCalculationEnabledAsync()
        => await IsFeatureEnabledAsync(FeatureFlags.EnablePaycheckCalculation);

    public async Task<bool> AreDependentOperationsEnabledAsync()
        => await IsFeatureEnabledAsync(FeatureFlags.EnableDependentOperations);

    public async Task<bool> IsHighSalaryCalculationEnabledAsync()
        => await IsFeatureEnabledAsync(FeatureFlags.EnableHighSalaryCalculation);

    public async Task<bool> IsSeniorDependentSurchargeEnabledAsync()
        => await IsFeatureEnabledAsync(FeatureFlags.EnableSeniorDependentSurcharge);

    public async Task<bool> IsDetailedPaycheckBreakdownEnabledAsync()
        => await IsFeatureEnabledAsync(FeatureFlags.EnableDetailedPaycheckBreakdown);
}
