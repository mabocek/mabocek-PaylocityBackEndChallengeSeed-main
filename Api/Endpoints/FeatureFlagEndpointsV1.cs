using Microsoft.FeatureManagement;
using Api.Features;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Api.Endpoints;

/// <summary>
/// Endpoints for managing feature flags (for demonstration and testing purposes)
/// </summary>
public static class FeatureFlagEndpointsV1
{
    public static void MapFeatureFlagEndpointsV1(this WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        var featureFlagGroup = app.MapGroup("api/v{version:apiVersion}/feature-flags")
            .WithTags("Feature Management")
            .WithDescription("Endpoints for managing feature flags")
            .WithApiVersionSet(apiVersionSet);

        featureFlagGroup.MapGet("", GetAllFeatureFlags)
            .WithName("GetAllFeatureFlags")
            .WithSummary("Get status of all feature flags")
            .WithDescription("Returns the current status (enabled/disabled) of all feature flags")
            .Produces<Dictionary<string, bool>>(200);

        featureFlagGroup.MapGet("{flagName}", GetFeatureFlag)
            .WithName("GetFeatureFlag")
            .WithSummary("Get status of a specific feature flag")
            .WithDescription("Returns whether a specific feature flag is enabled or disabled")
            .Produces<object>(200)
            .Produces<object>(404);
    }

    private static async Task<IResult> GetAllFeatureFlags(IFeatureManager featureManager)
    {
        try
        {
            var featureFlags = new Dictionary<string, bool>
            {
                { FeatureFlags.EnablePaycheckCalculation, await featureManager.IsEnabledAsync(FeatureFlags.EnablePaycheckCalculation) },
                { FeatureFlags.EnableDependentOperations, await featureManager.IsEnabledAsync(FeatureFlags.EnableDependentOperations) },
                { FeatureFlags.EnableHighSalaryCalculation, await featureManager.IsEnabledAsync(FeatureFlags.EnableHighSalaryCalculation) },
                { FeatureFlags.EnableSeniorDependentSurcharge, await featureManager.IsEnabledAsync(FeatureFlags.EnableSeniorDependentSurcharge) },
                { FeatureFlags.EnableDetailedPaycheckBreakdown, await featureManager.IsEnabledAsync(FeatureFlags.EnableDetailedPaycheckBreakdown) },
                { FeatureFlags.EnableSwaggerUI, await featureManager.IsEnabledAsync(FeatureFlags.EnableSwaggerUI) },
                { FeatureFlags.EnableBulkOperations, await featureManager.IsEnabledAsync(FeatureFlags.EnableBulkOperations) },
                { FeatureFlags.EnableAdvancedLogging, await featureManager.IsEnabledAsync(FeatureFlags.EnableAdvancedLogging) },
                { FeatureFlags.EnableCaching, await featureManager.IsEnabledAsync(FeatureFlags.EnableCaching) },
                { FeatureFlags.EnableRateLimiting, await featureManager.IsEnabledAsync(FeatureFlags.EnableRateLimiting) }
            };

            return Results.Ok(featureFlags);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving feature flags",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetFeatureFlag(string flagName, IFeatureManager featureManager)
    {
        try
        {
            // Validate that the flag name exists
            var validFlags = new[]
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

            if (!validFlags.Contains(flagName))
            {
                return Results.NotFound(new
                {
                    message = $"Feature flag '{flagName}' not found",
                    availableFlags = validFlags
                });
            }

            var isEnabled = await featureManager.IsEnabledAsync(flagName);

            return Results.Ok(new
            {
                flagName = flagName,
                isEnabled = isEnabled,
                description = GetFeatureFlagDescription(flagName)
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving feature flag",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static string GetFeatureFlagDescription(string flagName)
    {
        return flagName switch
        {
            FeatureFlags.EnablePaycheckCalculation => "Controls whether paycheck calculation functionality is enabled",
            FeatureFlags.EnableDependentOperations => "Controls whether dependent CRUD operations are enabled",
            FeatureFlags.EnableHighSalaryCalculation => "Controls whether high salary calculation (>$80K) is applied",
            FeatureFlags.EnableSeniorDependentSurcharge => "Controls whether senior dependent surcharge is applied",
            FeatureFlags.EnableDetailedPaycheckBreakdown => "Controls whether detailed paycheck breakdown is included in responses",
            FeatureFlags.EnableSwaggerUI => "Controls whether Swagger UI is enabled",
            FeatureFlags.EnableBulkOperations => "Controls whether bulk operations are enabled",
            FeatureFlags.EnableAdvancedLogging => "Controls whether advanced logging is enabled",
            FeatureFlags.EnableCaching => "Controls whether response caching is enabled",
            FeatureFlags.EnableRateLimiting => "Controls whether rate limiting is enabled",
            _ => "Unknown feature flag"
        };
    }
}
