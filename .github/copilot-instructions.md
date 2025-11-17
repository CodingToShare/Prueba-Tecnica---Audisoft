# AudiSoft School - AI Coding Agent Instructions

A full-stack education management system with **ASP.NET Core 8 backend** (Clean Architecture) and **AngularJS 1.8 frontend** (SPA).

## 🏗️ Architecture Overview

```
AudiSoft School/
├── Backend/                     # ASP.NET Core 8 API (Clean Architecture)
│   ├── src/
│   │   ├── AudiSoft.School.Api/             # Controllers, middleware, DI
│   │   ├── AudiSoft.School.Application/     # Services, DTOs, business logic
│   │   ├── AudiSoft.School.Domain/          # Entities, exceptions, rules
│   │   └── AudiSoft.School.Infrastructure/  # EF Core, repositories, migrations
│   ├── tests/                               # Integration & unit tests
│   ├── scripts/                             # SQL setup scripts
│   └── Dockerfile                           # Multi-stage .NET 8 build
│
├── Frontend/                    # AngularJS 1.8 SPA (no build step)
│   ├── app/
│   │   ├── core/                # Auth, routing, config, interceptors
│   │   ├── features/            # Estudiantes, Profesores, Notas, Dashboard
│   │   ├── shared/              # Reusable components, directives
│   │   └── assets/              # CSS, images, Bootstrap 5
│   ├── index.html               # Main entry point
│   ├── server.js                # Express.js static server
│   └── Dockerfile               # Multi-stage Node 20 build
│
├── docker-compose.yml           # Orchestrates Backend + Frontend + SQL Server
├── Makefile                     # Helper commands (make up, make logs, etc.)
├── docker-helper.sh             # Bash script with Docker utilities
└── DOCKER_SETUP.md             # Complete Docker documentation
```

## 🚀 Quick Start

### Docker (Recommended)
```bash
# Build all images
docker-compose build

# Start all services (Backend + Frontend + SQL Server)
docker-compose up -d

# Check status
make status

# View logs
make logs

# Access services
# Frontend: http://localhost:8080
# API: http://localhost:5281
# SQL Server: localhost:1433
```

### Local Development

**Backend:**
```bash
cd Backend
dotnet restore
# Setup database (see Database Setup section)
dotnet run --project src/AudiSoft.School.Api
```

**Frontend:**
```bash
cd Frontend
npm install
npm start
# Open http://localhost:8080
```

## 🗄️ Database Setup

### Option 1: SQL Script (Complete Reset) ⭐
```bash
# Windows LocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -i Backend/scripts/01_CreateTables_And_Seed.sql

# Docker SQL Server
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -i Backend/scripts/01_CreateTables_And_Seed.sql
```
Creates database, tables, indices, test data (3 roles, 5 professors, 10 students, 10 grades).

### Option 2: Entity Framework Migrations
```bash
cd Backend/src/AudiSoft.School.Infrastructure
dotnet ef database update --startup-project ../AudiSoft.School.Api
```

### Connection Strings
Update `Backend/src/AudiSoft.School.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=AudiSoftSchool;User Id=sa;Password=YourStrong@Passw0rd;Encrypt=false;TrustServerCertificate=true;"
  }
}
```

## 🔑 Backend Patterns

### Clean Architecture Layers
1. **Domain**: Entities (inherit `BaseEntity`), exceptions, business rules
2. **Application**: DTOs, services, validators, mappings
3. **Infrastructure**: EF Core context, repositories, migrations
4. **API**: Controllers, middleware, dependency injection

### Key Patterns
- **Entities**: Soft delete (`IsDeleted`), audit fields (`CreatedAt`, `UpdatedAt`), UTC timestamps
- **Repositories**: Generic `IRepository<T>` + specific interfaces (`IEstudianteRepository`)
- **Services**: Orchestrate business logic, validate with FluentValidation
- **Exceptions**: Custom domain exceptions map to HTTP status codes
- **Pagination**: `PagedResult<T>` with `X-Total-Count` header

