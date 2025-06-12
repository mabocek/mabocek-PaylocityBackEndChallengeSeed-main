using Microsoft.FeatureManagement;
using Api.Features;

namespace Api.Middleware;

/// <summary>
/// Middleware to handle feature flag-related functionality
/// </summary>
public class FeatureFlagMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FeatureFlagMiddleware> _logger;

    public FeatureFlagMiddleware(RequestDelegate next, ILogger<FeatureFlagMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IFeatureManager featureManager)
    {
        // Add feature flag information to response headers (for debugging)
        if (await featureManager.IsEnabledAsync(FeatureFlags.EnableAdvancedLogging))
        {
            var enabledFeatures = new List<string>();

            var allFlags = new[]
            {
                FeatureFlags.EnablePaycheckCalculation,
                FeatureFlags.EnableDependentOperations,
                FeatureFlags.EnableHighSalaryCalculation,
                FeatureFlags.EnableSeniorDependentSurcharge,
                FeatureFlags.EnableDetailedPaycheckBreakdown,
                FeatureFlags.EnableBulkOperations,
                FeatureFlags.EnableCaching,
                FeatureFlags.EnableRateLimiting
            };

            foreach (var flag in allFlags)
            {
                if (await featureManager.IsEnabledAsync(flag))
                {
                    enabledFeatures.Add(flag);
                }
            }

            context.Response.Headers.Append("X-Enabled-Features", string.Join(",", enabledFeatures));

            _logger.LogInformation("Request {Method} {Path} - Enabled features: {Features}",
                context.Request.Method,
                context.Request.Path,
                string.Join(", ", enabledFeatures));
        }

        await _next(context);
    }
}
