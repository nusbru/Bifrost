# Bifrost AI Coding Agent Instructions

## Project Overview
Bifrost is a **.NET 9 job application tracker** using a **clean architecture** with layered domain separation. The system helps users manage job opportunities, track applications, and store related notes.

- **Main Stack**: ASP.NET Core 9, Entity Framework Core 9, PostgreSQL 16
- **Database**: PostgreSQL running in Docker via `compose.yaml`
- **Documentation**: Scalar API reference UI at `/docs` (dev mode)

## Architecture Layers

### 1. **API Layer** (`src/Bifrost.Api`)
- **Entry point**: `Program.cs` - configures DI, EF Core, and endpoint groups
- **Endpoints**: Located in `Endpoints/` directory, mapped using ASP.NET Core Minimal APIs
  - Each entity (Job, JobApplication, ApplicationNote, Preferences) has its own `*Endpoints.cs` file
  - Pattern: `MapGroup("/api/{resource}").WithTags(...).WithOpenApi()` for OpenAPI docs
  - All endpoints return `IResult` with typed responses and error handling

### 2. **Core Layer** (`src/Bifrost.Core`)
- **Models**: Base `Entity` class (with `Id`, `SupabaseUserId`, timestamps) in `Models/`
  - All entities inherit from `Entity`
  - Core models: `Job`, `JobApplication`, `ApplicationNote`, `Preferences`
- **Repositories**: Interface-only (`IRepository<T>` in `Repositories/`) - implementations in Infrastructure
  - Generic interface pattern with LINQ `Find()` method for querying
- **Services**: Business logic in `Services/`
  - Pattern: `IJobService` (interface) + `JobService` (implementation)
  - Validation methods prefixed with `Validate*()` throw `ArgumentException` for invalid input
  - Database writes via repository, DI-injected

### 3. **Contracts Layer** (`src/Bifrost.Contracts`)
- Request/Response DTOs organized by entity type
- Pattern: `CreateJobRequest`, `JobResponse`, `UpdateJobRequest`, `UpdateJobStatusRequest`
- No business logic - pure data contracts

### 4. **Infrastructure Layer** (`src/Bifrost.Infrastructure`)
- **DependencyInjection.cs**: Central DI registration using `AddInfrastructure(config)` extension
- **DbContext**: `BifrostDbContext` with 4 DbSets (Jobs, JobApplications, ApplicationNotes, Preferences)
- **Repositories**: Concrete implementations in `Persistence/Repositories/`
- **EF Configurations**: Fluent API mappings in `Persistence/Configurations/`
  - Base: `EntityConfiguration<TEntity>` sets up key and SupabaseUserId requirement

## Key Patterns

### Dependency Injection
- **Scoped** registrations for DbContext and repositories
- **Scoped** for services (`AddScoped<IJobService, JobService>()`)
- Configuration: `AddInfrastructure()` in Infrastructure, service registration in Program.cs

### Entity Relationships
- One-to-many: Job → JobApplication (navigation property in Job model)
- All queries filtered by `SupabaseUserId` (multi-tenant via Supabase auth)

### Request/Response Mapping
- Services return domain models (`Job`, `JobApplication`, etc.)
- Endpoints manually map to contract DTOs before returning
- No AutoMapper - keep mappings visible in endpoint handlers

### Validation
- Input validation in **services** (null/empty checks)
- Throws `ArgumentException` with descriptive messages
- Endpoint handlers depend on service validation, not request filtering

## Testing

### Test Structure
- **Unit Tests**: `test/Bifrost.Core.Tests/` - mock repositories with NSubstitute
- **Infrastructure Tests**: `test/Bifrost.Infrastructure.Tests/` - repository implementations
- **API Tests**: `test/Bifrost.Api.Tests/` - endpoint handlers
- **Integration Tests**: `test/Bifrost.Integration.Tests/` - end-to-end HTTP API flows

### Testing Tools
- **Framework**: xUnit with `Fact`/`Theory` attributes
- **Assertions**: FluentAssertions (e.g., `.Should().Be()`, `.Should().ThrowAsync<>()`)
- **Mocking**: NSubstitute (`Substitute.For<IInterface>()`, `Received()`)
- **Coverage**: Coverlet (configured in `coverlet.runsettings`)

