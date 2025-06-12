using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Endpoints;
using Api.Services;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Text.Json;
using Api.Features;
using CreateEmployeeRequest = Api.Endpoints.CreateEmployeeRequest;
using UpdateEmployeeRequest = Api.Endpoints.UpdateEmployeeRequest;

namespace ApiTests.UnitTests;

/// <summary>
/// Unit tests for EmployeeEndpointsV1 minimal API endpoints
/// Tests all endpoint methods with various scenarios including success, failure, and edge cases
/// </summary>
public class EmployeeEndpointsV1Tests
{
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly Mock<IFeatureManager> _mockFeatureManager;

    public EmployeeEndpointsV1Tests()
    {
        _mockEmployeeService = new Mock<IEmployeeService>();
        _mockFeatureManager = new Mock<IFeatureManager>();

        // Enable features by default for testing
        _mockFeatureManager.Setup(x => x.IsEnabledAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    #region GetEmployeeById Tests

    [Fact]
    public async Task GetEmployeeById_ExistingEmployee_ReturnsOkWithEmployee()
    {
        // Arrange
        const int employeeId = 1;
        var employee = new GetEmployeeDto(1, "John", "Doe", 50000m, new DateOnly(1990, 1, 1),
            new List<Api.Dtos.Dependent.GetDependentDto>());

        _mockEmployeeService.Setup(x => x.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        // Act
        var result = await CallGetEmployeeById(employeeId);

        // Assert
        Assert.IsType<Ok<ApiResponse<GetEmployeeDto>>>(result);
        var okResult = (Ok<ApiResponse<GetEmployeeDto>>)result;
        Assert.True(okResult.Value?.Success);
        Assert.Equal(employee, okResult.Value?.Data);
    }

    [Fact]
    public async Task GetEmployeeById_NonExistentEmployee_ReturnsNotFound()
    {
        // Arrange
        const int employeeId = 999;

        _mockEmployeeService.Setup(x => x.GetByIdAsync(employeeId))
            .ReturnsAsync((GetEmployeeDto?)null);

        // Act
        var result = await CallGetEmployeeById(employeeId);

        // Assert
        Assert.IsType<NotFound<ApiResponse<GetEmployeeDto>>>(result);
        var notFoundResult = (NotFound<ApiResponse<GetEmployeeDto>>)result;
        Assert.False(notFoundResult.Value?.Success);
        Assert.Contains($"Employee with id {employeeId} not found", notFoundResult.Value?.Message);
    }

    [Fact]
    public async Task GetEmployeeById_ServiceThrowsException_ReturnsProblem()
    {
        // Arrange
        const int employeeId = 1;

        _mockEmployeeService.Setup(x => x.GetByIdAsync(employeeId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await CallGetEmployeeById(employeeId);

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
        var problemResult = (ProblemHttpResult)result;
        Assert.Equal(500, problemResult.StatusCode);
    }

    #endregion

    #region GetAllEmployees Tests

    [Fact]
    public async Task GetAllEmployees_ReturnsOkWithEmployeeList()
    {
        // Arrange
        var employees = new List<GetEmployeeDto>
        {
            new(1, "John", "Doe", 50000m, new DateOnly(1990, 1, 1), new List<Api.Dtos.Dependent.GetDependentDto>()),
            new(2, "Jane", "Smith", 60000m, new DateOnly(1985, 5, 15), new List<Api.Dtos.Dependent.GetDependentDto>())
        };

        var pagedResult = new PagedResult<GetEmployeeDto>(employees, 2, 1, 10);

        _mockEmployeeService.Setup(x => x.GetPagedAsync(1, 10, null, true))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await CallGetAllEmployees();

        // Assert
        Assert.IsType<Ok<ApiResponse<PagedResult<GetEmployeeDto>>>>(result);
        var okResult = (Ok<ApiResponse<PagedResult<GetEmployeeDto>>>)result;
        Assert.True(okResult.Value?.Success);
        Assert.Equal(pagedResult, okResult.Value?.Data);
    }

    [Fact]
    public async Task GetAllEmployees_ServiceThrowsException_ReturnsProblem()
    {
        // Arrange
        _mockEmployeeService.Setup(x => x.GetPagedAsync(1, 10, null, true))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await CallGetAllEmployees();

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
        var problemResult = (ProblemHttpResult)result;
        Assert.Equal(500, problemResult.StatusCode);
    }

    #endregion

    #region GetEmployeePaycheck Tests

    [Fact]
    public async Task GetEmployeePaycheck_FeatureEnabled_ExistingEmployee_ReturnsPaycheck()
    {
        // Arrange
        const int employeeId = 1;
        var paycheck = new GetPaycheckDto(1, "John Doe", 1923.08m, 461.54m, 1461.54m,
            new PaycheckDetailsDto(1000m, 0m, 0m, 0m, 1000m, 461.54m));

        _mockFeatureManager.Setup(x => x.IsEnabledAsync(FeatureFlags.EnablePaycheckCalculation))
            .ReturnsAsync(true);

        _mockEmployeeService.Setup(x => x.GetPaycheckAsync(employeeId))
            .ReturnsAsync(paycheck);

        // Act
        var result = await CallGetEmployeePaycheck(employeeId);

        // Assert
        Assert.IsType<Ok<ApiResponse<GetPaycheckDto>>>(result);
        var okResult = (Ok<ApiResponse<GetPaycheckDto>>)result;
        Assert.True(okResult.Value?.Success);
        Assert.Equal(paycheck, okResult.Value?.Data);
    }

    [Fact]
    public async Task GetEmployeePaycheck_FeatureDisabled_ReturnsBadRequest()
    {
        // Arrange
        const int employeeId = 1;

        _mockFeatureManager.Setup(x => x.IsEnabledAsync(FeatureFlags.EnablePaycheckCalculation))
            .ReturnsAsync(false);

        // Act
        var result = await CallGetEmployeePaycheck(employeeId);

        // Assert
        Assert.IsType<BadRequest<ApiResponse<GetPaycheckDto>>>(result);
        var badRequestResult = (BadRequest<ApiResponse<GetPaycheckDto>>)result;
        Assert.False(badRequestResult.Value?.Success);
        Assert.Contains("Paycheck calculation feature is currently disabled", badRequestResult.Value?.Message);
    }

    [Fact]
    public async Task GetEmployeePaycheck_NonExistentEmployee_ReturnsNotFound()
    {
        // Arrange
        const int employeeId = 999;

        _mockEmployeeService.Setup(x => x.GetPaycheckAsync(employeeId))
            .ReturnsAsync((GetPaycheckDto?)null);

        // Act
        var result = await CallGetEmployeePaycheck(employeeId);

        // Assert
        Assert.IsType<NotFound<ApiResponse<GetPaycheckDto>>>(result);
        var notFoundResult = (NotFound<ApiResponse<GetPaycheckDto>>)result;
        Assert.False(notFoundResult.Value?.Success);
        Assert.Contains($"Employee with id {employeeId} not found", notFoundResult.Value?.Message);
    }

    [Fact]
    public async Task GetEmployeePaycheck_ServiceThrowsException_ReturnsProblem()
    {
        // Arrange
        const int employeeId = 1;

        _mockEmployeeService.Setup(x => x.GetPaycheckAsync(employeeId))
            .ThrowsAsync(new InvalidOperationException("Calculation error"));

        // Act
        var result = await CallGetEmployeePaycheck(employeeId);

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
        var problemResult = (ProblemHttpResult)result;
        Assert.Equal(500, problemResult.StatusCode);
    }

    #endregion

    #region CreateEmployee Tests

    [Fact]
    public async Task CreateEmployee_ValidRequest_ReturnsCreatedEmployee()
    {
        // Arrange
        var request = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));
        var createdEmployee = new GetEmployeeDto(1, "John", "Doe", 50000m, new DateOnly(1990, 1, 1),
            new List<Api.Dtos.Dependent.GetDependentDto>());

        _mockEmployeeService.Setup(x => x.CreateAsync("John", "Doe", 50000m, new DateOnly(1990, 1, 1)))
            .ReturnsAsync(createdEmployee);

        // Act
        var result = await CallCreateEmployee(request);

        // Assert
        Assert.IsType<Created<ApiResponse<GetEmployeeDto>>>(result);
        var createdResult = (Created<ApiResponse<GetEmployeeDto>>)result;
        Assert.True(createdResult.Value?.Success);
        Assert.Equal(createdEmployee, createdResult.Value?.Data);
        Assert.Contains("Employee created successfully", createdResult.Value?.Message);
    }

    [Fact]
    public async Task CreateEmployee_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateEmployeeRequest("", "Doe", 50000m, new DateOnly(1990, 1, 1));

        _mockEmployeeService.Setup(x => x.CreateAsync("", "Doe", 50000m, new DateOnly(1990, 1, 1)))
            .ThrowsAsync(new ArgumentException("FirstName is required"));

        // Act
        var result = await CallCreateEmployee(request);

        // Assert
        Assert.IsType<BadRequest<ApiResponse<GetEmployeeDto>>>(result);
        var badRequestResult = (BadRequest<ApiResponse<GetEmployeeDto>>)result;
        Assert.False(badRequestResult.Value?.Success);
        Assert.Contains("FirstName is required", badRequestResult.Value?.Message);
    }

    [Fact]
    public async Task CreateEmployee_ServiceThrowsGeneralException_ReturnsProblem()
    {
        // Arrange
        var request = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));

        _mockEmployeeService.Setup(x => x.CreateAsync("John", "Doe", 50000m, new DateOnly(1990, 1, 1)))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await CallCreateEmployee(request);

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
        var problemResult = (ProblemHttpResult)result;
        Assert.Equal(500, problemResult.StatusCode);
    }

