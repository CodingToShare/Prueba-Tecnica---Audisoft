# AI Coding Agent Guide

This repo is a pure frontend SPA built with AngularJS 1.x (no build step). Use these conventions to be productive immediately.

## Big Picture
- Framework: AngularJS 1.8.x, single module `audiSoftSchoolApp`.
- Entry + scripts: `index.html` wires everything. Angular routes in `app.routes.js`. Core services live under `app/core/**`. Feature controllers/templates live under `app/features/**`.
- Env/config: `app/core/services/env-config-loader.service.js` loads `.env.development` (localhost) or `.env` via XHR; `app/core/services/config.service.js` reads these keys to compute `api.baseUrl`, timeouts, and auth storage keys.
- Auth: The effective `authService` is defined in `app/core/auth/auth.service.js` (it overrides the older one in `app/core/services/auth.service.js`). The interceptor `app/core/interceptors/auth.interceptor.js` injects `Bearer` tokens and auto-refreshes on 401.
- Routing/guards: `app.routes.js` uses `ngRoute` and each route can declare `access: { requiresLogin, allowedRoles }`. Guards are enforced in `run()` and via `app/core/services/route-auth.service.js`.

## Run/Debug
- Static server is required (env files are fetched via `$http.get`). From the project root:
  - PowerShell (recommended):
    ```powershell
    npm install -g http-server
    http-server -p 8080
    ```
  - Or use the VS Code "Live Server" extension.
- Local dev URL: `http://localhost:8080` (triggers `.env.development`).
- Create an env file if missing (project root):
  ```env
  # .env.development
  API_BASE_URL_DEVELOPMENT=http://localhost:5281/api/v1
  API_TIMEOUT=30000
  AUTH_TOKEN_KEY=audisoft_token
  AUTH_USER_KEY=audisoft_user
  AUTH_REFRESH_TOKEN_KEY=audisoft_refresh_token
  ```

## Key Patterns
- Controllers use controllerAs and `vm` pattern. Example: `app/features/dashboard/dashboard.controller.js`.
- Add routes with an `access` object. Example (from `app.routes.js`):
  ```js
  .when('/dashboard', {
    templateUrl: 'app/features/dashboard/dashboard.html',
    controller: 'DashboardController',
    controllerAs: 'dashboard',
    access: { requiresLogin: true, allowedRoles: ['Admin','Profesor','Estudiante'] }
  })
  ```
- Use `apiService` for HTTP. It builds URLs from `configService.getApiConfig().baseUrl`, returns `{ data, status, headers, totalCount? }`, and normalizes errors:
  ```js
  apiService.get('estudiantes', { page: 1 })
    .then(res => { /* res.data, res.totalCount */ })
    .catch(err => { /* err.status, err.message */ });
  ```
- Auth service (use the version in `app/core/auth/auth.service.js`):
  ```js
  authService.login({ email, password })
    .then(user => { /* authenticated */ })
    .catch(err => { /* err.message, err.status */ });

  authService.isAuthenticated();
  authService.getCurrentUser();
  authService.refreshAccessToken();
  ```
  Notes: storage keys come from `configService.getAuthConfig()`; session persists in `localStorage`.
- Interceptor is auto-registered and adds `Authorization: Bearer <token>` when calling API URLs; it queues and retries requests during token refresh and redirects to login on failure.
- Route auth flow: In `app.module.js` `run()`, `$routeChangeStart` checks `next.access` via `routeAuthService`. On deny, it redirects to `#/login` or `#/unauthorized`.

## Project Conventions
- Feature-first layout: `app/features/<feature>/<feature>*.{controller.js,html}`.
- Shared views under `app/shared/views` (e.g., `unauthorized.html`, `not-found.html`).
- Always add new controller scripts to `index.html` in the correct order after core services.
- Prefer `apiService` over raw `$http`. Do not manually set `Authorization` headers; the interceptor handles it.
- Be aware of duplicate `authService` files; the one under `app/core/auth/` is authoritative. Use its method signatures (credentials object), not the older string-based example in `DashboardController`.

## Current State
- Implemented: base module, routing skeleton, env/config loader, API wrapper, auth interceptor, dashboard scaffold, shared 401/404 views.
- Pending: feature modules for `estudiantes`, `profesores`, `notas` (routes exist but templates/controllers may be missing), full auth screens.

## Extending Quickly
- Add a feature:
  - Create `app/features/foo/foo.controller.js` and `app/features/foo/foo.html`.
  - Register a route in `app.routes.js` with `access` as needed.
  - Add the script tag in `index.html`.
  - Use `apiService` with endpoints relative to `configService.getApiBaseUrl()`.

If anything above is unclear (e.g., run steps on your machine, env files, or which auth service to target), tell me what you want to add or run and I’ll refine these notes.