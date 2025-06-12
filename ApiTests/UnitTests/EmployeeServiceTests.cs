using Api.Services;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Features.Employees.Queries;
using Api.Features.Employees.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApiTests.UnitTests;

/// <summary>
/// Tests for EmployeeService - key integration point for employee operations
/// Covers service layer logic and integration with CQRS and PaycheckCalculationService
/// </summary>
public class EmployeeServiceTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<EmployeeService>> _mockLogger;
    private readonly Mock<IPaycheckCalculationService> _mockPaycheckService;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<EmployeeService>>();
        _mockPaycheckService = new Mock<IPaycheckCalculationService>();

        _service = new EmployeeService(_mockMediator.Object, _mockLogger.Object, _mockPaycheckService.Object);
    }

    #region Constructor Tests

    /// <summary>
    /// Tests constructor validation for null dependencies
    /// </summary>
    [Theory]
    [InlineData("logger")] // Logger is null
    [InlineData("paycheckService")] // PaycheckService is null
    public void Constructor_ThrowsArgumentNullException_WhenDependencyIsNull(string nullDependency)
    {
        // Act & Assert
        switch (nullDependency)
        {
            case "logger":
                Assert.Throws<ArgumentNullException>(() =>
                    new EmployeeService(_mockMediator.Object, null!, _mockPaycheckService.Object));
                break;
            case "paycheckService":
                Assert.Throws<ArgumentNullException>(() =>
                    new EmployeeService(_mockMediator.Object, _mockLogger.Object, null!));
                break;
        }
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldCallMediator_WithGetAllEmployeesQuery()
    {
        // Arrange
        var expectedEmployees = new List<GetEmployeeDto>
        {
            new(1, "John", "Doe", 50000m, new DateOnly(1990, 1, 1), new List<Api.Dtos.Dependent.GetDependentDto>())
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetAllEmployeesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployees);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(expectedEmployees, result);
        _mockMediator.Verify(x => x.Send(It.IsAny<GetAllEmployeesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldCallMediator_WithGetEmployeeByIdQuery()
    {
        // Arrange
        const int employeeId = 1;
        var expectedEmployee = new GetEmployeeDto(1, "John", "Doe", 50000m, new DateOnly(1990, 1, 1),
            new List<Api.Dtos.Dependent.GetDependentDto>());

        _mockMediator.Setup(x => x.Send(It.Is<GetEmployeeByIdQuery>(q => q.Id == employeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployee);

        // Act
        var result = await _service.GetByIdAsync(employeeId);

        // Assert
        Assert.Equal(expectedEmployee, result);
        _mockMediator.Verify(x => x.Send(It.Is<GetEmployeeByIdQuery>(q => q.Id == employeeId), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetPaycheckAsync Tests

    [Fact]
    public async Task GetPaycheckAsync_EmployeeExists_ShouldReturnCalculatedPaycheck()
    {
        // Arrange
        const int employeeId = 1;
        var employee = new GetEmployeeDto(1, "John", "Doe", 50000m, new DateOnly(1990, 1, 1),
            new List<Api.Dtos.Dependent.GetDependentDto>());
        var expectedPaycheck = new GetPaycheckDto(1, "John Doe", 1923.08m, 461.54m, 1461.54m,
            new Api.Dtos.Paycheck.PaycheckDetailsDto(1000m, 0m, 0m, 0m, 1000m, 461.54m));

        _mockMediator.Setup(x => x.Send(It.Is<GetEmployeePaycheckQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockPaycheckService.Setup(x => x.CalculatePaycheckAsync(employee))
            .ReturnsAsync(expectedPaycheck);

        // Act
        var result = await _service.GetPaycheckAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPaycheck, result);
        _mockMediator.Verify(x => x.Send(It.Is<GetEmployeePaycheckQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()), Times.Once);
        _mockPaycheckService.Verify(x => x.CalculatePaycheckAsync(employee), Times.Once);
    }

    [Fact]
    public async Task GetPaycheckAsync_EmployeeNotFound_ShouldReturnNull()
    {
        // Arrange
        const int employeeId = 999;

        _mockMediator.Setup(x => x.Send(It.Is<GetEmployeePaycheckQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetEmployeeDto?)null);

        // Act
        var result = await _service.GetPaycheckAsync(employeeId);

        // Assert
        Assert.Null(result);
        _mockMediator.Verify(x => x.Send(It.Is<GetEmployeePaycheckQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()), Times.Once);
        _mockPaycheckService.Verify(x => x.CalculatePaycheckAsync(It.IsAny<GetEmployeeDto>()), Times.Never);
    }

    [Fact]
    public async Task GetPaycheckAsync_PaycheckCalculationThrows_ShouldPropagateException()
    {
        // Arrange
        const int employeeId = 1;
        var employee = new GetEmployeeDto(1, "John", "Doe", 50000m, new DateOnly(1990, 1, 1),
            new List<Api.Dtos.Dependent.GetDependentDto>());

        _mockMediator.Setup(x => x.Send(It.Is<GetEmployeePaycheckQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockPaycheckService.Setup(x => x.CalculatePaycheckAsync(employee))
            .ThrowsAsync(new InvalidOperationException("Calculation error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetPaycheckAsync(employeeId));
        Assert.Equal("Calculation error", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCallMediator_WithCreateEmployeeCommand()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "Doe";
        const decimal salary = 50000m;
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var expectedEmployee = new GetEmployeeDto(1, firstName, lastName, salary, dateOfBirth,
            new List<Api.Dtos.Dependent.GetDependentDto>());

        _mockMediator.Setup(x => x.Send(It.Is<CreateEmployeeCommand>(c =>
            c.FirstName == firstName && c.LastName == lastName && c.Salary == salary && c.DateOfBirth == dateOfBirth),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployee);

        // Act
        var result = await _service.CreateAsync(firstName, lastName, salary, dateOfBirth);

        // Assert
        Assert.Equal(expectedEmployee, result);
        _mockMediator.Verify(x => x.Send(It.Is<CreateEmployeeCommand>(c =>
            c.FirstName == firstName && c.LastName == lastName && c.Salary == salary && c.DateOfBirth == dateOfBirth),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldCallMediator_WithUpdateEmployeeCommand()
    {
        // Arrange
        const int id = 1;
        const string firstName = "Jane";
        const string lastName = "Smith";
        const decimal salary = 60000m;
        var dateOfBirth = new DateOnly(1985, 5, 15);
        var expectedEmployee = new GetEmployeeDto(id, firstName, lastName, salary, dateOfBirth,
            new List<Api.Dtos.Dependent.GetDependentDto>());

        _mockMediator.Setup(x => x.Send(It.Is<UpdateEmployeeCommand>(c =>
            c.Id == id && c.FirstName == firstName && c.LastName == lastName && c.Salary == salary && c.DateOfBirth == dateOfBirth),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployee);

        // Act
        var result = await _service.UpdateAsync(id, firstName, lastName, salary, dateOfBirth);

        // Assert
        Assert.Equal(expectedEmployee, result);
        _mockMediator.Verify(x => x.Send(It.Is<UpdateEmployeeCommand>(c =>
            c.Id == id && c.FirstName == firstName && c.LastName == lastName && c.Salary == salary && c.DateOfBirth == dateOfBirth),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldCallMediator_WithDeleteEmployeeCommand()
    {
        // Arrange
        const int employeeId = 1;
        const bool expectedResult = true;

        _mockMediator.Setup(x => x.Send(It.Is<DeleteEmployeeCommand>(c => c.Id == employeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.DeleteAsync(employeeId);

        // Assert
        Assert.Equal(expectedResult, result);
        _mockMediator.Verify(x => x.Send(It.Is<DeleteEmployeeCommand>(c => c.Id == employeeId), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