### Adding New Features
1. Create entity in `Domain/Entities` inheriting `BaseEntity`
2. Add `DbSet<T>` to `AudiSoftSchoolDbContext` with query filter
3. Create DTOs in `Application/DTOs` (Create/Update variants)
4. Add AutoMapper mappings in `MappingProfile`
5. Create validators in `Application/Validators`
6. Implement repository and service
7. Add controller following existing pattern (e.g., `EstudiantesController`)

### Database Migrations
```bash
cd Backend/src/AudiSoft.School.Infrastructure
dotnet ef migrations add DescriptionOfChange --startup-project ../AudiSoft.School.Api
dotnet ef database update --startup-project ../AudiSoft.School.Api
```

## 🎨 Frontend Patterns

### Architecture
- **Entry**: `index.html` wires all scripts
- **Routing**: `app/routes.js` with AngularJS `ngRoute`
- **Auth**: JWT tokens in localStorage, auto-refresh on 401
- **Config**: `.env.development`/`.env.production` loaded via HTTP

### Key Files
- `app/core/services/env-config-loader.service.js`: Loads environment
- `app/core/auth/auth.service.js`: JWT authentication, token refresh
- `app/core/interceptors/auth.interceptor.js`: Injects Bearer tokens
- `app/core/services/api.service.js`: HTTP wrapper with pagination support
- `app/routes.js`: Route definitions with access control

### Controllers Pattern
Use `controllerAs` and `vm` pattern:
```javascript
angular
  .module('audiSoftSchoolApp')
  .controller('EstudiantesController', EstudiantesController);

function EstudiantesController(apiService, $scope, $log) {
  var vm = this;
  
  vm.estudiantes = [];
  vm.loadEstudiantes = loadEstudiantes;
  
  function loadEstudiantes() {
    apiService.get('estudiantes', { page: 1 })
      .then(res => {
        vm.estudiantes = res.data;
        vm.totalCount = res.totalCount;
      });
  }
  
  activate();
  function activate() {
    loadEstudiantes();
  }
}
```

### Services & HTTP
Use `apiService` (not raw `$http`):
```javascript
apiService.get('students')        // GET /api/v1/students
apiService.post('students', data) // POST /api/v1/students
apiService.put('students/5', data) // PUT /api/v1/students/5
apiService.delete('students/5')   // DELETE /api/v1/students/5
```

### Routing with Access Control
```javascript
.when('/estudiantes', {
  templateUrl: 'app/features/estudiantes/estudiantes.html',
  controller: 'EstudiantesController',
  controllerAs: 'vm',
  access: { requiresLogin: true, allowedRoles: ['Admin', 'Profesor'] }
})
```

### Adding New Features
1. Create `app/features/foo/foo.controller.js` and `foo.html`
2. Add route in `app/routes.js` with `access` object
3. Register controller script in `index.html`
4. Use `apiService` for HTTP calls
5. Follow existing patterns (pagination, error handling, loading states)

### Environment Configuration
Create `.env.development`:
```
API_BASE_URL_DEVELOPMENT=http://localhost:5281/api/v1
API_TIMEOUT=30000
AUTH_TOKEN_KEY=audisoft_token
AUTH_USER_KEY=audisoft_user
AUTH_REFRESH_TOKEN_KEY=audisoft_refresh_token
DEBUG_MODE=true
```

## 🐳 Docker Build & Deploy

### Building Images
- **Backend**: Multi-stage SDK 8.0 (build) + AspNet 8.0 (runtime)
- **Frontend**: Multi-stage Node 20 (build) + Node 20-alpine (runtime)
- **Database**: SQL Server 2022 with persistent volume

### Helper Commands
```bash
make build              # Build all images
make up                 # Start services
make down               # Stop services
make logs               # View all logs
make status             # Check health
make clean              # Remove containers/volumes
make shell-backend      # Access backend container
```

### Docker Compose Services
- `sqlserver`: Port 1433, volume `sqlserver_data`
- `backend`: Port 5281, depends_on sqlserver
- `frontend`: Port 8080, depends_on backend

