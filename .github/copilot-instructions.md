docker-compose build
docker-compose up -d
dotnet restore
dotnet run --project src/AudiSoft.School.Api
dotnet ef database update --startup-project ../AudiSoft.School.Api
dotnet ef migrations add DescriptionOfChange --startup-project ../AudiSoft.School.Api
dotnet ef database update --startup-project ../AudiSoft.School.Api
dotnet test
# AudiSoft School – AI Coding Agent Guide (Condensed)

Purpose: Give an AI agent immediate, safe productivity across the .NET 8 Clean Architecture backend and AngularJS 1.8 SPA frontend. Keep changes consistent with existing patterns; do not introduce new architectural styles.

## Backend Essentials (Clean Architecture)
Layers: Api (`Program.cs`, Controllers), Application (DTOs, Services, Validators, Mappings), Domain (Entities + Exceptions), Infrastructure (EF Core, Repositories, Migrations).
Entities: All inherit `BaseEntity` (CreatedAt, UpdatedAt, IsDeleted). Soft delete enforced via global query filters (see `AudiSoftSchoolDbContext.cs`: `entity.HasQueryFilter(e => !e.IsDeleted)`). Use `DateTime.UtcNow` only.
Repositories: Generic `Repository<T>` + specific (e.g. `IEstudianteRepository`, implementation `EstudianteRepository`). Pattern: expose `Query()` for IQueryable, add focused helpers (e.g. `GetByNombreAsync`).
Services: Orchestrate validation + repository access (e.g. `EstudianteService`). Inject validators (`FluentValidation`) and map errors to custom exceptions (`EntityNotFoundException`, `InvalidEntityStateException`).
Program Startup (`Program.cs`): Registers Serilog early, adds validators, AutoMapper, repositories, services, JWT auth & role policies (e.g. `AdminOnly`, `ProfesorOrAdmin`). CORS policy `AllowFrontend` exposes pagination headers.
Pagination/Filtering: Incoming `QueryParams` processed by extensions to build dynamic filters; list endpoints return `PagedResult<T>` + headers `X-Total-Count`. Preserve these headers when adding new list endpoints.
Controller Pattern: Attribute routing `[Route("api/v1/[controller]")]`, role-based authorization, logging context-rich messages, adding `X-Total-Count` before returning. Example: `EstudiantesController.GetAll()` applies user role filters then `return Ok(result)`.
Error Handling: Global `ExceptionHandlingMiddleware` converts domain/application exceptions → structured HTTP responses. Throw, don't manually create error responses (except validation guard clauses). Avoid swallowing exceptions—let middleware format.

## Backend Workflows
Run Dev: `cd Backend/src/AudiSoft.School.Api; dotnet run` (migrations + seeding auto-run in Development/Production block). Tests: `cd Backend; dotnet test` (integration uses in-memory host factory). Migrations: `cd Backend/src/AudiSoft.School.Infrastructure; dotnet ef migrations add <Name> --startup-project ../AudiSoft.School.Api` then `dotnet ef database update`.
DB Reset (Full Seed): Execute `Backend/scripts/01_CreateTables_And_Seed.sql` via `sqlcmd` (drops & recreates plus seed users/roles/notas).
Add Feature (API): Entity → DbContext `DbSet` + query filter → DTOs (Create/Update/View) → AutoMapper profile → Validator(s) → Repository method(s) only if not covered by generic → Service method → Controller action (apply role policy, set headers). Keep naming and folder parity with existing examples.

## Frontend Essentials (AngularJS 1.8 SPA)
Entry: `index.html` loads scripts; single root module `audiSoftSchoolApp`. Routes in `app.routes.js` with `access: { requiresLogin, allowedRoles }` consumed by `run()` guard logic.
Config Loading: `env-config-loader.service.js` fetches `.env.development` or `.env` → `config.service.js` normalizes API base & auth storage keys; never hardcode URLs in feature code.
Authentication: Active implementation in `app/core/auth/auth.service.js`; interceptor `auth.interceptor.js` injects `Authorization: Bearer <token>` and handles refresh + queued retries. Use `authService.login({ email, password })`, `authService.refreshAccessToken()`.
HTTP Pattern: Always use `apiService` (builds URL, attaches pagination, normalizes errors, surfaces `totalCount` from headers). Example: `apiService.get('estudiantes', { page: 1 }).then(r => vm.estudiantes = r.data);`.
Controller Style: ControllerAs + `vm`; initialize through `activate()`; keep synchronous assignments at top; isolate API calls into named functions.
Adding Feature (UI): Create `app/features/<name>/<name>.controller.js` + `<name>.html`; add route with `access`; append script tag to `index.html` maintaining existing ordering (core → features). Use `apiService` for CRUD; do not manually set auth headers.

## Cross-Cutting Conventions
JWT Roles: Admin (full), Profesor (notes + read students), Estudiante (own data). Enforce server-side; client guards are UX only.
Logging: Use structured logging with placeholders (`_logger.LogInformation("Creando estudiante {Nombre}", dto.Nombre)`). Do not add console logging in frontend—centralize via Angular services if needed.
Headers: Preserve `X-Total-Count` (and if added, keep naming consistent). Expose new pagination headers through CORS config if introduced.
Security: Do not relax `RequireAuthenticatedUser` fallback policy; new endpoints needing anonymous access must explicitly allow it.

## Safe Modification Checklist
1. Confirm layer boundary: controller <-> service <-> repository.
2. Reuse existing DTO shapes / mapping patterns; extend MappingProfile if new fields.
3. Add validators for any Create/Update DTO; fail fast before repository calls.
4. Respect soft delete: prefer repository delete → sets `IsDeleted`; avoid hard `DbContext.Remove()` unless intentional.
5. Keep Angular new scripts ordered after core services to avoid DI timing issues.

## Quick Commands
Backend run: `dotnet run --project Backend/src/AudiSoft.School.Api`
Tests: `dotnet test Backend`
Seed (manual): `sqlcmd -S "(localdb)\MSSQLLocalDB" -i Backend/scripts/01_CreateTables_And_Seed.sql`
Frontend (simple server): `npm install -g http-server; cd Frontend; http-server -p 8080`

Feedback: Ask for clarifications if adding cross-cutting concerns (caching, new headers, auth changes) before implementing.
