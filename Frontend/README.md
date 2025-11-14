# AudiSoft School - Frontend

SPA desarrollada en AngularJS 1.8 + Bootstrap 5 que consume la API .NET 8. Incluye autenticación JWT, control de UI por rol, componentes reutilizables, validación de formularios, UX global (loading/toasts) y una página de reportes con exportación CSV.

## 🏗️ Arquitectura y Estructura

```
Frontend/
├── index.html                        # Entrada y layout principal (navbar, toasts, overlay)
├── app.module.js                     # Módulo raíz de AngularJS
├── app.routes.js                     # Rutas, protección por rol y redirecciones
├── .env                              # Config por defecto (producción)
├── .env.development                  # Config en desarrollo (se carga automáticamente en localhost)
├── assets/
│   └── css/app.css                   # Estilos y utilidades (btns, toasts, overlay)
└── app/
        ├── core/
        │   ├── controllers/main.controller.js     # Navbar, sesión y helpers de rol
        │   ├── interceptors/
        │   │   ├── auth.interceptor.js            # Inyecta JWT, maneja 401/403 y refresh
        │   │   └── loading.interceptor.js         # Muestra/oculta overlay en llamadas API
        │   ├── services/
        │   │   ├── env-config-loader.service.js   # Carga .env(.development) vía HTTP
        │   │   ├── app-initializer.service.js     # Inicialización temprana
        │   │   ├── config.service.js              # Centraliza configuración (API, UI, paginación)
        │   │   ├── api.service.js                 # Cliente HTTP base (GET/POST/PUT/DELETE)
        │   │   ├── auth.service.js                # Autenticación y sesión JWT
        │   │   ├── route-auth.service.js          # Protección de rutas
        │   │   ├── estudiantes.service.js         # API Estudiantes
        │   │   ├── profesores.service.js          # API Profesores
        │   │   ├── notas.service.js               # API Notas
        │   │   └── reportes.service.js            # API Reportes (resumen + CSV)
        │   └── filters/range.filter.js            # Utilidades de presentación
        ├── shared/
        │   ├── components/
        │   │   ├── as-table.directive.js          # Tabla configurable con sort/paginación
        │   │   ├── as-modal.directive.js          # Modal reusable con transclusión
        │   │   └── as-input.directive.js          # Inputs con validación AngularJS + Bootstrap
        │   ├── directives/acl.directive.js        # has-role/has-any-role para UI por rol
        │   └── controllers/unauthorized.controller.js
        └── features/
                ├── login/                              # Login de usuarios
                ├── dashboard/                          # Resumen role-aware y últimas notas
                ├── estudiantes/                        # CRUD/listado de estudiantes
                ├── profesores/                         # CRUD/listado de profesores
                ├── notas/                              # CRUD/listado de notas (reutiliza componentes)
                └── reportes/                           # Resumen de notas y exportación CSV
```

## 🚀 Requisitos (entorno limpio)

La app es 100% estática (no requiere build). Necesita únicamente un servidor HTTP simple para servir `index.html` y permitir que la app cargue `.env` por HTTP (no funciona con `file://`).

Elige una de estas opciones (todas multiplataforma):

- Opción A: Docker (sin instalar Node/Python)
- Opción B: Python 3 (http.server)
- Opción C: Node.js (http-server o serve)
- Opción D: VS Code Extension “Live Server”

### Opción A — Docker (recomendado si no tienes nada instalado)

1) Instala Docker Desktop (Windows/macOS) o Docker Engine (Linux):
     - https://docs.docker.com/get-docker/

2) Sirve el Frontend con Nginx:

```bash
cd Frontend
docker run --rm -p 8080:80 -v "$PWD":/usr/share/nginx/html:ro nginx:alpine
```

3) Abre: http://localhost:8080

Nota: Nginx sirve archivos que empiezan con punto (como `.env.development`), necesario para la carga de configuración.

### Opción B — Python 3

```bash
# Linux/macOS (Python 3 suele venir preinstalado)
cd Frontend
python3 -m http.server 8080

# Windows (si tienes Python instalado)
cd Frontend
py -3 -m http.server 8080
```

Abre: http://localhost:8080

### Opción C — Node.js

1) Instalar Node.js:

- Windows (winget): `winget install OpenJS.NodeJS.LTS`
- Windows (Chocolatey): `choco install nodejs-lts`
- macOS (Homebrew): `brew install node`
- Linux (Ubuntu/Debian): `sudo apt update && sudo apt install -y nodejs npm`

2) Instalar un servidor estático y levantarlo (elige uno):

