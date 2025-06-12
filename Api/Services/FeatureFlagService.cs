using Microsoft.FeatureManagement;
using Api.Features;

namespace Api.Services;

/// <summary>
/// Service interface for feature flag operations
/// </summary>
public interface IFeatureFlagService
{
    Task<bool> IsFeatureEnabledAsync(string featureName);
    Task<Dictionary<string, bool>> GetAllFeatureFlagsAsync();
    Task<bool> IsPaycheckCalculationEnabledAsync();
    Task<bool> AreDependentOperationsEnabledAsync();
    Task<bool> IsHighSalaryCalculationEnabledAsync();
    Task<bool> IsSeniorDependentSurchargeEnabledAsync();
    Task<bool> IsDetailedPaycheckBreakdownEnabledAsync();
}

/// <summary>
/// Service for feature flag operations
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<FeatureFlagService> _logger;

    public FeatureFlagService(IFeatureManager featureManager, ILogger<FeatureFlagService> logger)
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

    public async Task<Dictionary<string, bool>> GetAllFeatureFlagsAsync()
    {
        var result = new Dictionary<string, bool>();

        var allFlags = new[]
        {
            FeatureFlags.EnablePaycheckCalculation,
            FeatureFlags.EnableDependentOperations,
            FeatureFlags.EnableHighSalaryCalculation,
            FeatureFlags.EnableSeniorDependentSurcharge,
            FeatureFlags.EnableDetailedPaycheckBreakdown,
            FeatureFlags.EnableSwaggerUI,
            FeatureFlags.EnableBulkOperations,
            FeatureFlags.EnableAdvancedLogging,
            FeatureFlags.EnableCaching,
            FeatureFlags.EnableRateLimiting
        };

        foreach (var flag in allFlags)
        {
            result[flag] = await IsFeatureEnabledAsync(flag);
        }

        return result;
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
