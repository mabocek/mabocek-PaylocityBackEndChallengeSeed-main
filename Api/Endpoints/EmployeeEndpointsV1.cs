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
            .Produces<GetEmployeeDto>(200)
            .Produces(404)
            .Produces(400);

        employeeGroup.MapGet("", GetAllEmployees)
            .WithName("GetAllEmployees")
            .WithSummary("Get employees with pagination and optional sorting")
            .WithDescription("Get employees with pagination support (default page=1, pageSize=10). Supports sorting by firstName, lastName, salary, dateOfBirth.")
            .Produces<PagedResult<GetEmployeeDto>>(200)
            .Produces(400);

        employeeGroup.MapGet("{id:int}/paycheck", GetEmployeePaycheck)
            .WithName("GetEmployeePaycheck")
            .WithSummary("Get paycheck calculation for employee")
            .Produces<GetPaycheckDto>(200)
            .Produces(404)
            .Produces(400);

        employeeGroup.MapPost("", CreateEmployee)
            .WithName("CreateEmployee")
            .WithSummary("Create a new employee")
            .Produces<GetEmployeeDto>(201)
            .Produces(400);

        employeeGroup.MapPut("{id:int}", UpdateEmployee)
            .WithName("UpdateEmployee")
            .WithSummary("Update an existing employee")
            .Produces<GetEmployeeDto>(200)
            .Produces(404)
            .Produces(400);

        employeeGroup.MapDelete("{id:int}", DeleteEmployee)
            .WithName("DeleteEmployee")
            .WithSummary("Delete an employee")
            .Produces(200)
            .Produces(404)
            .Produces(400);
    }

    private static async Task<IResult> GetEmployeeById(int id, IEmployeeService employeeService)
    {
        try
        {
            var result = await employeeService.GetEmployeeByIdAsync(id);

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

    private static async Task<IResult> GetAllEmployees(
        IEmployeeService employeeService,
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string? sortOrder = null)
    {
        try
        {
            var result = await employeeService.GetEmployeesPagedAsync(page, pageSize, sortBy, sortOrder);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> GetEmployeePaycheck(int id, IEmployeeService employeeService)
    {
        try
        {
            var result = await employeeService.GetEmployeePaycheckAsync(id);

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

    private static async Task<IResult> CreateEmployee([FromBody] CreateEmployeeRequest request, IEmployeeService employeeService)
    {
        try
        {
            var result = await employeeService.CreateEmployeeAsync(
                request.FirstName,
                request.LastName,
                request.Salary,
                request.DateOfBirth);

            return Results.Created($"/api/v1/employees/{result.Id}", result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request, IEmployeeService employeeService)
    {
        try
        {
            var result = await employeeService.UpdateEmployeeAsync(
                id,
                request.FirstName,
                request.LastName,
                request.Salary,
                request.DateOfBirth);

            return Results.Ok(result);
        }
        catch (ArgumentException)
        {
            return Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> DeleteEmployee(int id, IEmployeeService employeeService)
    {
        try
        {
            await employeeService.DeleteEmployeeAsync(id);
            return Results.Ok();
        }
        catch (ArgumentException)
        {
            return Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
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