### Health Checks
Each service includes health checks. Verify with:
```bash
curl http://localhost:5281/health     # Backend
curl http://localhost:8080/health     # Frontend
```

## 🔐 Authentication

### Backend (JWT)
- `POST /api/v1/auth/login` with credentials
- Returns `{ accessToken, refreshToken, expiresIn }`
- Stored in localStorage

### Frontend
- `authService.login(credentials)` initiates JWT flow
- Interceptor auto-injects `Authorization: Bearer <token>`
- On 401, interceptor retries with refreshed token
- On persistent failure, redirects to login

### Test Credentials
From database seed:
- Admin: `admin` / `Admin@123456`
- Teacher: `profesor1` / `Profesor@123456`
- Student: `juan.perez` / `Estudiante@123456`

## 📊 Data Models

### Core Entities
- **Estudiante**: Student with name, audit fields, soft delete
- **Profesor**: Teacher with name, department, audit fields
- **Nota**: Grade linking Estudiante + Profesor, with value 0-100
- **Usuario**: User account with email, password hash, roles
- **Rol**: Role (Admin, Profesor, Estudiante)

### Relationships
- Nota → Profesor (RequiredDelete, OnDelete.Restrict)
- Nota → Estudiante (RequiredDelete, OnDelete.Restrict)
- Usuario ↔ Rol (Many-to-many via UsuarioRoles)

## 🧪 Testing

### Backend
```bash
cd Backend
dotnet test
```
Integration tests use `TestWebApplicationFactory` with in-memory database.

### Frontend
No automated tests configured; manual testing via browser.

## 🔧 Common Tasks

### Add API Endpoint
1. Create/update Entity in Domain
2. Create/update DTO in Application
3. Add validator in Application/Validators
4. Add service method in Application/Services
5. Add repository method if needed
6. Add controller action in API/Controllers
7. Regenerate migrations if entity changed

### Fix Frontend Component
1. Identify feature under `app/features/<feature>/`
2. Update `.controller.js` for logic
3. Update `.html` for UI
4. Use `apiService` for API calls
5. Follow existing error/loading patterns

### Deploy to Azure
```bash
# See azure-setup.sh for full automated deployment
# Or manual deployment via Azure Portal / Azure CLI
```

## 📚 Key Files Reference

**Backend Config**:
- `Backend/src/AudiSoft.School.Api/Program.cs` - DI, middleware setup
- `Backend/src/AudiSoft.School.Api/appsettings.Development.json` - DB, Auth config
- `Backend/src/AudiSoft.School.Infrastructure/Persistence/AudiSoftSchoolDbContext.cs` - EF Core config

**Frontend Config**:
- `Frontend/index.html` - All script includes
- `Frontend/app/core/services/config.service.js` - Runtime config
- `Frontend/server.js` - Express server setup
- `Frontend/.env.development` - Local development env

**Docker**:
- `docker-compose.yml` - Service orchestration
- `Backend/Dockerfile` - Backend image
- `Frontend/Dockerfile` - Frontend image
- `Makefile` - Common commands

## ⚠️ Important Notes

- **UTC timestamps**: Always use `DateTime.UtcNow` in backend
- **Soft delete**: All entities use `IsDeleted` flag; configure query filters in DbContext
- **Connection strings**: Different per environment; never commit sensitive data
- **JWT secrets**: Generate strong random keys; store in environment variables
- **CORS**: Configure in `appsettings.json` under `Cors:AllowedOrigins`
- **Frontend env files**: Loaded dynamically via XHR; `.env.development` used for localhost
- **Auth interceptor**: Automatically refreshes tokens; handles 401 responses
- **Error handling**: Domain exceptions converted to HTTP responses via middleware

## 📖 Documentation

- Backend detailed guide: `Backend/README.md`
- Frontend detailed guide: `Frontend/README.md`
- Docker setup: `DOCKER_SETUP.md`
- Root README: `README.md`
