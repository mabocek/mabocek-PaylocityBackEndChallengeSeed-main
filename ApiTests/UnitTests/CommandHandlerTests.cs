using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Features.Employees.Commands;
using Api.Models;
using Api.Repositories;
using Api.Dtos.Employee;
using Api.Dtos.Dependent;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiTests.UnitTests;

/// <summary>
/// Unit tests for command handlers to improve code coverage
/// Tests the CQRS command handling for employee operations
/// </summary>
public class CommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<ILogger<CreateEmployeeCommandHandler>> _mockCreateLogger;
    private readonly Mock<ILogger<UpdateEmployeeCommandHandler>> _mockUpdateLogger;
    private readonly Mock<ILogger<DeleteEmployeeCommandHandler>> _mockDeleteLogger;

    public CommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockCreateLogger = new Mock<ILogger<CreateEmployeeCommandHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateEmployeeCommandHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeleteEmployeeCommandHandler>>();

        _mockUnitOfWork.Setup(x => x.Employees).Returns(_mockEmployeeRepository.Object);
    }

    [Fact]
    public async Task CreateEmployeeCommandHandler_ValidRequest_ReturnsEmployee()
    {
        // Arrange
        var command = new CreateEmployeeCommand("John", "Doe", 50000m, new DateOnly(1990, 1, 1));
        var createdEmployee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Salary = 50000m,
            DateOfBirth = new DateOnly(1990, 1, 1),
            Dependents = new List<Dependent>()
        };

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<Api.Dtos.Employee.GetEmployeeDto>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Api.Dtos.Employee.GetEmployeeDto>> operation, CancellationToken ct) =>
            {
                _mockEmployeeRepository.Setup(x => x.AddAsync(It.IsAny<Employee>(), ct))
                    .ReturnsAsync(createdEmployee);

                return await operation();
            });

        var handler = new CreateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockCreateLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal(50000m, result.Salary);
        Assert.Equal(new DateOnly(1990, 1, 1), result.DateOfBirth);
    }

    [Fact]
    public async Task CreateEmployeeCommandHandler_EmptyFirstName_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreateEmployeeCommand("", "Doe", 50000m, new DateOnly(1990, 1, 1));

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<Api.Dtos.Employee.GetEmployeeDto>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Api.Dtos.Employee.GetEmployeeDto>> operation, CancellationToken ct) =>
            {
                return await operation();
            });

        var handler = new CreateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockCreateLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("FirstName is required", exception.Message);
    }

    [Fact]
    public async Task CreateEmployeeCommandHandler_EmptyLastName_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreateEmployeeCommand("John", "", 50000m, new DateOnly(1990, 1, 1));

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<Api.Dtos.Employee.GetEmployeeDto>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Api.Dtos.Employee.GetEmployeeDto>> operation, CancellationToken ct) =>
            {
                return await operation();
            });

        var handler = new CreateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockCreateLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("LastName is required", exception.Message);
    }

    [Fact]
    public async Task CreateEmployeeCommandHandler_NegativeSalary_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreateEmployeeCommand("John", "Doe", -1000m, new DateOnly(1990, 1, 1));

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<Api.Dtos.Employee.GetEmployeeDto>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Api.Dtos.Employee.GetEmployeeDto>> operation, CancellationToken ct) =>
            {
                return await operation();
            });

        var handler = new CreateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockCreateLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("Salary must be positive", exception.Message);
    }

    [Fact]
    public async Task CreateEmployeeCommandHandler_ZeroSalary_ThrowsArgumentException()
    {
        // Arrange
        var command = new CreateEmployeeCommand("John", "Doe", 0m, new DateOnly(1990, 1, 1));

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<Api.Dtos.Employee.GetEmployeeDto>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Api.Dtos.Employee.GetEmployeeDto>> operation, CancellationToken ct) =>
            {
                return await operation();
            });

        var handler = new CreateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockCreateLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("Salary must be positive", exception.Message);
    }

    [Fact]
    public async Task CreateEmployeeCommandHandler_TrimsWhitespace_Success()
    {
        // Arrange
        var command = new CreateEmployeeCommand("  John  ", "  Doe  ", 50000m, new DateOnly(1990, 1, 1));
        var createdEmployee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Salary = 50000m,
            DateOfBirth = new DateOnly(1990, 1, 1),
            Dependents = new List<Dependent>()
        };

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<Api.Dtos.Employee.GetEmployeeDto>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Api.Dtos.Employee.GetEmployeeDto>> operation, CancellationToken ct) =>
            {
                _mockEmployeeRepository.Setup(x => x.AddAsync(It.IsAny<Employee>(), ct))
                    .ReturnsAsync(createdEmployee);

                return await operation();
            });

        var handler = new CreateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockCreateLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public void CreateEmployeeCommandHandler_NullUnitOfWork_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CreateEmployeeCommandHandler(null!, _mockCreateLogger.Object));
    }

    [Fact]
    public void CreateEmployeeCommandHandler_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CreateEmployeeCommandHandler(_mockUnitOfWork.Object, null!));
    }

    // UpdateEmployeeCommandHandler Tests

    [Fact]
    public async Task UpdateEmployeeCommandHandler_ValidRequest_ReturnsUpdatedEmployee()
    {
        // Arrange
        var command = new UpdateEmployeeCommand(1, "Jane", "Smith", 60000m, new DateOnly(1985, 5, 15));
        var existingEmployee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Salary = 50000m,
            DateOfBirth = new DateOnly(1990, 1, 1),
            Dependents = new List<Dependent>()
        };

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<Api.Dtos.Employee.GetEmployeeDto?>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Api.Dtos.Employee.GetEmployeeDto?>> operation, CancellationToken ct) =>
            {
                _mockEmployeeRepository.Setup(x => x.GetWithDependentsAsync(1, ct))
                    .ReturnsAsync(existingEmployee);

                return await operation();
            });

        var handler = new UpdateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockUpdateLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Smith", result.LastName);
        Assert.Equal(60000m, result.Salary);
        Assert.Equal(new DateOnly(1985, 5, 15), result.DateOfBirth);
    }

    [Fact]
    public async Task UpdateEmployeeCommandHandler_EmployeeNotFound_ReturnsNull()
    {
        // Arrange
        var command = new UpdateEmployeeCommand(999, "Jane", "Smith", 60000m, new DateOnly(1985, 5, 15));

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<Api.Dtos.Employee.GetEmployeeDto?>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Api.Dtos.Employee.GetEmployeeDto?>> operation, CancellationToken ct) =>
            {
                _mockEmployeeRepository.Setup(x => x.GetWithDependentsAsync(999, ct))
                    .ReturnsAsync((Employee?)null);

                return await operation();
            });

        var handler = new UpdateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockUpdateLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateEmployeeCommandHandler_EmptyFirstName_ThrowsArgumentException()
    {
        // Arrange
        var command = new UpdateEmployeeCommand(1, "", "Smith", 60000m, new DateOnly(1985, 5, 15));
        var handler = new UpdateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockUpdateLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("FirstName is required", exception.Message);
    }

    [Fact]
    public async Task UpdateEmployeeCommandHandler_EmptyLastName_ThrowsArgumentException()
    {
        // Arrange
        var command = new UpdateEmployeeCommand(1, "Jane", "", 60000m, new DateOnly(1985, 5, 15));
        var handler = new UpdateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockUpdateLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("LastName is required", exception.Message);
    }

    [Fact]
    public async Task UpdateEmployeeCommandHandler_NegativeSalary_ThrowsArgumentException()
    {
        // Arrange
        var command = new UpdateEmployeeCommand(1, "Jane", "Smith", -1000m, new DateOnly(1985, 5, 15));
        var handler = new UpdateEmployeeCommandHandler(_mockUnitOfWork.Object, _mockUpdateLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
        Assert.Contains("Salary must be positive", exception.Message);
    }

    [Fact]
    public void UpdateEmployeeCommandHandler_NullUnitOfWork_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UpdateEmployeeCommandHandler(null!, _mockUpdateLogger.Object));
    }

    [Fact]
    public void UpdateEmployeeCommandHandler_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UpdateEmployeeCommandHandler(_mockUnitOfWork.Object, null!));
    }

    // DeleteEmployeeCommandHandler Tests

    [Fact]
    public async Task DeleteEmployeeCommandHandler_ValidEmployeeId_ReturnsTrue()
    {
        // Arrange
        var command = new DeleteEmployeeCommand(1);

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<bool>> operation, CancellationToken ct) =>
            {
                _mockEmployeeRepository.Setup(x => x.ExistsAsync(1, ct))
                    .ReturnsAsync(true);

                return await operation();
            });

        var handler = new DeleteEmployeeCommandHandler(_mockUnitOfWork.Object, _mockDeleteLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteEmployeeCommandHandler_EmployeeNotFound_ReturnsFalse()
    {
        // Arrange
        var command = new DeleteEmployeeCommand(999);

        _mockUnitOfWork.Setup(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<Task<bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<bool>> operation, CancellationToken ct) =>
            {
                _mockEmployeeRepository.Setup(x => x.ExistsAsync(999, ct))
                    .ReturnsAsync(false);

                return await operation();
            });

        var handler = new DeleteEmployeeCommandHandler(_mockUnitOfWork.Object, _mockDeleteLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DeleteEmployeeCommandHandler_NullUnitOfWork_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DeleteEmployeeCommandHandler(null!, _mockDeleteLogger.Object));
    }

    [Fact]
    public void DeleteEmployeeCommandHandler_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DeleteEmployeeCommandHandler(_mockUnitOfWork.Object, null!));
    }
}
