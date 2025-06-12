using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiTests.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Feature", "Employees")]
public class EmployeeV1IntegrationTests : IntegrationTestBase
{
    public EmployeeV1IntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }
    [Fact]
    [Trait("TestType", "GetAll")]
    [Trait("Priority", "High")]
    [Trait("DataComplexity", "Complex")]
    public async Task WhenAskedForAllEmployees_ShouldReturnAllEmployees()
    {
        var response = await HttpClient.GetAsync("/api/v1/employees");
        var employees = new List<GetEmployeeDto>
        {
            new(1, "LeBron", "James", 75420.99m, new DateOnly(1984, 12, 30), new List<GetDependentDto>()),
            new(2, "Ja", "Morant", 92365.22m, new DateOnly(1999, 8, 10), new List<GetDependentDto>
            {
                new(1, "Spouse", "Morant", new DateOnly(1998, 3, 3), Relationship.Spouse, 2),
                new(2, "Child1", "Morant", new DateOnly(2020, 6, 23), Relationship.Child, 2),
                new(3, "Child2", "Morant", new DateOnly(2021, 5, 18), Relationship.Child, 2)
            }),
            new(3, "Michael", "Jordan", 143211.12m, new DateOnly(1963, 2, 17), new List<GetDependentDto>
            {
                new(4, "DP", "Jordan", new DateOnly(1974, 1, 2), Relationship.DomesticPartner, 3)
            })
        };

        // Construct expected paginated result
        var expectedPagedResult = new PagedResult<GetEmployeeDto>(employees, 3, 1, 10);

        await response.ShouldReturn(HttpStatusCode.OK, expectedPagedResult);
    }

    [Fact]
    [Trait("TestType", "GetById")]
    [Trait("Priority", "High")]
    //task: make test pass
    public async Task WhenAskedForAnEmployee_ShouldReturnCorrectEmployee()
    {
        var response = await HttpClient.GetAsync("/api/v1/employees/1");
        var employee = new GetEmployeeDto(1, "LeBron", "James", 75420.99m, new DateOnly(1984, 12, 30), new List<GetDependentDto>());
        await response.ShouldReturn(HttpStatusCode.OK, employee);
    }

    /// <summary>
    /// Tests error handling for various non-existent employee scenarios
    /// </summary>
    [Theory]
    [Trait("TestType", "ErrorHandling")]
    [Trait("Priority", "Medium")]
    [Trait("ExpectedResult", "NotFound")]
    [InlineData(int.MinValue)] // Minimum integer value
    [InlineData(-1)] // Negative ID
    [InlineData(0)] // Zero ID
    [InlineData(999999)] // Large non-existent ID
    [InlineData(100)] // Reasonable but non-existent ID
    public async Task WhenAskedForANonexistentEmployee_ShouldReturn404(int nonExistentEmployeeId)
    {
        var response = await HttpClient.GetAsync($"/api/v1/employees/{nonExistentEmployeeId}");
        await response.ShouldReturn(HttpStatusCode.NotFound);
    }
}

