# AudiSoft School — Full Stack (Backend .NET 8 + Frontend AngularJS)

Sistema de gestión escolar compuesto por:
- Backend: API REST en .NET 8 (Clean Architecture, EF Core, JWT, Swagger)
- Frontend: SPA en AngularJS 1.8 + Bootstrap 5 (auth JWT, UI por rol, componentes reutilizables, reportes)

Este README ofrece una visión integral y configuración para desarrollo local y producción en Azure. Para detalles completos, consulta:
- Backend: `Backend/README.md`
- Frontend: `Frontend/README.md`

---

## 🏗️ Arquitectura General

```
Prueba-Tecnica---Audisoft/
├── Backend/
│   ├── src/
│   │   ├── AudiSoft.School.Api/            # API ASP.NET Core (.NET 8)
│   │   ├── AudiSoft.School.Application/    # Servicios, DTOs, validaciones
│   │   ├── AudiSoft.School.Domain/         # Entidades y reglas de dominio
│   │   └── AudiSoft.School.Infrastructure/ # EF Core, repositorios, migraciones
│   ├── scripts/
│   │   └── 01_CreateTables_And_Seed.sql   # ⭐ Script único de setup BD
│   └── README.md                           # Guía completa del backend
└── Frontend/
    ├── app/                                # Código AngularJS modularizado
    ├── assets/                             # Estilos y recursos
    ├── .env / .env.development             # Configuración por entorno
    └── README.md                           # Guía completa del frontend
```

### Funcionalidades clave
- Autenticación JWT y autorización por roles: Admin, Profesor, Estudiante.
- CRUD de Estudiantes, Profesores y Notas con filtros avanzados, orden y paginación.
- Reportes de Notas: resumen (totales, promedio, top por grupo, distribución) y exportación CSV.
- Frontend con componentes reutilizables (tabla, modal, input), validación de formularios y UX global (overlay de carga, toasts).

---

## ✅ Requisitos

- .NET 8 SDK (para compilar/ejecutar el backend)
- SQL Server (LocalDB/Express) o contenedor Docker de SQL Server
- Un servidor HTTP estático para el frontend (Docker, Python, Node o Live Server)

Opcional pero recomendado:
- Docker (para levantar SQL Server y/o servir el frontend fácilmente)
- VS Code + extensión Live Server
- Azure CLI (para despliegue en nube)

---

## 🔐 Usuarios de Prueba (Locales y Producción)

Las contraseñas se codifican con **SHA256 + Salt: `AudiSoft_School_Salt_2024`**

| Usuario | Contraseña | Rol | Email |
|---------|------------|-----|-------|
| `admin` | `Admin@123456` | Admin | admin@audisoft.com |
| `maria.garcia` | `Profesor@123` | Profesor | maria.garcia@audisoft.com |
| `carlos.rodriguez` | `Profesor@123` | Profesor | carlos.rodriguez@audisoft.com |
| `juan.perez` | `Estudiante@123` | Estudiante | juan.perez@student.audisoft.com |
| `sofia.martin` | `Estudiante@123` | Estudiante | sofia.martin@student.audisoft.com |

---

## 🚀 Quick Start (entorno limpio, multiplataforma)

### 1) Base de datos

#### Opción A: Docker SQL Server

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

#### Opción B: SQL Server LocalDB (Windows)

```powershell
# Ya viene con Visual Studio o SQL Server Developer Edition
# Verificar estado:
sqllocaldb info
sqllocaldb start MSSQLLocalDB
```

### 2) Backend (.NET 8)

```bash
cd Backend

# Restaurar paquetes
dotnet restore

# Ejecutar script SQL único (setup BD completo)
# Windows LocalDB:
cd scripts
sqlcmd -S "(localdb)\MSSQLLocalDB" -i 01_CreateTables_And_Seed.sql
cd ..

# O Docker SQL Server:
# sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -i scripts/01_CreateTables_And_Seed.sql

# Ajustar configuración (si usas Docker SQL Server):
# Editar src/AudiSoft.School.Api/appsettings.Development.json:
#   "ConnectionStrings": { 
#     "DefaultConnection": "Server=localhost,1433;Database=AudiSoftSchoolDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
#   }
#   "Cors": { "AllowedOrigins": [ "http://localhost:8080" ] }

# Ejecutar API
cd src/AudiSoft.School.Api
dotnet run
```

La API expone Swagger en `http://localhost:5281` (o el puerto configurado).

### 3) Frontend (SPA estática)

Elige una opción:

#### Docker (nginx) - Recomendado ⭐
```bash
cd Frontend
docker run --rm -p 8080:80 -v "$PWD":/usr/share/nginx/html:ro nginx:alpine
```

#### Python 3
```bash
cd Frontend
python3 -m http.server 8080
```

