using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Services;
using Microsoft.FeatureManagement;
using Api.Features;
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
            .Produces<ApiResponse<GetEmployeeDto>>(404);

        employeeGroup.MapGet("", GetAllEmployees)
            .WithName("GetAllEmployees")
            .WithSummary("Get employees with pagination and optional sorting")
            .WithDescription("Get employees with pagination support (default page=1, pageSize=10). Supports sorting by firstName, lastName, salary, dateOfBirth.")
            .Produces<ApiResponse<PagedResult<GetEmployeeDto>>>(200);

        employeeGroup.MapGet("all", GetAllEmployeesUnpaged)
            .WithName("GetAllEmployeesUnpaged")
            .WithSummary("Get all employees (unpaginated)")
            .WithDescription("Legacy endpoint that returns all employees without pagination. Use the main endpoint for better performance.")
            .Produces<ApiResponse<List<GetEmployeeDto>>>(200);

        employeeGroup.MapGet("{id:int}/paycheck",
            async (int id, IEmployeeService employeeService, IFeatureManager featureManager) =>
                await GetEmployeePaycheck(id, employeeService, featureManager))
            .WithName("GetEmployeePaycheck")
            .WithSummary("Get paycheck calculation for employee")
            .Produces<ApiResponse<GetPaycheckDto>>(200)
            .Produces<ApiResponse<GetPaycheckDto>>(404);

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
            .Produces<ApiResponse<object>>(404);
    }

    private static async Task<IResult> GetEmployeeById(int id, IEmployeeService employeeService)
    {
        try
        {
            var employee = await employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return Results.NotFound(new ApiResponse<GetEmployeeDto>
                {
                    Data = null,
                    Success = false,
                    Message = $"Employee with id {id} not found"
                });
            }

            var result = new ApiResponse<GetEmployeeDto>
            {
                Data = employee,
                Success = true
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving employee",
                detail: ex.Message,
                statusCode: 500);
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
            // Convert sort order string to boolean
            bool ascending = sortOrder?.ToLower() != "desc";

            var pagedEmployees = await employeeService.GetPagedAsync(page, pageSize, sortBy, ascending);

            var result = new ApiResponse<PagedResult<GetEmployeeDto>>
            {
                Data = pagedEmployees,
                Success = true
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving employees",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetAllEmployeesUnpaged(IEmployeeService employeeService)
    {
        try
        {
            var employees = await employeeService.GetAllAsync();

            var result = new ApiResponse<List<GetEmployeeDto>>
            {
                Data = employees,
                Success = true
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving all employees",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetEmployeePaycheck(int id, IEmployeeService employeeService, IFeatureManager featureManager)
    {
        try
        {
            // Check if paycheck calculation feature is enabled
            if (!await featureManager.IsEnabledAsync(FeatureFlags.EnablePaycheckCalculation))
            {
                return Results.BadRequest(new ApiResponse<GetPaycheckDto>
                {
                    Data = null,
                    Success = false,
                    Message = "Paycheck calculation feature is currently disabled"
                });
            }

            var paycheck = await employeeService.GetPaycheckAsync(id);

            if (paycheck == null)
            {
                return Results.NotFound(new ApiResponse<GetPaycheckDto>
                {
                    Data = null,
                    Success = false,
                    Message = $"Employee with id {id} not found"
                });
            }

            var result = new ApiResponse<GetPaycheckDto>
            {
                Data = paycheck,
                Success = true
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error calculating paycheck",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> CreateEmployee([FromBody] CreateEmployeeRequest request, IEmployeeService employeeService)
    {
        try
        {
            var employee = await employeeService.CreateAsync(
                request.FirstName,
                request.LastName,
                request.Salary,
                request.DateOfBirth);

            var result = new ApiResponse<GetEmployeeDto>
            {
                Data = employee,
                Success = true,
                Message = "Employee created successfully"
            };

            return Results.Created($"/api/v1/employees/{employee.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ApiResponse<GetEmployeeDto>
            {
                Data = null,
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error creating employee",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request, IEmployeeService employeeService)
    {
        try
        {
            var employee = await employeeService.UpdateAsync(
                id,
                request.FirstName,
                request.LastName,
                request.Salary,
                request.DateOfBirth);

            if (employee == null)
            {
                return Results.NotFound(new ApiResponse<GetEmployeeDto>
                {
                    Data = null,
                    Success = false,
                    Message = $"Employee with id {id} not found"
                });
            }

            var result = new ApiResponse<GetEmployeeDto>
            {
                Data = employee,
                Success = true,
                Message = "Employee updated successfully"
            };

            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ApiResponse<GetEmployeeDto>
            {
                Data = null,
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error updating employee",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteEmployee(int id, IEmployeeService employeeService)
    {
        try
        {
            var success = await employeeService.DeleteAsync(id);

            if (!success)
            {
                return Results.NotFound(new ApiResponse<object>
                {
                    Data = null,
                    Success = false,
                    Message = $"Employee with id {id} not found"
                });
            }

            var result = new ApiResponse<object>
            {
                Data = null,
                Success = true,
                Message = "Employee deleted successfully"
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error deleting employee",
                detail: ex.Message,
                statusCode: 500);
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
