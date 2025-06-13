using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Services;
using Asp.Versioning;

namespace Api.Endpoints;

public static class EmployeeEndpointsV1
{
    public static void MapEmployeeEndpointsV1(this WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        var employeeGroup = app.MapGroup("api/v{version:apiVersion}/employees")
            .WithTags("Employees")
            .WithApiVersionSet(apiVersionSet);

        employeeGroup.MapGet("{id:int}", GetEmployeeById)
            .WithName("GetEmployee")
            .WithSummary("Get employee by id")
            .Produces<ApiResponse<GetEmployeeDto>>(200)
            .Produces<ApiResponse<GetEmployeeDto>>(404)
            .Produces<ApiResponse<GetEmployeeDto>>(400);

        employeeGroup.MapGet("", GetAllEmployees)
            .WithName("GetAllEmployees")
            .WithSummary("Get employees with pagination and optional sorting")
            .WithDescription("Get employees with pagination support (default page=1, pageSize=10). Supports sorting by firstName, lastName, salary, dateOfBirth.")
            .Produces<ApiResponse<PagedResult<GetEmployeeDto>>>(200)
            .Produces<ApiResponse<PagedResult<GetEmployeeDto>>>(400);

        employeeGroup.MapGet("{id:int}/paycheck", GetEmployeePaycheck)
            .WithName("GetEmployeePaycheck")
            .WithSummary("Get paycheck calculation for employee")
            .Produces<ApiResponse<GetPaycheckDto>>(200)
            .Produces<ApiResponse<GetPaycheckDto>>(404)
            .Produces<ApiResponse<GetPaycheckDto>>(400);

        employeeGroup.MapPost("", CreateEmployee)
            .WithName("CreateEmployee")
            .WithSummary("Create a new employee")
            .Produces<ApiResponse<GetEmployeeDto>>(201)
            .Produces<ApiResponse<GetEmployeeDto>>(400);

        employeeGroup.MapPut("{id:int}", UpdateEmployee)
            .WithName("UpdateEmployee")
            .WithSummary("Update an existing employee")
            .Produces<ApiResponse<GetEmployeeDto>>(200)
            .Produces<ApiResponse<GetEmployeeDto>>(404)
            .Produces<ApiResponse<GetEmployeeDto>>(400);

        employeeGroup.MapDelete("{id:int}", DeleteEmployee)
            .WithName("DeleteEmployee")
            .WithSummary("Delete an employee")
            .Produces<ApiResponse<object>>(200)
            .Produces<ApiResponse<object>>(404)
            .Produces<ApiResponse<object>>(400);
    }

    private static async Task<IResult> GetEmployeeById(int id, IEmployeeService employeeService)
    {
        var result = await employeeService.GetEmployeeByIdAsync(id);

        if (!result.Success && result.Message?.Contains("not found") == true)
        {
            return Results.NotFound(result);
        }

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetAllEmployees(
        IEmployeeService employeeService,
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string? sortOrder = null)
    {
        var result = await employeeService.GetEmployeesPagedAsync(page, pageSize, sortBy, sortOrder);

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetEmployeePaycheck(int id, IEmployeeService employeeService)
    {
        var result = await employeeService.GetEmployeePaycheckAsync(id);

        if (!result.Success && result.Message?.Contains("not found") == true)
        {
            return Results.NotFound(result);
        }

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateEmployee([FromBody] CreateEmployeeRequest request, IEmployeeService employeeService)
    {
        var result = await employeeService.CreateEmployeeAsync(
            request.FirstName,
            request.LastName,
            request.Salary,
            request.DateOfBirth);

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Created($"/api/v1/employees/{result.Data!.Id}", result);
    }

    private static async Task<IResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request, IEmployeeService employeeService)
    {
        var result = await employeeService.UpdateEmployeeAsync(
            id,
            request.FirstName,
            request.LastName,
            request.Salary,
            request.DateOfBirth);

        if (!result.Success && result.Message?.Contains("not found") == true)
        {
            return Results.NotFound(result);
        }

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteEmployee(int id, IEmployeeService employeeService)
    {
        var result = await employeeService.DeleteEmployeeAsync(id);

        if (!result.Success && result.Message?.Contains("not found") == true)
        {
            return Results.NotFound(result);
        }

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }
}

/// <summary>
/// Request model for creating an employee
/// </summary>
public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    decimal Salary,
    DateOnly DateOfBirth);

/// <summary>
/// Request model for updating an employee
/// </summary>
public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    decimal Salary,
    DateOnly DateOfBirth);
