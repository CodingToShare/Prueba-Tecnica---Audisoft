# 🔧 Solución: Frontend en Azure - Errores de JavaScript

## Problema

El frontend en Azure muestra errores:
```
Uncaught SyntaxError: Unexpected token '<' (at app.module.js:1:1)
Uncaught Error: [$injector:nomod] Module 'audiSoftSchoolApp' not found
```

**Causa**: Los archivos `.js` están siendo servidos como HTML (`<!DOCTYPE>...`) en lugar de JavaScript.

---

## ✅ Soluciones Implementadas

### 1. **Servidor Node.js Corregido** (`server.js`)

✅ **Lo que se arregló**:
- Cambió la ruta estática de `public/` a raíz de Frontend (donde está `index.html`)
- Agregó MIME types explícitos para `.js`, `.css`, `.html`
- Agregó endpoints `/api/config` y `/.env` para configuración
- Agregó mejor logging y debugging

### 2. **Configuración de Entorno** 

Creado archivo `.env.production`:
```env
API_BASE_URL_PRODUCTION=https://app-audisoft-api.azurewebsites.net/api/v1
DEBUG_MODE=false
PORT=8080
```

### 3. **Cargador de Configuración Mejorado** 

Actualizado `env-config-loader.service.js` con prioridades:
1. **Primero**: `GET /api/config` (recomendado para Azure)
2. **Segundo**: `.env.development` o `.env.production`
3. **Tercero**: `.env` (fallback)
4. **Último**: Configuración por defecto

---

## 🚀 Acciones Requeridas en Azure Portal

### Paso 1: Configurar Variables de Entorno

1. Portal.azure.com → **App Services** → `app-audisoft-web`
2. Ir a **Configuration** → **Application settings**
3. Agregar/Actualizar las siguientes variables:

| Key | Value | Notes |
|-----|-------|-------|
| `API_BASE_URL_PRODUCTION` | `https://app-audisoft-api.azurewebsites.net/api/v1` | URL del backend |
| `NODE_ENV` | `production` | Modo producción |
| `DEBUG_MODE` | `false` | Desactivar debug |
| `PORT` | `8080` | Puerto por defecto |

4. **Click "Save"** (reinicia la app automáticamente)

### Paso 2: Verificar la Salida de Inicio (Startup Output)

1. App Services → `app-audisoft-web`
2. Ir a **Log stream** (tiempo real)
3. Debería ver:
```
╔══════════════════════════════════════════╗
║ 🚀 AudiSoft Frontend Server              ║
╠══════════════════════════════════════════╣
║ Port:     8080                           ║
║ API:      https://app-audisoft-api...   ║
║ Mode:     Production                     ║
║ CWD:      /home/site/wwwroot            ║
╚══════════════════════════════════════════╝
```

### Paso 3: Verificar Endpoints de Diagnóstico

Abre en tu navegador:

1. **Health Check**:
   ```
   https://app-audisoft-web.azurewebsites.net/health
   ```
   Deberías ver:
   ```json
   {
     "status": "ok",
     "app": "AudiSoft School Frontend",
     "node": "v20.x.x"
   }
   ```

2. **Configuración (Debug)**:
   ```
   https://app-audisoft-web.azurewebsites.net/debug/config
   ```
   Deberías ver:
   ```json
   {
     "apiBase": "https://app-audisoft-api.azurewebsites.net/api/v1",
     "debugMode": false,
     "port": 8080,
     "cwd": "/home/site/wwwroot"
   }
   ```

3. **Configuración (API)**:
   ```
   https://app-audisoft-web.azurewebsites.net/api/config
   ```
   Deberías ver:
   ```json
   {
     "apiBaseUrl": "https://app-audisoft-api.azurewebsites.net/api/v1",
     "debugMode": false,
     "version": "1.0.0",
     "environment": "production"
   }
   ```

### Paso 4: Probar Acceso a Archivos Estáticos

1. **JavaScript**:
   ```
   https://app-audisoft-web.azurewebsites.net/app/app.module.js
   ```
   Header `Content-Type` debe ser: `application/javascript; charset=utf-8`

2. **CSS**:
   ```
   https://app-audisoft-web.azurewebsites.net/assets/css/app.css
   ```
   Header `Content-Type` debe ser: `text/css; charset=utf-8`

---

## 🔍 Verificación Completa

### En la consola del navegador:

1. **Cargar aplicación**: `https://app-audisoft-web.azurewebsites.net`
2. Abrir **DevTools** (F12)
3. Ir a la pestaña **Console**
4. Deberías NO ver errores rojos de sintaxis
5. Deberías ver mensaje similar a:
   ```
   EnvConfigLoader: Configuration loaded from /api/config
   ```

### En Network tab:

1. Filtrar por `app.module.js`
2. Click para ver detalles
3. **Response** debe mostrar código JavaScript (no HTML)
4. **Headers** → **Content-Type** debe ser `application/javascript`

---

## 📋 Checklist de Solución

- [ ] Variables de entorno agregadas en Azure Portal
- [ ] Log stream muestra mensaje de inicio correcto
- [ ] `/health` retorna JSON válido
- [ ] `/debug/config` muestra configuración correcta
- [ ] `/api/config` retorna JSON con apiBaseUrl
- [ ] `app.module.js` tiene Content-Type: application/javascript
- [ ] Console no muestra errores de sintaxis
- [ ] Página carga sin errores AngularJS
- [ ] Login funciona
- [ ] Dashboard se carga

---

## 🆘 Si Aún No Funciona

### Opción A: Reiniciar la App

```bash
# En Azure Portal
App Services → app-audisoft-web → Restart
```

### Opción B: Verificar Logs Detallados

```bash
# Conectar via SSH/Kudu Console
https://app-audisoft-web.scm.azurewebsites.net/debug/cmd
cd /home/site/wwwroot
ls -la  # Ver estructura de archivos
cat package.json  # Verificar dependencies
npm list  # Ver paquetes instalados
```

### Opción C: Verificar Process

```bash
# Ver si Node.js está corriendo
ps aux | grep node
```

### Opción D: Limpiar y Reinstalar

```bash
# En Azure Kudu Console
cd /home/site/wwwroot
rm -rf node_modules
npm install
npm start
```

---

## 📞 Contacto / Debugging Remoto

Si necesitas ver logs en tiempo real:

```bash
# SSH a la app (si está habilitado)
ssh username@app-audisoft-web.azurewebsites.net

# Ver logs en vivo
tail -f /var/log/syslog | grep node
```

---

**Última actualización**: 14 de noviembre, 2025
