# FPTSphere Backend Test Suite

This test project contains comprehensive unit tests for the FPTSphere backend application, covering all layers of the architecture.

## Test Structure

### BusinessLayer Tests
- **DTOs Tests**: Validation of Data Transfer Objects (CreateUserDto, UpdateUserDto, UserDto)
- **Services Tests**: Unit tests for UserService with mocked dependencies
- **Mappings Tests**: AutoMapper profile validation and mapping tests

### DataLayer Tests
- **Repositories Tests**: Unit tests for UserRepository using in-memory database

### WebApi Tests
- **Controllers Tests**: Unit tests for UsersController with mocked services

## Technologies Used

- **xUnit**: Testing framework
- **Moq**: Mocking framework for unit tests
- **FluentAssertions**: Readable assertion library
- **EntityFrameworkCore.InMemory**: In-memory database for repository tests
- **AutoMapper**: Object mapping

## Running Tests

From the solution root directory:

```bash
dotnet test
```

To run with detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

To run tests with code coverage:

```bash
dotnet test /p:CollectCoverage=true
```

## Test Coverage

### DTO Tests (24 tests)
- CreateUserDto: Property initialization, setters, various input scenarios
- UpdateUserDto: Property initialization, nullable properties, validation
- UserDto: Property management and null handling

### Mapping Tests (7 tests)
- AutoMapper configuration validation
- User to UserDto mapping
- CreateUserDto to User mapping
- UpdateUserDto to User mapping
- Collections mapping
- Null property handling

### Repository Tests (13 tests)
- GetAllAsync: Empty and populated collections
- GetByIdAsync: Existing and non-existing users
- CreateAsync: User creation and ID generation
- UpdateAsync: Successful updates and non-existing users
- DeleteAsync: Successful deletion and non-existing users
- Complete data preservation

### Service Tests (13 tests)
- CreateAsync: User creation with business logic (IsAuthorized, DepartmentMajor override)
- GetAllAsync: User retrieval and DTO mapping
- GetByIdAsync: Single user retrieval
- UpdateAsync: User updates and validation
- Edge cases: Empty strings, non-existing users

### Controller Tests (13 tests)
- GetAll: Successful retrieval and empty collections
- GetById: Existing users, non-existing users, edge cases
- Create: Valid DTOs, invalid ModelState, CreatedAtAction responses
- Update: Successful updates, ID mismatches, non-existing users
- Service interaction verification

## Test Patterns

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern for clarity and maintainability.

### In-Memory Database
Repository and service tests use Entity Framework's in-memory database provider to avoid external dependencies.

### Mocking
Controller tests use Moq to mock service dependencies, ensuring true unit testing isolation.

### Descriptive Naming
Test names clearly describe the scenario being tested and the expected outcome.

## Adding New Tests

When adding new functionality:

1. Create tests before implementing features (TDD approach)
2. Follow existing naming conventions
3. Use FluentAssertions for readable assertions
4. Ensure proper test isolation (unique database names, fresh mocks)
5. Test happy paths, edge cases, and failure scenarios
6. Verify all method interactions with mocks

## Continuous Integration

These tests are designed to run in CI/CD pipelines without external dependencies.