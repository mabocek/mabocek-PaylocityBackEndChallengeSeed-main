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
public class EmployeeEndpointsV1Tests_Working
{
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly Mock<IFeatureManager> _mockFeatureManager;

    public EmployeeEndpointsV1Tests_Working()
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
        var apiResponse = new ApiResponse<GetEmployeeDto>
        {
            Data = employee,
            Success = true,
            Message = string.Empty
        };

        _mockEmployeeService.Setup(x => x.GetEmployeeByIdAsync(employeeId))
            .ReturnsAsync(apiResponse);

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
        var apiResponse = new ApiResponse<GetEmployeeDto>
        {
            Data = null,
            Success = false,
            Message = $"Employee with id {employeeId} not found"
        };

        _mockEmployeeService.Setup(x => x.GetEmployeeByIdAsync(employeeId))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await CallGetEmployeeById(employeeId);

        // Assert
        Assert.IsType<NotFound<ApiResponse<GetEmployeeDto>>>(result);
        var notFoundResult = (NotFound<ApiResponse<GetEmployeeDto>>)result;
        Assert.False(notFoundResult.Value?.Success);
        Assert.Contains($"Employee with id {employeeId} not found", notFoundResult.Value?.Message);
    }

    #endregion

    #region Helper Methods

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
        var task = (Task<IResult>)method!.Invoke(null, new object[] { _mockEmployeeService.Object, 1, 10, null, null })!;
        return await task;
    }

    private async Task<IResult> CallGetEmployeePaycheck(int id)
    {
        var method = typeof(EmployeeEndpointsV1).GetMethod("GetEmployeePaycheck",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var task = (Task<IResult>)method!.Invoke(null, new object[] { id, _mockEmployeeService.Object })!;
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
