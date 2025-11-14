# AudiSoft School API - AI Coding Agent Instructions

## Architecture Overview

This is an ASP.NET Core Web API following **Clean Architecture** with 4 layers:
- **API** (`AudiSoft.School.Api`) - Controllers, middleware, DI configuration
- **Application** (`AudiSoft.School.Application`) - Services, DTOs, business logic
- **Domain** (`AudiSoft.School.Domain`) - Entities, exceptions, business rules  
- **Infrastructure** (`AudiSoft.School.Infrastructure`) - Data access, EF Core, repositories

## Key Patterns & Conventions

### Entity Pattern
- All entities inherit from `BaseEntity` with audit fields (`CreatedAt`, `UpdatedAt`, `IsDeleted`)
- **Soft delete** is implemented globally via EF Core query filters: `entity.HasQueryFilter(e => !e.IsDeleted)`
- UTC timestamps: Always use `DateTime.UtcNow` for consistency

### Repository Pattern
- Generic `IRepository<T>` with specific interfaces like `IEstudianteRepository` 
- All operations use **database transactions** (see `Repository<T>.AddAsync()`)
- Exposes `Query()` method for complex filtering via `IQueryableExtensions`

### Service Layer Pattern
- Services orchestrate business logic and validation
- **FluentValidation** for all DTOs (registered in `Program.cs`)
- Custom exceptions: `EntityNotFoundException`, `InvalidEntityStateException`

### Query & Pagination
- `QueryParams` class supports advanced filtering: `"Nombre:Juan|Id=5"` (OR/AND logic)
- All list endpoints return `PagedResult<T>` with `X-Total-Count` header
- Use `IQueryableExtensions.ApplyFilter()` and `.ApplyPagingAsync()` for consistency

## Development Workflows

### Adding New Entities
1. Create entity in `Domain/Entities` inheriting `BaseEntity`
2. Add `DbSet<T>` to `AudiSoftSchoolDbContext` with query filter
3. Create DTOs in `Application/DTOs` (Create/Update variants)
4. Add AutoMapper mappings in `MappingProfile`
5. Create validators in `Application/Validators`
6. Implement repository interface and service
7. Create controller following existing pattern

### Database Changes
- Migrations are in `Infrastructure/Migrations`
- Connection string in `appsettings.Development.json`
- Use `dotnet ef migrations add` from `Infrastructure` project directory

### Exception Handling
- Global middleware in `ExceptionHandlingMiddleware` converts domain exceptions to HTTP responses
- Custom exceptions map to specific status codes (404, 400, 500)
- Always log with structured logging: `_logger.LogInformation("Message {Parameter}", value)`

## Project Configuration

### Dependencies
- **EF Core** with SQL Server
- **AutoMapper** for entity-DTO mapping  
- **FluentValidation** for input validation
- **Serilog** for structured logging to console + files
- **Swagger/OpenAPI** with full documentation

### Build & Run
```bash
cd Backend/src/AudiSoft.School.Api
dotnet run
```
- Swagger UI available at root URL in development
- Logs written to `Logs/audisoft-log-{date}.txt`

### API Standards
- All endpoints use `/api/v1/[controller]` routing
- Consistent response structure with proper HTTP status codes
- Search endpoints support both simple and advanced filtering
- Full XML documentation for Swagger generation

## Critical Integration Points

- **Dependency Injection**: All services registered in `Program.cs` with proper lifetimes
- **Database Context**: Configured with soft delete query filters and SQL Server
- **Validation Pipeline**: FluentValidation integrated at service layer, not controller level
- **Logging**: Serilog configured early in `Program.cs` with enrichers and multiple sinks