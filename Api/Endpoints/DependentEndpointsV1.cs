using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.Dtos.Dependent;
using Api.Services;
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
            async (int id, IDependentService dependentService) =>
                await GetDependentById(id, dependentService))
            .WithName("GetDependent")
            .WithSummary("Get dependent by id")
            .Produces<GetDependentDto>(200)
            .Produces(404)
            .Produces(400);

        dependentGroup.MapGet("",
            async (IDependentService dependentService,
                   int page = 1, int pageSize = 10, int? employeeId = null, string? relationship = null,
                   string? sortBy = null, string? sortOrder = null) =>
                await GetAllDependents(dependentService, page, pageSize, employeeId, relationship, sortBy, sortOrder))
            .WithName("GetAllDependents")
            .WithSummary("Get dependents with pagination, filtering and sorting")
            .WithDescription("Get dependents with pagination support (default page=1, pageSize=10). Supports filtering by employeeId and relationship, and sorting by firstName, lastName, dateOfBirth, relationship, employeeId.")
            .Produces<PagedResult<GetDependentDto>>(200)
            .Produces(400);
    }

    private static async Task<IResult> GetDependentById(int id, IDependentService dependentService)
    {
        try
        {
            var result = await dependentService.GetDependentByIdAsync(id);

            if (result == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> GetAllDependents(
        IDependentService dependentService,
        int page = 1,
        int pageSize = 10,
        int? employeeId = null,
        string? relationship = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        try
        {
            var result = await dependentService.GetDependentsPagedAsync(page, pageSize, employeeId, relationship, sortBy, sortOrder);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }
}