    #endregion

    #region UpdateEmployee Tests

    [Fact]
    public async Task UpdateEmployee_ValidRequest_ReturnsUpdatedEmployee()
    {
        // Arrange
        const int employeeId = 1;
        var request = new UpdateEmployeeRequest("Jane", "Smith", 60000m, new DateOnly(1985, 5, 15));
        var updatedEmployee = new GetEmployeeDto(1, "Jane", "Smith", 60000m, new DateOnly(1985, 5, 15),
            new List<Api.Dtos.Dependent.GetDependentDto>());

        _mockEmployeeService.Setup(x => x.UpdateAsync(employeeId, "Jane", "Smith", 60000m, new DateOnly(1985, 5, 15)))
            .ReturnsAsync(updatedEmployee);

        // Act
        var result = await CallUpdateEmployee(employeeId, request);

        // Assert
        Assert.IsType<Ok<ApiResponse<GetEmployeeDto>>>(result);
        var okResult = (Ok<ApiResponse<GetEmployeeDto>>)result;
        Assert.True(okResult.Value?.Success);
        Assert.Equal(updatedEmployee, okResult.Value?.Data);
        Assert.Contains("Employee updated successfully", okResult.Value?.Message);
    }

    [Fact]
    public async Task UpdateEmployee_NonExistentEmployee_ReturnsNotFound()
    {
        // Arrange
        const int employeeId = 999;
        var request = new UpdateEmployeeRequest("Jane", "Smith", 60000m, new DateOnly(1985, 5, 15));

        _mockEmployeeService.Setup(x => x.UpdateAsync(employeeId, "Jane", "Smith", 60000m, new DateOnly(1985, 5, 15)))
            .ReturnsAsync((GetEmployeeDto?)null);

        // Act
        var result = await CallUpdateEmployee(employeeId, request);

        // Assert
        Assert.IsType<NotFound<ApiResponse<GetEmployeeDto>>>(result);
        var notFoundResult = (NotFound<ApiResponse<GetEmployeeDto>>)result;
        Assert.False(notFoundResult.Value?.Success);
        Assert.Contains($"Employee with id {employeeId} not found", notFoundResult.Value?.Message);
    }

