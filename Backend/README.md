# AudiSoft School API - Backend

Sistema de gestión escolar desarrollado con **.NET 8** siguiendo los principios de **Clean Architecture**. Proporciona una API REST completa para la administración de estudiantes, profesores, notas y usuarios con autenticación JWT.

## 🏗️ Arquitectura

El proyecto sigue **Clean Architecture** con 4 capas bien definidas:

```
Backend/
├── src/
│   ├── AudiSoft.School.Api/           # 🌐 Capa de Presentación
│   ├── AudiSoft.School.Application/   # 💼 Capa de Aplicación  
│   ├── AudiSoft.School.Domain/        # 🏛️ Capa de Dominio
│   └── AudiSoft.School.Infrastructure/ # 🔧 Capa de Infraestructura
├── tests/
│   └── AudiSoft.School.Tests/         # 🧪 Pruebas Unitarias e Integración
├── Dockerfile                          # 🐳 Multi-stage .NET 8.0 build
└── scripts/
    ├── 01_InitialCreate.sql           # 📄 Script inicial de base de datos
    └── 02_CreateTables_And_Seed.sql   # 📄 Script de tablas y datos iniciales
```

### Capas del Sistema

- **🌐 API Layer**: Controladores REST, middleware, configuración de Swagger
- **💼 Application Layer**: Servicios, DTOs, validaciones, lógica de negocio
- **🏛️ Domain Layer**: Entidades, excepciones, reglas de negocio
- **🔧 Infrastructure Layer**: Repositorios, Entity Framework, persistencia

## 🚀 Requisitos Previos

### Para Docker (Recomendado - Sin instalar .NET ni SQL Server) ⭐
- **Docker Desktop** (Windows/macOS) o **Docker Engine** (Linux)
- **docker-compose** (incluido en Docker Desktop)
- El stack se levanta con: `docker-compose up -d`

### Para Desarrollo Local
- **Sistema Operativo**: Windows 10/11, macOS, o Linux
- **.NET 8.0 SDK** (versión 8.0.100 o superior)
- **SQL Server** (LocalDB, Express, o completo)
- **Git** (para clonar el repositorio)

## 📥 Instalación desde Cero

### Quick Start con Docker (TODO-EN-UNO)

Si ya tienes Docker instalado, ejecuta:

```bash
# Desde la raíz del proyecto
cd ..

# Editar .env con contraseña fuerte
nano .env
# SQL_SA_PASSWORD=TuContraseñaFuerte123!

# Construir imágenes
docker-compose build

# Levantar servicios (Backend + Frontend + SQL Server)
docker-compose up -d

# Verificar que están en ejecución
docker-compose ps

# Acceder
# Backend API: http://localhost:5281
# Frontend: http://localhost:8080 (login con admin/Admin@123456)
```

**Para detener:**
```bash
docker-compose down
```

**Para ver logs del backend:**
```bash
docker-compose logs -f backend
```

---

### Instalación Local (Sin Docker)

### 1. Instalar .NET 8 SDK

#### Windows
```powershell
# Opción A: Descargar desde el sitio oficial
# Ir a: https://dotnet.microsoft.com/download/dotnet/8.0
# Descargar "SDK 8.0.x" para Windows x64

# Opción B: Usar winget (Windows Package Manager)
winget install Microsoft.DotNet.SDK.8

# Opción C: Usar Chocolatey
choco install dotnet-8.0-sdk
```

#### macOS
```bash
# Opción A: Usar Homebrew
brew install dotnet@8

# Opción B: Descargar desde el sitio oficial
# Ir a: https://dotnet.microsoft.com/download/dotnet/8.0
```

#### Linux (Ubuntu/Debian)
```bash
# Agregar el repositorio de Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# Actualizar e instalar .NET SDK
sudo apt update
sudo apt install -y dotnet-sdk-8.0
```

### 2. Instalar SQL Server

#### Windows
```powershell
# Opción A: SQL Server LocalDB (recomendado para desarrollo)
# Descargar desde: https://www.microsoft.com/sql-server/sql-server-downloads
# Seleccionar "Developer" o "Express"

# Opción B: Usar Chocolatey
choco install sql-server-express localdb
```

#### macOS/Linux
```bash
# Usar Docker para SQL Server
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sqlserver --hostname sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 3. Verificar Instalaciones

```bash
# Verificar .NET SDK
dotnet --version
# Debe mostrar: 8.0.xxx

# Verificar todas las versiones instaladas
dotnet --list-sdks
# Debe incluir una versión 8.0.xxx