### Unit Test Pattern (from JobServiceTests)
```csharp
public async Task CreateJobAsync_WithValidData_CreatesJobSuccessfully()
{
    // Arrange
    var userId = Guid.NewGuid();
    // Act
    var result = await _jobService.CreateJobAsync(...);
    // Assert
    result.Should().NotBeNull();
    await _jobRepositoryMock.Received(1).Add(Arg.Any<Job>());
}
```

### Integration Testing

**Setup & Fixtures** (`test/Bifrost.Integration.Tests/Fixtures/`)
- `TestApiFactory`: WebApplicationFactory using in-memory database
  - Configures DI to replace PostgreSQL with EF Core in-memory provider
  - Provides `CreateClient()` for HttpClient
  - `ClearDatabaseAsync()` ensures test isolation
- `IntegrationTestBase`: Base class implementing `IAsyncLifetime`
  - Automatic database cleanup before/after each test
  - Helper methods: `DeserializeResponseAsync<T>()`, `ParseProblemDetailsAsync()`
  - Test users: `TestUserId` and `AnotherTestUserId` for multi-user scenarios

**Test Coverage** - All endpoints tested with:
- ✅ Happy path (201 Created, 200 OK, 204 NoContent)
- ✅ Validation errors (400 BadRequest - empty fields, invalid ranges)
- ✅ Not found scenarios (404 NotFound - non-existent resources)
- ✅ Multi-user isolation (SupabaseUserId filtering)
- ✅ Dependency validation (e.g., creating application for non-existent job)

**Test Files**:
- `JobEndpointsTests.cs` - Jobs CRUD + user-specific queries
- `JobApplicationEndpointsTests.cs` - Applications CRUD + job queries
- `ApplicationNoteEndpointsTests.cs` - Notes CRUD + application isolation
- `PreferencesEndpointsTests.cs` - Preferences CRUD + salary range validation

**Running Integration Tests**:
```bash
# Run all integration tests
dotnet test test/Bifrost.Integration.Tests

# Run specific test class
dotnet test test/Bifrost.Integration.Tests --filter "ClassName=Bifrost.Integration.Tests.Endpoints.JobEndpointsTests"

# Run with coverage
dotnet test test/Bifrost.Integration.Tests /p:CollectCoverage=true
```

**Important Notes**:
- In-memory database is automatically created/destroyed per test
- No PostgreSQL Docker container required for integration tests
- Each test is isolated - previous test data doesn't affect subsequent tests
- Tests validate both successful operations and error conditions

## Database & Migrations

- **Connection string** (dev): `Host=localhost;Database=BifrostDB;Username=bifrost_user;Password=bifrost_pass;Port=5432`
- **Docker startup**: `docker-compose up` - PostgreSQL with health checks
- **Entity Framework Migrations**: Located in `Persistence/Migrations/` - apply with standard EF tooling

## Enums

- **JobType**: Enum in `Core/Enums/` - FullTime, PartTime, Contract, Freelance, etc.
- **JobApplicationStatus**: Tracked separately, referenced in models
- Stored as integer in DB, deserialized to enum in models

## Development Workflow

1. **Start Docker**: `docker-compose up -d` (PostgreSQL)
2. **Build**: `dotnet build` or rebuild in IDE
3. **Tests**: `dotnet test` (all test projects)
4. **Run API**: `dotnet run --project src/Bifrost.Api` - listen on https://localhost:7001
5. **API Docs**: Visit `https://localhost:7001/docs` (Scalar UI)

## Code Style & Conventions

- **Nullable reference types enabled**: All projects use `<Nullable>enable</Nullable>`
- **Implicit usings**: Enabled - no need for repetitive `using` statements
- **Error responses**: Return `ProblemDetails` with HTTP status codes (400, 404, etc.)
- **Naming**: Service methods use `*Async` suffix, repositories use generic operations
- **Validation errors**: Captured as `ArgumentException` in services, handled in endpoint middleware

## Common Development Tasks

- **Add endpoint**: Create method in `Endpoints/*Endpoints.cs`, map in group, inject service
- **Add service logic**: Implement in `Services/*Service.cs`, register in DI, call from endpoint
- **Add database entity**: Define model in `Core/Models/`, create repository interface, implement in Infrastructure, add DbSet to context
- **Update database schema**: Add migration, update entity configuration in `Persistence/Configurations/`

## External Dependencies

- **Supabase**: Auth via `SupabaseUserId` (Guid) on all entities - acts as tenant identifier
- **Scalar.AspNetCore**: Interactive API documentation library
- **Entity Framework Core 9**: ORM for PostgreSQL
