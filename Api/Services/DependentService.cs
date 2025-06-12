using Api.Dtos.Dependent;
using Api.Models;
using Api.Features.Dependents.Queries;
using MediatR;

namespace Api.Services;

public class DependentService : IDependentService
{
    private const string SortByFirstName = "firstname";
    private const string SortByLastName = "lastname";
    private const string SortByDateOfBirth = "dateofbirth";
    private const string SortByRelationship = "relationship";
    private const string SortByEmployeeId = "employeeid";

    private readonly IMediator _mediator;

    public DependentService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<List<GetDependentDto>> GetAllAsync(
        int? employeeId = null,
        Relationship? relationship = null,
        string? sortBy = null,
        bool ascending = true)
    {
        var dependents = await _mediator.Send(new GetAllDependentsQuery());

        // Apply filtering and sorting (this logic matches what was in the endpoint)
        var filteredDependents = dependents.AsEnumerable();

        if (employeeId.HasValue)
        {
            filteredDependents = filteredDependents.Where(d => d.EmployeeId == employeeId.Value);
        }

        if (relationship.HasValue)
        {
            filteredDependents = filteredDependents.Where(d => d.Relationship == relationship.Value);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            filteredDependents = sortBy.ToLower() switch
            {
                SortByFirstName => ascending
                    ? filteredDependents.OrderBy(d => d.FirstName)
                    : filteredDependents.OrderByDescending(d => d.FirstName),
                SortByLastName => ascending
                    ? filteredDependents.OrderBy(d => d.LastName)
                    : filteredDependents.OrderByDescending(d => d.LastName),
                SortByDateOfBirth => ascending
                    ? filteredDependents.OrderBy(d => d.DateOfBirth)
                    : filteredDependents.OrderByDescending(d => d.DateOfBirth),
                SortByRelationship => ascending
                    ? filteredDependents.OrderBy(d => d.Relationship)
                    : filteredDependents.OrderByDescending(d => d.Relationship),
                SortByEmployeeId => ascending
                    ? filteredDependents.OrderBy(d => d.EmployeeId)
                    : filteredDependents.OrderByDescending(d => d.EmployeeId),
                _ => filteredDependents.OrderBy(d => d.Id) // Default sort by Id
            };
        }
        else
        {
            // Default sorting by Id if no sortBy parameter is provided
            filteredDependents = filteredDependents.OrderBy(d => d.Id);
        }

        return filteredDependents.ToList();
    }

    public async Task<PagedResult<GetDependentDto>> GetPagedAsync(
        int page,
        int pageSize,
        int? employeeId = null,
        Relationship? relationship = null,
        string? sortBy = null,
        bool ascending = true)
    {
        return await _mediator.Send(new GetPagedDependentsQuery(
            page,
            pageSize,
            employeeId,
            relationship,
            sortBy,
            ascending));
    }

    public async Task<GetDependentDto?> GetByIdAsync(int id)
    {
        return await _mediator.Send(new GetDependentByIdQuery(id));
    }
}
