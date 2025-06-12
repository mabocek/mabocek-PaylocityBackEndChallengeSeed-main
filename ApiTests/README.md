# Integration Tests Documentation

This document describes the organization and decoration of integration tests in the PaylocityBenefitsCalculator project.

## Test Organization

### Test Categories

All integration tests are decorated with the following traits:

#### Primary Category
- `[Trait("Category", "Integration")]` - Identifies all integration tests

#### Feature Areas
- `[Trait("Feature", "Employees")]` - Tests related to employee operations
- `[Trait("Feature", "Dependents")]` - Tests related to dependent operations

#### Test Types
- `[Trait("TestType", "GetAll")]` - Tests that retrieve all records
- `[Trait("TestType", "GetById")]` - Tests that retrieve a specific record by ID
- `[Trait("TestType", "ErrorHandling")]` - Tests that verify error scenarios

#### Priority Levels
- `[Trait("Priority", "High")]` - Critical functionality tests (CRUD operations)
- `[Trait("Priority", "Medium")]` - Important but non-critical tests (error handling)
- `[Trait("Priority", "Low")]` - Nice-to-have tests

#### Additional Attributes
- `[Trait("ExpectedResult", "NotFound")]` - Tests expecting 404 responses
- `[Trait("DataComplexity", "Complex")]` - Tests involving complex data structures

## Running Tests by Category

### Run all integration tests
```bash
dotnet test --filter "Category=Integration"
```

### Run tests by feature
```bash
# Employee tests only
dotnet test --filter "Feature=Employees"

# Dependent tests only
dotnet test --filter "Feature=Dependents"
```

### Run tests by priority
```bash
# High priority tests only
dotnet test --filter "Priority=High"

# Medium priority tests only
dotnet test --filter "Priority=Medium"
```

### Run tests by type
```bash
# All GetAll operations
dotnet test --filter "TestType=GetAll"

# All error handling tests
dotnet test --filter "TestType=ErrorHandling"
```

### Combine filters
```bash
# High priority employee tests
dotnet test --filter "Feature=Employees&Priority=High"

# Error handling tests for all features
dotnet test --filter "TestType=ErrorHandling"
```

## Test Structure

### Base Class
- `IntegrationTest` - Abstract base class providing HTTP client configuration
- Configured to connect to `https://localhost:7124`
- Implements `IDisposable` for proper cleanup

### Test Classes
- `EmployeeIntegrationTests` - Tests for employee-related endpoints
- `DependentIntegrationTests` - Tests for dependent-related endpoints

## Prerequisites

Before running integration tests:

1. **Start the API server:**
   ```bash
   dotnet run --project Api
   ```

2. **Verify the server is running on the correct port (7124)**

3. **Run the integration tests:**
   ```bash
   dotnet test
   ```

## Notes

- Integration tests require a running API server
- Tests use real HTTP requests to verify API behavior
- All tests are currently marked with "//task: make test pass" comments indicating they need implementation
- The tests verify both successful operations and error scenarios (404 responses)
