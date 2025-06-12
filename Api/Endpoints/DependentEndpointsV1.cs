using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.Dtos.Dependent;
using Api.Services;
using Microsoft.FeatureManagement;
using Api.Features;
using Asp.Versioning;

namespace Api.Endpoints;

public static class DependentEndpointsV1
{
    public static void MapDependentEndpointsV1(this WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        var dependentGroup = app.MapGroup("api/v{version:apiVersion}/dependents")
            .WithTags("Dependents")
            .WithApiVersionSet(apiVersionSet);

        dependentGroup.MapGet("{id:int}",
            async (int id, IDependentService dependentService, IFeatureManager featureManager) =>
                await GetDependentById(id, dependentService, featureManager))
            .WithName("GetDependent")
            .WithSummary("Get dependent by id")
            .Produces<ApiResponse<GetDependentDto>>(200)
            .Produces<ApiResponse<GetDependentDto>>(404);

        dependentGroup.MapGet("",
            async (IDependentService dependentService, IFeatureManager featureManager,
                   int page = 1, int pageSize = 10, int? employeeId = null, string? relationship = null,
                   string? sortBy = null, string? sortOrder = null) =>
                await GetAllDependents(dependentService, featureManager, page, pageSize, employeeId, relationship, sortBy, sortOrder))
            .WithName("GetAllDependents")
            .WithSummary("Get dependents with pagination, filtering and sorting")
            .WithDescription("Get dependents with pagination support (default page=1, pageSize=10). Supports filtering by employeeId and relationship, and sorting by firstName, lastName, dateOfBirth, relationship, employeeId.")
            .Produces<ApiResponse<PagedResult<GetDependentDto>>>(200);

        dependentGroup.MapGet("all",
            async (IDependentService dependentService, IFeatureManager featureManager,
                   int? employeeId = null, string? relationship = null, string? sortBy = null, string? sortOrder = null) =>
                await GetAllDependentsUnpaged(dependentService, featureManager, employeeId, relationship, sortBy, sortOrder))
            .WithName("GetAllDependentsUnpaged")
            .WithSummary("Get all dependents (unpaginated)")
            .WithDescription("Legacy endpoint that returns all dependents without pagination. Use the main endpoint for better performance.")
            .Produces<ApiResponse<List<GetDependentDto>>>(200);
    }

    private static async Task<IResult> GetDependentById(int id, IDependentService dependentService, IFeatureManager featureManager)
    {
        try
        {
            // Check if dependent operations feature is enabled
            if (!await featureManager.IsEnabledAsync(FeatureFlags.EnableDependentOperations))
            {
                return Results.BadRequest(new ApiResponse<GetDependentDto>
                {
                    Data = null,
                    Success = false,
                    Message = "Dependent operations feature is currently disabled"
                });
            }

            var dependent = await dependentService.GetByIdAsync(id);

            if (dependent == null)
            {
                return Results.NotFound(new ApiResponse<GetDependentDto>
                {
                    Data = null,
                    Success = false,
                    Message = $"Dependent with id {id} not found"
                });
            }

            var result = new ApiResponse<GetDependentDto>
            {
                Data = dependent,
                Success = true
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving dependent",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetAllDependents(
        IDependentService dependentService,
        IFeatureManager featureManager,
        int page = 1,
        int pageSize = 10,
        int? employeeId = null,
        string? relationship = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        try
        {
            // Check if dependent operations feature is enabled
            if (!await featureManager.IsEnabledAsync(FeatureFlags.EnableDependentOperations))
            {
                return Results.BadRequest(new ApiResponse<PagedResult<GetDependentDto>>
                {
                    Data = null,
                    Success = false,
                    Message = "Dependent operations feature is currently disabled"
                });
            }

            // Convert string relationship to enum if provided
            Relationship? relationshipEnum = null;
            if (!string.IsNullOrEmpty(relationship) && Enum.TryParse<Relationship>(relationship, true, out var parsed))
            {
                relationshipEnum = parsed;
            }

            // Convert sort order string to boolean
            bool ascending = sortOrder?.ToLower() != "desc";

            var pagedDependents = await dependentService.GetPagedAsync(page, pageSize, employeeId, relationshipEnum, sortBy, ascending);

            var result = new ApiResponse<PagedResult<GetDependentDto>>
            {
                Data = pagedDependents,
                Success = true
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving dependents",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetAllDependentsUnpaged(
        IDependentService dependentService,
        IFeatureManager featureManager,
        int? employeeId = null,
        string? relationship = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        try
        {
            // Check if dependent operations feature is enabled
            if (!await featureManager.IsEnabledAsync(FeatureFlags.EnableDependentOperations))
            {
                return Results.BadRequest(new ApiResponse<List<GetDependentDto>>
                {
                    Data = null,
                    Success = false,
                    Message = "Dependent operations feature is currently disabled"
                });
            }

            // Convert string relationship to enum if provided
            Relationship? relationshipEnum = null;
            if (!string.IsNullOrEmpty(relationship) && Enum.TryParse<Relationship>(relationship, true, out var parsed))
            {
                relationshipEnum = parsed;
            }

            // Convert sort order string to boolean
            bool ascending = sortOrder?.ToLower() != "desc";

            var dependents = await dependentService.GetAllAsync(employeeId, relationshipEnum, sortBy, ascending);

            var result = new ApiResponse<List<GetDependentDto>>
            {
                Data = dependents,
                Success = true
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving all dependents",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}