# Verificar SQL Server (Windows)
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION"
```

## 🛠️ Configuración del Proyecto

### 1. Clonar el Repositorio

```bash
git clone https://github.com/CodingToShare/Prueba-Tecnica---Audisoft.git
cd Prueba-Tecnica---Audisoft/Backend
```

### 2. Configurar Base de Datos

#### Opción A: Usar Script SQL Único (Recomendado) ⭐

El script `01_CreateTables_And_Seed.sql` dropea y recrea la BD completamente, asegurando consistencia total:

```bash
# Desde la carpeta Backend/scripts

# Windows con SQL Server LocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -i 01_CreateTables_And_Seed.sql

# Windows con SQL Server completo/Express
sqlcmd -S "localhost\SQLEXPRESS" -U sa -P YourPassword -i 01_CreateTables_And_Seed.sql

# Linux/macOS con SQL Server en Docker
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -i 01_CreateTables_And_Seed.sql
```

**Qué hace el script:**
- ✅ Dropea `AudiSoftSchoolDb` si existe
- ✅ Crea la BD desde cero
- ✅ Crea todas las tablas con esquema correcto (incluyendo `DeletedAt`)
- ✅ Inserta datos iniciales (3 roles, 5 profesores, 10 estudiantes)
- ✅ Crea usuarios con contraseñas codificadas correctamente
- ✅ Inserta 10 notas de ejemplo
- ✅ Actualiza tabla `__EFMigrationsHistory`

#### Opción B: Usar Entity Framework Migrations

```bash
# Restaurar paquetes
dotnet restore

# Navegar al proyecto de infraestructura
cd src/AudiSoft.School.Infrastructure

# Crear y aplicar migraciones
dotnet ef database update --startup-project ../AudiSoft.School.Api
```

#### Cadenas de Conexión

**Desarrollo Local (SQL Server LocalDB):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AudiSoftSchoolDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

**Desarrollo Local (SQL Server Express):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AudiSoftSchoolDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

**Desarrollo Local (Docker SQL Server):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=AudiSoftSchoolDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  }
}
```

**Producción (Azure SQL Database):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:servidor-audisoft-1763149184.database.windows.net,1433;Initial Catalog=AudiSoftSchoolDb;Persist Security Info=False;User ID=adminuser;Password=StrongPwd@2024;Encrypt=true;Connection Timeout=30;TrustServerCertificate=false"
  }
}
```

Editar el archivo correspondiente según tu entorno:
- **Desarrollo**: `src/AudiSoft.School.Api/appsettings.Development.json`
- **Producción**: `src/AudiSoft.School.Api/appsettings.json`

## ▶️ Ejecución

### Ejecución con Docker

```bash
# Desde la raíz del proyecto
docker-compose up -d backend

# Ver logs
docker-compose logs -f backend

# Acceder a Swagger
# http://localhost:5281

# Ver estado
docker-compose ps
```

### Ejecución Local

#### 1. Restaurar Dependencias

```bash
# Desde la carpeta Backend/
dotnet restore
```

#### 2. Compilar el Proyecto

```bash
dotnet build
```

#### 3. Ejecutar la Aplicación

```bash
# Navegar al proyecto API
cd src/AudiSoft.School.Api

# Ejecutar en modo desarrollo
dotnet run

# O con hot reload
dotnet watch run
```

#### 4. Verificar que Funciona

La aplicación estará disponible en:
- **Swagger UI**: http://localhost:5000 o https://localhost:5001
- **API Base**: http://localhost:5000/api/v1

## 🧪 Ejecutar Pruebas

### Pruebas en Docker

```bash
# Desde la raíz del proyecto
docker-compose exec backend dotnet test
```

### Pruebas Local

```bash
# Desde la carpeta Backend/
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con más detalle
dotnet test --verbosity normal

# Ejecutar solo pruebas unitarias
dotnet test --filter "FullyQualifiedName~Unit"

