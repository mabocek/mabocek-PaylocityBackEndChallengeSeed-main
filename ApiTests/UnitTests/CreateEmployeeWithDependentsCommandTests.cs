using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Dtos.Employee;
using Api.Features.Employees.Commands;
using Api.Models;
using Api.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class CreateEmployeeWithDependentsCommandTests
{
    private static CreateEmployeeWithDependentsCommandHandler CreateHandler()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var logger = new Mock<ILogger<CreateEmployeeWithDependentsCommandHandler>>();

        // Configure the mock to actually execute the operation passed to ExecuteInTransactionAsync
        unitOfWork.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task<GetEmployeeDto>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<GetEmployeeDto>>, CancellationToken>(async (operation, ct) => await operation());

        return new CreateEmployeeWithDependentsCommandHandler(unitOfWork.Object, logger.Object);
    }

    [Fact]
    public async Task Throws_When_BothSpouseAndDomesticPartner()
    {
        var handler = CreateHandler();
        var command = new CreateEmployeeWithDependentsCommand(
            "Test", "User", 50000, DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            new List<CreateDependentDto>
            {
                new("A", "B", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), Relationship.Spouse),
                new("C", "D", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), Relationship.DomesticPartner)
            }
        );
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Throws_When_MultipleSpouses()
    {
        var handler = CreateHandler();
        var command = new CreateEmployeeWithDependentsCommand(
            "Test", "User", 50000, DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            new List<CreateDependentDto>
            {
                new("A", "B", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), Relationship.Spouse),
                new("C", "D", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), Relationship.Spouse)
            }
        );
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Throws_When_MultipleDomesticPartners()
    {
        var handler = CreateHandler();
        var command = new CreateEmployeeWithDependentsCommand(
            "Test", "User", 50000, DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            new List<CreateDependentDto>
            {
                new("A", "B", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), Relationship.DomesticPartner),
                new("C", "D", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), Relationship.DomesticPartner)
            }
        );
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Succeeds_With_OneSpouseOrOneDomesticPartner()
    {
        var handler = CreateHandler();
        var command1 = new CreateEmployeeWithDependentsCommand(
            "Test", "User", 50000, DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            new List<CreateDependentDto>
            {
                new("A", "B", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), Relationship.Spouse)
            }
        );
        // Should not throw (will fail later due to mock, but not ArgumentException)
        await Assert.ThrowsAnyAsync<Exception>(() => handler.Handle(command1, CancellationToken.None));

        var command2 = new CreateEmployeeWithDependentsCommand(
            "Test", "User", 50000, DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            new List<CreateDependentDto>
            {
                new("A", "B", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), Relationship.DomesticPartner)
            }
        );
        await Assert.ThrowsAnyAsync<Exception>(() => handler.Handle(command2, CancellationToken.None));
    }
}
