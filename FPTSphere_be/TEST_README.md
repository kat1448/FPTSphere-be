# FPTSphere Backend - Unit Tests

## Overview
This test suite provides comprehensive unit test coverage for the User CRUD functionality implemented in the FPTSphere backend application.

## Test Projects

### 1. BusinessLayer.Tests
Tests for the business logic layer including:
- **UserServiceTests**: Comprehensive tests for `UserService` covering all CRUD operations
  - CreateAsync: 5 test cases including edge cases and validation
  - GetAllAsync: 4 test cases covering empty lists, multiple users, and large datasets
  - GetByIdAsync: 5 test cases covering valid/invalid IDs
  - UpdateAsync: 6 test cases covering updates and edge cases
  - Integration tests for end-to-end workflows
  
- **MappingProfileTests**: Tests for AutoMapper configuration
  - Configuration validation
  - User to UserDto mapping (3 test cases)
  - UserDto to User reverse mapping (2 test cases)
  - CreateUserDto to User mapping (3 test cases)
  - UpdateUserDto to User mapping (3 test cases)
  - Collection mapping tests
  - Mapping to existing instance tests

### 2. DataLayer.Tests
Tests for the data access layer:
- **UserRepositoryTests**: Tests for `UserRepository` CRUD operations
  - GetAllAsync: 3 test cases
  - GetByIdAsync: 5 test cases
  - CreateAsync: 4 test cases
  - UpdateAsync: 4 test cases
  - DeleteAsync: 5 test cases
  - Integration tests for complete workflows

### 3. WebApi.Tests
Tests for the API controllers:
- **UsersControllerTests**: Tests for `UsersController` endpoints
  - GetAll: 4 test cases covering success and edge cases
  - GetById: 5 test cases covering valid/invalid IDs
  - Create: 5 test cases covering validation and creation
  - Update: 7 test cases covering updates, validation, and error cases
  - Integration and error handling tests: 6 additional test cases

## Test Statistics
- **Total Test Cases**: 80+
- **Test Framework**: xUnit
- **Mocking Framework**: Moq
- **Assertion Library**: FluentAssertions
- **In-Memory Database**: Entity Framework Core InMemory

## Running the Tests

### Run all tests
```bash
dotnet test
```

### Run specific test project
```bash
dotnet test BusinessLayer.Tests/BusinessLayer.Tests.csproj
dotnet test DataLayer.Tests/DataLayer.Tests.csproj
dotnet test WebApi.Tests/WebApi.Tests.csproj
```

### Run with coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run specific test
```bash
dotnet test --filter "FullyQualifiedName~UserServiceTests.CreateAsync_WithValidDto_ShouldCreateUserAndReturnUserDto"
```

## Test Coverage Areas

### Happy Path Scenarios
- Creating users with valid data
- Retrieving all users
- Retrieving users by ID
- Updating existing users
- Deleting users

### Edge Cases
- Empty/null inputs
- Non-existent IDs
- Negative IDs
- Zero IDs
- Empty strings
- Large datasets (100+ records)
- Multiple sequential operations

### Error Handling
- Invalid model state
- ID mismatches
- Service exceptions propagation
- Non-existent resource handling

### Integration Scenarios
- Create and retrieve workflows
- Create, update, and delete workflows
- Multiple operations in sequence

## Best Practices Followed
1. **Arrange-Act-Assert (AAA)** pattern in all tests
2. **Descriptive test names** that clearly communicate intent
3. **Single responsibility** - each test validates one specific behavior
4. **Test isolation** - using in-memory databases with unique instances
5. **Comprehensive coverage** - happy paths, edge cases, and error scenarios
6. **Mocking external dependencies** - using Moq for service layer
7. **Fluent assertions** - readable and expressive test assertions

## Dependencies
- xUnit 2.6.2
- Moq 4.20.70
- FluentAssertions 6.12.0
- Microsoft.EntityFrameworkCore.InMemory 9.0.0
- Microsoft.AspNetCore.Mvc.Testing 8.0.0
- AutoMapper 13.0.1

## Maintenance Notes
- Tests use in-memory databases to avoid external dependencies
- Each test method creates its own database context for isolation
- Mock setup is done in test constructors where appropriate
- Tests are organized by functionality using regions for better readability