# Ejecutar solo pruebas de integración
dotnet test --filter "FullyQualifiedName~Integration"
```

## 🔐 Usuarios por Defecto

El sistema incluye usuarios predefinidos para pruebas. Las contraseñas se codifican con **SHA256 + Salt: `AudiSoft_School_Salt_2024`**

| Usuario | Contraseña | Rol | Email | Hash SHA256 |
|---------|------------|-----|-------|------------|
| `admin` | `Admin@123456` | Admin | admin@audisoft.com | `t2eJXPeIYQVzMAwMW+jLZKW6fWnlcISBzr7M+AF3XpI=` |
| `maria.garcia` | `Profesor@123` | Profesor | maria.garcia@audisoft.com | `lo6Y0GQuS94+AmpPL07NNn4Nr2+L/EjUuRwadRX8lOo=` |
| `carlos.rodriguez` | `Profesor@123` | Profesor | carlos.rodriguez@audisoft.com | `lo6Y0GQuS94+AmpPL07NNn4Nr2+L/EjUuRwadRX8lOo=` |
| `juan.perez` | `Estudiante@123` | Estudiante | juan.perez@student.audisoft.com | `TbtzAuoi8LpS/CcSnmRzvyX76vGPQjFLZNF78zb9HGk=` |
| `sofia.martin` | `Estudiante@123` | Estudiante | sofia.martin@student.audisoft.com | `TbtzAuoi8LpS/CcSnmRzvyX76vGPQjFLZNF78zb9HGk=` |

## 📚 Documentación API

### Swagger UI
Una vez ejecutada la aplicación, accede a la documentación interactiva:
- **URL**: http://localhost:5000
- **Funcionalidades**: 
  - Explorar todos los endpoints
  - Probar la API directamente
  - Ver modelos de datos
  - Autenticación JWT integrada

### Endpoints Principales

```
🔐 Autenticación
POST /api/v1/Auth/login          # Login y obtención de token JWT
POST /api/v1/Auth/logout         # Cerrar sesión
GET  /api/v1/Auth/me             # Información del usuario actual

👥 Gestión de Usuarios (Solo Admin)
GET    /api/v1/Usuarios          # Listar usuarios
POST   /api/v1/Usuarios          # Crear usuario
GET    /api/v1/Usuarios/{id}     # Obtener usuario
PUT    /api/v1/Usuarios/{id}     # Actualizar usuario
DELETE /api/v1/Usuarios/{id}     # Eliminar usuario

🎓 Gestión de Estudiantes
GET    /api/v1/Estudiantes       # Listar estudiantes (Admin/Profesor)
POST   /api/v1/Estudiantes       # Crear estudiante (Admin)
GET    /api/v1/Estudiantes/{id}  # Obtener estudiante
PUT    /api/v1/Estudiantes/{id}  # Actualizar estudiante (Admin)
DELETE /api/v1/Estudiantes/{id}  # Eliminar estudiante (Admin)

👨‍🏫 Gestión de Profesores (Solo Admin)
GET    /api/v1/Profesores        # Listar profesores
POST   /api/v1/Profesores        # Crear profesor
GET    /api/v1/Profesores/{id}   # Obtener profesor
PUT    /api/v1/Profesores/{id}   # Actualizar profesor
DELETE /api/v1/Profesores/{id}   # Eliminar profesor

📊 Gestión de Notas
GET    /api/v1/Notas             # Listar notas (filtrado por rol)
POST   /api/v1/Notas             # Crear nota (Profesor/Admin)
GET    /api/v1/Notas/{id}        # Obtener nota
PUT    /api/v1/Notas/{id}        # Actualizar nota (Profesor/Admin)
DELETE /api/v1/Notas/{id}        # Eliminar nota (Profesor/Admin)

📈 Reportes de Notas
GET  /api/v1/Reportes/notas/resumen   # Resumen estadístico (total, promedio, top por profesor/estudiante, distribución)
GET  /api/v1/Reportes/notas/export    # Exportación CSV (aplica mismos filtros y restricciones por rol)
```

## 🔍 Funcionalidades Avanzadas

### Filtrado Avanzado
El sistema soporta filtros complejos en todos los endpoints de listado:

```bash
# Sintaxis de filtros
campo:valor     # Contiene (LIKE)
campo=valor     # Igual
campo>valor     # Mayor que
campo<valor     # Menor que
campo>=valor    # Mayor o igual
campo<=valor    # Menor o igual

# Operadores lógicos
;              # AND
|              # OR

# Ejemplos
GET /api/v1/Estudiantes?Filter=Nombre:Juan
GET /api/v1/Notas?Filter=Valor>=80;Nombre:Matemáticas
GET /api/v1/Notas?Filter=Valor>90|Estudiante:María
```

### Reportes: Filtros y Exportación

Los reportes de notas soportan los mismos filtros avanzados y agregan parámetros específicos. El filtrado por rol se aplica siempre en el servidor: Admin ve todo; Profesor solo sus notas; Estudiante solo sus notas.

Parámetros comunes (query string):

- from: fecha inicio (YYYY-MM-DD)
- to: fecha fin (YYYY-MM-DD)
- idProfesor: filtra por profesor (efectivo principalmente para Admin)
- idEstudiante: filtra por estudiante (efectivo principalmente para Admin)
- Filter / FilterField / FilterValue: filtros avanzados existentes
- SortField / SortDesc: ordenamiento

Ejemplos:

```
# Resumen del año actual
GET /api/v1/Reportes/notas/resumen?from=2025-01-01&to=2025-12-31