```bash
# http-server
npm install -g http-server
cd Frontend
http-server -p 8080 --cors

# o con 'serve'
npm install -g serve
cd Frontend
serve -l 8080 --single
```

Abre: http://localhost:8080

Importante: asegúrate de que el servidor estático no bloquee dotfiles (archivos que empiezan por `.`). `http-server` los sirve por defecto; si usas otro, revisa su flag equivalente.

### Opción D — VS Code Live Server

1) Instala la extensión “Live Server”.
2) Click derecho en `index.html` → “Open with Live Server”.
3) Ajusta el puerto a 8080 si necesitas alinear con CORS del backend.

## ⚙️ Configuración por Entorno (.env)

El frontend carga configuración desde `.env.development` (en localhost/192.168.x) o `.env` (producción) vía `env-config-loader.service.js`.

Variables principales:

- `API_BASE_URL_DEVELOPMENT` (por defecto `http://localhost:5281/api/v1`)
- `API_BASE_URL_PRODUCTION`
- `API_TIMEOUT`, `API_RETRY_ATTEMPTS`, `PAGINATION_DEFAULT_PAGE_SIZE`, etc.

Ejemplo `.env.development`:

```
# API base del backend en desarrollo
API_BASE_URL_DEVELOPMENT=http://localhost:5281/api/v1

# UI/UX
UI_TOAST_DURATION=5000
UI_LOADING_DELAY=300

# Paginación
PAGINATION_DEFAULT_PAGE_SIZE=20
PAGINATION_MAX_PAGE_SIZE=100
```

Notas importantes:

- El backend debe permitir CORS para el origen del frontend (por ejemplo, `http://localhost:8080`). En el backend se configura en `Cors:AllowedOrigins` (ver README del backend).
- Si sirves el frontend en otro puerto u origen, agrega ese origen en `Cors:AllowedOrigins`.

## 🔐 Autenticación y Roles

- JWT almacenado en `localStorage` (`audisoft_token`) y datos de usuario en `audisoft_user`.
- `auth.interceptor.js` inyecta el Bearer token y gestiona 401/403 (incluye refresh y colas de reintentos).
- `acl.directive.js` aporta `has-role` y `has-any-role` para mostrar/ocultar elementos por rol.
- Protección de rutas en `app.routes.js` mediante `route-auth.service.js`.

Usuarios de prueba (según backend):

| Usuario       | Contraseña  | Rol        |
|---------------|-------------|------------|
| `admin`       | `Admin123@` | Admin      |
| `profesor1`   | `Prof123@`  | Profesor   |
| `estudiante1` | `Est123@`   | Estudiante |

## 🧩 Componentes Reutilizables y UX Global

- `as-table`: tabla configurable (columnas, sort, paginación, acciones slot).
- `as-modal`: modal parametrizable con body/footer transcluidos.
- `as-input`: inputs con validación AngularJS (required, minlength, maxlength, pattern, min, max) y feedback Bootstrap.
- Overlay de carga global + interceptor de loading.
- `toastService` integrado con interceptores y acciones de CRUD.

## 📊 Página de Reportes

- Ruta: `#!/reportes` (visible para usuarios autenticados; el servidor filtra por rol).
- Resumen: total de notas, promedio general, top por Profesor/Estudiante (top 10), distribución por rangos.
- Exportación: botón “Exportar CSV” descarga con filtros aplicados (rango de fechas `from/to`).

## 🏃‍♂️ Puesta en Marcha Rápida

1) Levanta el backend (ver README del Backend). Por defecto expone `http://localhost:5281/api/v1` y CORS para `http://localhost:8080`.
2) Sirve el frontend con una de las opciones A-D en el puerto 8080.
3) Abre `http://localhost:8080`, inicia sesión con un usuario de prueba.
4) Navega por Dashboard, Notas (CRUD), Estudiantes/Profesores y Reportes.

## 🔧 Solución de Problemas

- La app no carga `.env.development`:
    - Asegúrate de servir el directorio con un servidor HTTP que permita dotfiles.
    - Comprueba en la pestaña “Network” del navegador que `/.env.development` devuelve 200.

- 401/403 en llamadas:
    - Revisa que el login haya funcionado (existe `audisoft_token`).
    - Verifica que el backend esté corriendo y la hora del sistema sea correcta (exp del token).

- CORS bloquea solicitudes:
    - Agrega el origen del frontend en `Cors:AllowedOrigins` del backend y reinicia la API.

- Reportes vacíos:
    - Verifica que existan notas en el periodo; ajusta filtros `from/to`.

---

Desarrollado con enfoque modular y reutilizable, alineado a Clean Architecture del backend.