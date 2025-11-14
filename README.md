# AudiSoft School — Full Stack (Backend .NET 8 + Frontend AngularJS)

Sistema de gestión escolar compuesto por:
- Backend: API REST en .NET 8 (Clean Architecture, EF Core, JWT, Swagger)
- Frontend: SPA en AngularJS 1.8 + Bootstrap 5 (auth JWT, UI por rol, componentes reutilizables, reportes)

Este README ofrece una visión integral y una guía rápida de instalación conjunta. Para detalles completos, consulta los README específicos:
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
│   ├── scripts/                            # SQL y utilidades
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

---

## 🚀 Quick Start (entorno limpio, multiplataforma)

A continuación, un flujo mínimo de punta a punta. Para mayor detalle, revisa los README de Backend y Frontend.

### 1) Base de datos (opción Docker)

```bash
# Levantar SQL Server en Docker
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

Actualiza la cadena de conexión del backend si usarás Docker SQL Server (ver más abajo).

### 2) Backend (.NET 8)

```bash
cd Backend
# Restaurar paquetes
dotnet restore

# (Opcional) Aplicar migraciones EF si las usas
# cd src/AudiSoft.School.Infrastructure
# dotnet ef database update --startup-project ../AudiSoft.School.Api

# Ajustar appsettings para CORS y DB
# - src/AudiSoft.School.Api/appsettings.Development.json
#   "Cors": { "AllowedOrigins": [ "http://localhost:8080" ] }
#   "ConnectionStrings": { "DefaultConnection": "Server=localhost,1433;Database=AudiSoftSchoolDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true" }

# Ejecutar API
cd src/AudiSoft.School.Api
dotnet run
```

La API expone Swagger en el puerto configurado (ej.: http://localhost:5000) y la base `http://localhost:5000/api/v1` o similar si tu perfil/puerto difiere.

### 3) Frontend (SPA estática)

El frontend se sirve estáticamente. Elige una opción:

- Docker (nginx):
```bash
cd Frontend
docker run --rm -p 8080:80 -v "$PWD":/usr/share/nginx/html:ro nginx:alpine
```

- Python 3:
```bash
cd Frontend
python3 -m http.server 8080
```

- Node.js (http-server):
```bash
npm install -g http-server
cd Frontend
http-server -p 8080 --cors
```

Asegúrate que `Frontend/.env.development` apunte al backend (por defecto `http://localhost:5281/api/v1`). Si tu API corre en otro puerto, ajusta `API_BASE_URL_DEVELOPMENT`.

### 4) Probar la app

- Abre `http://localhost:8080`.
- Inicia sesión con usuarios de prueba (ver README del backend).
- Navega por Dashboard, Notas (CRUD por rol), Estudiantes/Profesores y Reportes (resumen + CSV).

---

## 🔧 Configuración Clave

### Backend
- CORS configurable en `appsettings*.json` vía `Cors:AllowedOrigins`.
- Cadenas de conexión en `ConnectionStrings:DefaultConnection`.
- Endpoints principales (ejemplos):
  - `POST /api/v1/Auth/login`
  - `GET /api/v1/Notas`
  - `GET /api/v1/Reportes/notas/resumen`
  - `GET /api/v1/Reportes/notas/export`

Ver detalles, ejemplos de filtros, paginación y exportación en `Backend/README.md`.

### Frontend
- Carga de config por `.env.development` (localhost) o `.env` (prod), manejada por `env-config-loader`.
- Interceptores de auth y loading ya configurados globalmente.
- Página de Reportes en `#!/reportes` con filtros de fecha y descarga CSV.

Ver estructura, opciones de despliegue estático y resolución de problemas en `Frontend/README.md`.

---

## 👥 Roles y Seguridad

- Admin: Acceso total.
- Profesor: Gestión de sus propias notas; acceso a listados permitidos.
- Estudiante: Lectura de sus propias notas.

El backend aplica el filtrado por rol en servidores; el frontend refleja y oculta/limita UI con directivas y protección de rutas.

---

## 🆘 Resolución de Problemas (rápido)

- CORS bloquea peticiones: agrega el origen del frontend a `Cors:AllowedOrigins` y reinicia la API.
- 401/403: verifica login y hora del sistema; revisa roles del usuario.
- `.env.development` no carga: usa un servidor que sirva dotfiles (nginx/http-server sí lo hacen).
- Reportes vacíos: revisa fechas `from/to` y que existan notas.

---

## 📚 Referencias
- Backend: `Backend/README.md`
- Frontend: `Frontend/README.md`

---

Desarrollado con enfoque modular y principios de Clean Architecture.