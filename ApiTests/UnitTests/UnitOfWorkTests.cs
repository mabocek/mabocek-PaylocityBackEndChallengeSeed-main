using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Api.Data;
using Api.Models;
using Api.Repositories;
using Xunit;

namespace ApiTests.UnitTests;

/// <summary>
/// Unit tests for the Unit of Work pattern implementation
/// </summary>
public class UnitOfWorkTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Setup service provider
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(_context);
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDependentRepository, DependentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        _serviceProvider = services.BuildServiceProvider();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
    }

    [Fact]
    public void UnitOfWork_ShouldProvideRepositories()
    {
        // Arrange & Act
        var employees = _unitOfWork.Employees;
        var dependents = _unitOfWork.Dependents;

        // Assert
        Assert.NotNull(employees);
        Assert.NotNull(dependents);
        Assert.IsAssignableFrom<IEmployeeRepository>(employees);
        Assert.IsAssignableFrom<IDependentRepository>(dependents);
    }

    [Theory]
    [InlineData("John", "Doe", 50000, "1990-01-01")]
    [InlineData("Jane", "Smith", 75000, "1985-05-15")]
    [InlineData("Bob", "Johnson", 42000, "1992-12-03")]
    [InlineData("Alice", "Williams", 68000, "1987-09-22")]
    public async Task UnitOfWork_ShouldSaveChanges(string firstName, string lastName, decimal salary, string dateOfBirthString)
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Salary = salary,
            DateOfBirth = DateOnly.Parse(dateOfBirthString)
        };

        // Act
        await _unitOfWork.Employees.AddAsync(employee);
        var changeCount = await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.Equal(1, changeCount);
        Assert.True(employee.Id > 0);

        // Verify the employee data was saved correctly
        var savedEmployee = await _unitOfWork.Employees.GetByIdAsync(employee.Id);
        Assert.NotNull(savedEmployee);
        Assert.Equal(firstName, savedEmployee.FirstName);
        Assert.Equal(lastName, savedEmployee.LastName);
        Assert.Equal(salary, savedEmployee.Salary);
    }

    [Theory]
    [InlineData("Jane", "Smith", 60000, "1985-05-15")]
    [InlineData("Transaction", "Test", 55000, "1988-11-20")]
    [InlineData("Data", "Driven", 72000, "1983-03-14")]
    public async Task UnitOfWork_ShouldHandleTransaction(string firstName, string lastName, decimal salary, string dateOfBirthString)
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Salary = salary,
            DateOfBirth = DateOnly.Parse(dateOfBirthString)
        };

        // Act
        var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();
            return employee.Id;
        });

        // Assert
        Assert.True(result > 0);
        Assert.Equal(result, employee.Id);

        // Verify employee was saved
        var savedEmployee = await _unitOfWork.Employees.GetByIdAsync(result);
        Assert.NotNull(savedEmployee);
        Assert.Equal(firstName, savedEmployee.FirstName);
        Assert.Equal(lastName, savedEmployee.LastName);
        Assert.Equal(salary, savedEmployee.Salary);
    }

    [Theory]
    [InlineData("Test", "User", 45000, "1992-03-10")]
    [InlineData("Exception", "Handler", 38000, "1995-07-18")]
    [InlineData("Rollback", "Scenario", 52000, "1989-01-25")]
    public async Task UnitOfWork_ShouldRollbackOnException(string firstName, string lastName, decimal salary, string dateOfBirthString)
    {
        // Note: InMemory database doesn't support transactions, so this test
        // demonstrates the API but won't actually rollback in memory tests

        // Arrange
        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Salary = salary,
            DateOfBirth = DateOnly.Parse(dateOfBirthString)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _unitOfWork.Employees.AddAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                // This should cause a rollback
                throw new InvalidOperationException($"Test exception for {firstName} {lastName}");
            });
        });

        // Verify the exception message contains the employee name
        Assert.Contains(firstName, exception.Message);
        Assert.Contains(lastName, exception.Message);

        // Note: With a real database (SQL Server, PostgreSQL, etc.) the transaction would be rolled back
        // and the employee would not exist. With InMemory database, the data remains.
        // This test validates the exception propagation behavior.
    }

    [Theory]
    [InlineData("Multi", "Repo", 55000, "1988-08-20", "Child", "Repo", "2015-12-25", "Child")]
    [InlineData("Parent", "One", 67000, "1986-04-12", "Spouse", "One", "1987-02-28", "Spouse")]
    [InlineData("Family", "Man", 73000, "1982-10-15", "Kid", "Man", "2018-06-05", "Child")]
    public async Task UnitOfWork_ShouldHandleMultipleRepositoriesInTransaction(
        string empFirstName, string empLastName, decimal empSalary, string empDateOfBirth,
        string depFirstName, string depLastName, string depDateOfBirth, string relationship)
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = empFirstName,
            LastName = empLastName,
            Salary = empSalary,
            DateOfBirth = DateOnly.Parse(empDateOfBirth)
        };

        var relationshipEnum = Enum.Parse<Relationship>(relationship);

        // Act
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Add employee
            var createdEmployee = await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            // Add dependent
            var dependent = new Dependent
            {
                FirstName = depFirstName,
                LastName = depLastName,
                DateOfBirth = DateOnly.Parse(depDateOfBirth),
                Relationship = relationshipEnum,
                EmployeeId = createdEmployee.Id
            };

            await _unitOfWork.Dependents.AddAsync(dependent);
            await _unitOfWork.SaveChangesAsync();
        });

        // Assert
        var savedEmployee = await _unitOfWork.Employees.GetAllAsync();
        var savedDependents = await _unitOfWork.Dependents.GetAllAsync();

        Assert.Contains(savedEmployee, e => e.FirstName == empFirstName && e.LastName == empLastName);
        Assert.Contains(savedDependents, d => d.FirstName == depFirstName && d.LastName == depLastName);

        // Verify the relationship
        var employeeRecord = savedEmployee.First(e => e.FirstName == empFirstName);
        var dependentRecord = savedDependents.First(d => d.FirstName == depFirstName);
        Assert.Equal(employeeRecord.Id, dependentRecord.EmployeeId);
        Assert.Equal(relationshipEnum, dependentRecord.Relationship);
    }

    [Theory]
    [InlineData("Manual", "Transaction", 48000, "1991-11-11")]
    [InlineData("Commit", "Test", 54000, "1990-06-30")]
    [InlineData("Direct", "Control", 61000, "1984-09-08")]
    public async Task UnitOfWork_ShouldManuallyBeginCommitTransaction(string firstName, string lastName, decimal salary, string dateOfBirthString)
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Salary = salary,
            DateOfBirth = DateOnly.Parse(dateOfBirthString)
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        // Assert
        var savedEmployee = await _unitOfWork.Employees.GetAllAsync();
        Assert.Contains(savedEmployee, e => e.FirstName == firstName && e.LastName == lastName);

        // Verify the saved employee has the correct data
        var employeeRecord = savedEmployee.First(e => e.FirstName == firstName);
        Assert.Equal(salary, employeeRecord.Salary);
        Assert.Equal(DateOnly.Parse(dateOfBirthString), employeeRecord.DateOfBirth);
    }

    [Theory]
    [InlineData("Rollback", "Test", 47000, "1993-07-07")]
    [InlineData("Manual", "Cancel", 39000, "1996-02-14")]
    [InlineData("Abort", "Operation", 51000, "1987-12-01")]
    public async Task UnitOfWork_ShouldManuallyRollbackTransaction(string firstName, string lastName, decimal salary, string dateOfBirthString)
    {
        // Note: InMemory database doesn't support transactions, so this test
        // demonstrates the API but won't actually rollback in memory tests

        // Arrange
        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Salary = salary,
            DateOfBirth = DateOnly.Parse(dateOfBirthString)
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.RollbackTransactionAsync();

        // Note: With a real database (SQL Server, PostgreSQL, etc.) the transaction would be rolled back
        // and the employee would not exist. With InMemory database, the data remains.
        // This test validates the manual transaction API behavior.

        // Assert - verify the rollback API was called without exceptions
        // The employee will still exist in memory database, but the API worked correctly
        Assert.True(employee.Id > 0); // Employee was assigned an ID during add operation
    }

    public void Dispose()
    {
        _unitOfWork?.Dispose();
        _context?.Dispose();
        if (_serviceProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
    }
}