    [Fact]
    public async Task UpdateEmployee_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        const int employeeId = 1;
        var request = new UpdateEmployeeRequest("", "Smith", 60000m, new DateOnly(1985, 5, 15));

        _mockEmployeeService.Setup(x => x.UpdateAsync(employeeId, "", "Smith", 60000m, new DateOnly(1985, 5, 15)))
            .ThrowsAsync(new ArgumentException("FirstName is required"));

        // Act
        var result = await CallUpdateEmployee(employeeId, request);

        // Assert
        Assert.IsType<BadRequest<ApiResponse<GetEmployeeDto>>>(result);
        var badRequestResult = (BadRequest<ApiResponse<GetEmployeeDto>>)result;
        Assert.False(badRequestResult.Value?.Success);
        Assert.Contains("FirstName is required", badRequestResult.Value?.Message);
    }

    #endregion

    #region DeleteEmployee Tests

    [Fact]
    public async Task DeleteEmployee_ExistingEmployee_ReturnsOk()
    {
        // Arrange
        const int employeeId = 1;

        _mockEmployeeService.Setup(x => x.DeleteAsync(employeeId))
            .ReturnsAsync(true);

        // Act
        var result = await CallDeleteEmployee(employeeId);

        // Assert
        Assert.IsType<Ok<ApiResponse<object>>>(result);
        var okResult = (Ok<ApiResponse<object>>)result;
        Assert.True(okResult.Value?.Success);
        Assert.Contains("Employee deleted successfully", okResult.Value?.Message);
    }

    [Fact]
    public async Task DeleteEmployee_NonExistentEmployee_ReturnsNotFound()
    {
        // Arrange
        const int employeeId = 999;

        _mockEmployeeService.Setup(x => x.DeleteAsync(employeeId))
            .ReturnsAsync(false);

        // Act
        var result = await CallDeleteEmployee(employeeId);

        // Assert
        Assert.IsType<NotFound<ApiResponse<object>>>(result);
        var notFoundResult = (NotFound<ApiResponse<object>>)result;
        Assert.False(notFoundResult.Value?.Success);
        Assert.Contains($"Employee with id {employeeId} not found", notFoundResult.Value?.Message);
    }

    [Fact]
    public async Task DeleteEmployee_ServiceThrowsException_ReturnsProblem()
    {
        // Arrange
        const int employeeId = 1;

        _mockEmployeeService.Setup(x => x.DeleteAsync(employeeId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await CallDeleteEmployee(employeeId);

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
        var problemResult = (ProblemHttpResult)result;
        Assert.Equal(500, problemResult.StatusCode);
    }

    #endregion

    #region Helper Methods - Use reflection to call private endpoint methods

    private async Task<IResult> CallGetEmployeeById(int id)
    {
        var method = typeof(EmployeeEndpointsV1).GetMethod("GetEmployeeById",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var task = (Task<IResult>)method!.Invoke(null, new object[] { id, _mockEmployeeService.Object })!;
        return await task;
    }

    private async Task<IResult> CallGetAllEmployees()
    {
        var method = typeof(EmployeeEndpointsV1).GetMethod("GetAllEmployees",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var task = (Task<IResult>)method!.Invoke(null, new object?[] { _mockEmployeeService.Object, 1, 10, null, null })!;
        return await task;
    }

    private async Task<IResult> CallGetEmployeePaycheck(int id)
    {
        var method = typeof(EmployeeEndpointsV1).GetMethod("GetEmployeePaycheck",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var task = (Task<IResult>)method!.Invoke(null, new object[] { id, _mockEmployeeService.Object, _mockFeatureManager.Object })!;
        return await task;
    }

    private async Task<IResult> CallCreateEmployee(CreateEmployeeRequest request)
    {
        var method = typeof(EmployeeEndpointsV1).GetMethod("CreateEmployee",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var task = (Task<IResult>)method!.Invoke(null, new object[] { request, _mockEmployeeService.Object })!;
        return await task;
    }

    private async Task<IResult> CallUpdateEmployee(int id, UpdateEmployeeRequest request)
    {
        var method = typeof(EmployeeEndpointsV1).GetMethod("UpdateEmployee",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var task = (Task<IResult>)method!.Invoke(null, new object[] { id, request, _mockEmployeeService.Object })!;
        return await task;
    }

    private async Task<IResult> CallDeleteEmployee(int id)
    {
        var method = typeof(EmployeeEndpointsV1).GetMethod("DeleteEmployee",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var task = (Task<IResult>)method!.Invoke(null, new object[] { id, _mockEmployeeService.Object })!;
        return await task;
    }

    #endregion
}