# Solo profesor 3 (Admin) y orden por fecha descendente
GET /api/v1/Reportes/notas/resumen?idProfesor=3&sortField=CreatedAt&sortDesc=true

# Exportación CSV con rango de fechas
GET /api/v1/Reportes/notas/export?from=2025-01-01&to=2025-06-30
```

Detalles de exportación CSV:

- Codificación UTF-8 con BOM (compatible con Excel)
- Cabecera: `Id,Nombre,Valor,Profesor,Estudiante,CreatedAt`
- Tamaño limitado razonablemente en servidor para prevenir exportaciones excesivas

### Paginación
Todos los endpoints de listado soportan paginación:

```bash
GET /api/v1/Estudiantes?Page=1&PageSize=10&SortField=Nombre&SortDesc=false
```

### Autenticación JWT
1. **Login**: `POST /api/v1/Auth/login` con credenciales
2. **Token**: Usar el token en header `Authorization: Bearer <token>`
3. **Permisos**: Cada endpoint tiene permisos específicos por rol

## 🛠️ Tecnologías Utilizadas

- **.NET 8.0** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core 8.0** - ORM
- **SQL Server 2022** - Base de datos
- **JWT Bearer** - Autenticación
- **FluentValidation** - Validación de datos
- **AutoMapper** - Mapeo de objetos
- **Serilog** - Logging estructurado
- **Swagger/OpenAPI** - Documentación API
- **xUnit** - Testing framework
- **FluentAssertions** - Assertions para testing
- **Docker** - Containerización multi-stage
- **Docker Compose** - Orquestación 3-servicios (API, Frontend, DB)

## 📁 Estructura de Archivos

```
Backend/
├── 📁 src/
│   ├── 🌐 AudiSoft.School.Api/
│   │   ├── Controllers/           # Controladores REST
│   │   ├── Middleware/           # Middleware personalizado
│   │   └── Program.cs            # Configuración de la aplicación
│   ├── 💼 AudiSoft.School.Application/
│   │   ├── DTOs/                 # Data Transfer Objects
│   │   ├── Services/             # Servicios de aplicación
│   │   ├── Validators/           # Validadores FluentValidation
│   │   └── Interfaces/           # Contratos de repositorios
│   ├── 🏛️ AudiSoft.School.Domain/
│   │   ├── Entities/             # Entidades de dominio
│   │   └── Exceptions/           # Excepciones personalizadas
│   └── 🔧 AudiSoft.School.Infrastructure/
│       ├── Persistence/          # Contexto de EF Core
│       ├── Repositories/         # Implementación de repositorios
│       └── Migrations/           # Migraciones de base de datos
├── 🧪 tests/
│   └── AudiSoft.School.Tests/    # Pruebas unitarias e integración
├── 🐳 Dockerfile                 # Multi-stage .NET 8 → aspnet 8 (53% optimización)
├── 📄 scripts/                   # Scripts SQL
└── 📋 README.md                  # Este archivo

## 🌐 Configuración de CORS

El origen del frontend ya no está codificado en el código. Se configura desde `appsettings` mediante la sección `Cors:AllowedOrigins`.

Ejemplo (desarrollo): `src/AudiSoft.School.Api/appsettings.Development.json`

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:8080"
    ]
  }
}
```

Ejemplo (producción): `src/AudiSoft.School.Api/appsettings.json`

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://app.audisoft.com",
      "https://admin.audisoft.com"
    ]
  }
}
```

Notas:

- Si `Cors:AllowedOrigins` está vacío o ausente, en desarrollo se aplica un fallback permisivo con log de advertencia.
- Asegúrate de incluir todos los orígenes necesarios (HTTP/HTTPS y puertos correctos).
```

## 🚨 Solución de Problemas

### Error: SDK de .NET no encontrado
```bash
# Verificar instalación
dotnet --version

# Si no encuentra .NET 8, reinstalar
# Windows: Descargar desde https://dotnet.microsoft.com/download/dotnet/8.0
```

### Error: No se puede conectar a la base de datos
```bash
# Verificar SQL Server LocalDB
sqllocaldb info

# Iniciar LocalDB si está detenido
sqllocaldb start MSSQLLocalDB

# Verificar cadena de conexión en appsettings.Development.json
```

### Error: Puerto en uso
```bash
# Cambiar puerto en launchSettings.json
# O usar puerto diferente:
dotnet run --urls "http://localhost:5002"
```

### Problemas con SSL en desarrollo
```bash
# Confiar en certificados de desarrollo
dotnet dev-certs https --trust
```