#### Node.js (http-server)
```bash
npm install -g http-server
cd Frontend
http-server -p 8080 --cors
```

#### VS Code Live Server
Click derecho en `index.html` → "Open with Live Server".

### 4) Probar la app

- Abre `http://localhost:8080`
- Inicia sesión con: `admin` / `Admin@123456`
- Navega por Dashboard, Notas (CRUD), Estudiantes/Profesores y Reportes

---

## 🌐 Configuración por Entorno

### Desarrollo Local

**Backend** (`src/AudiSoft.School.Api/appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AudiSoftSchoolDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:8080"]
  }
}
```

**Frontend** (`.env.development`):
```
API_BASE_URL_DEVELOPMENT=http://localhost:5281/api/v1
UI_TOAST_DURATION=5000
PAGINATION_DEFAULT_PAGE_SIZE=20
```

### Producción (Azure)

#### Recursos Azure Creados

- **Resource Group**: `rg-audisoft-school` (East US 2)
- **App Service Backend**: `app-audisoft-api` (Linux, B1)
  - Runtime: .NET 8
  - URL: `https://app-audisoft-api.azurewebsites.net`
- **App Service Frontend**: `app-audisoft-web` (Linux, B1)
  - Runtime: Node.js 20-lts
  - URL: `https://app-audisoft-web.azurewebsites.net`
- **SQL Database**: `AudiSoftSchoolDb`
  - Server: `servidor-audisoft-1763149184.database.windows.net`
  - Tier: Basic (5 DTUs)
  - Credenciales: `adminuser` / `StrongPwd@2024`

#### Backend Configuration (Producción)

`src/AudiSoft.School.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:servidor-audisoft-1763149184.database.windows.net,1433;Initial Catalog=AudiSoftSchoolDb;Persist Security Info=False;User ID=adminuser;Password=StrongPwd@2024;Encrypt=true;Connection Timeout=30;TrustServerCertificate=false"
  },
  "Cors": {
    "AllowedOrigins": ["https://app-audisoft-web.azurewebsites.net"]
  },
  "Swagger": {
    "Enabled": true
  }
}
```

#### Frontend Configuration (Producción)

`.env`:
```
API_BASE_URL_PRODUCTION=https://app-audisoft-api.azurewebsites.net/api/v1
API_TIMEOUT=30000
PAGINATION_DEFAULT_PAGE_SIZE=20
```

#### CI/CD (GitHub Actions)

Workflows automáticos en `.github/workflows/`:
- `deploy-backend.yml`: Build .NET 8, publica en App Service Backend
- `deploy-frontend.yml`: Build Node.js, publica en App Service Frontend

Trigger: Cada push a `main`

Secretos requeridos en GitHub:
- `AZURE_SUBSCRIPTION_ID`
- `AZURE_CREDENTIALS`
- `AZURE_RESOURCE_GROUP`
- `AZURE_APP_SERVICE_API`
- `AZURE_APP_SERVICE_WEB`

---

## 📋 Checklist de Setup Inicial

- [ ] Clonar repo: `git clone https://github.com/CodingToShare/Prueba-Tecnica---Audisoft.git`
- [ ] Instalar .NET 8 SDK
- [ ] Instalar SQL Server (LocalDB o Docker)
- [ ] Ejecutar script BD: `sqlcmd ... -i Backend/scripts/01_CreateTables_And_Seed.sql`
- [ ] Ajustar `appsettings.Development.json` (cadena de conexión, CORS)
- [ ] Levantar Backend: `cd Backend/src/AudiSoft.School.Api && dotnet run`
- [ ] Levantar Frontend: `cd Frontend && python3 -m http.server 8080` (o Docker/Node)
- [ ] Acceder a `http://localhost:8080` y loguear con credenciales

---

## 🔧 Solución de Problemas (rápido)

| Problema | Solución |
|----------|----------|
| CORS bloquea peticiones | Agrega origen en `Cors:AllowedOrigins` y reinicia API |
| 401/403 en login | Verifica credenciales; comprueba que BD tiene datos |
| `.env.development` no carga | Usa servidor que sirva dotfiles (nginx/http-server sí) |
| Reportes vacíos | Ajusta fechas en filtros; verifica que existan notas |
| BD no conecta (local) | Verifica LocalDB está running: `sqllocaldb start MSSQLLocalDB` |
| API no responde | Verifica puerto en `launchSettings.json`; prueba en Swagger |

---

## 📚 Referencias

- Backend: `Backend/README.md`
- Frontend: `Frontend/README.md`
- SQL Script: `Backend/scripts/01_CreateTables_And_Seed.sql`

---

Desarrollado con enfoque modular, Clean Architecture y CI/CD automatizado en Azure.