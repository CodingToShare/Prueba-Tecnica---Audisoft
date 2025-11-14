# ⚡ ACCIONES URGENTES - Azure Frontend Fix

## 🔴 Problema Actual

Frontend en Azure no carga: los archivos `.js` se sirven como HTML.

---

## ✅ Lo Que Se Hizo (Código)

1. ✅ Actualizado `Frontend/server.js`:
   - Corregida ruta estática (de `public/` a raíz)
   - Agregados MIME types correctos
   - Nuevo endpoint `/api/config`
   - Mejor logging

2. ✅ Actualizado `Frontend/app/core/services/env-config-loader.service.js`:
   - Ahora intenta `/api/config` primero
   - Luego fallback a `.env` files
   - Mucha mejor manejo de errores

3. ✅ Creado `Frontend/.env.production`:
   - Variables correctas para Azure

---

## 🚀 QUÉ DEBES HACER EN AZURE PORTAL

### 1️⃣ Ir a App Service `app-audisoft-web`

```
portal.azure.com 
→ App Services 
→ app-audisoft-web
```

### 2️⃣ Configurar Variables de Entorno

Click en: **Settings** → **Configuration** → **Application settings**

**Agregar estas 4 variables** (click + New application setting):

| Variable | Valor |
|----------|-------|
| `API_BASE_URL_PRODUCTION` | `https://app-audisoft-api.azurewebsites.net/api/v1` |
| `NODE_ENV` | `production` |
| `DEBUG_MODE` | `false` |
| `PORT` | `8080` |

Después de cada variable, click en el `+` verde.

### 3️⃣ Guardar Cambios

Click en: **Save** (arriba)

⏳ Espera 30-60 segundos a que se reinicie la app.

---

## ✔️ Verificar que Funciona

### Test 1: Health Check

Abre en navegador:
```
https://app-audisoft-web.azurewebsites.net/health
```

Deberías ver JSON (no error):
```json
{
  "status": "ok",
  "app": "AudiSoft School Frontend",
  "node": "v20.x.x"
}
```

### Test 2: Configuración

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

### Test 3: Archivo JS

```
https://app-audisoft-web.azurewebsites.net/app/app.module.js
```

✅ Debe mostrar código JavaScript
✅ En DevTools (F12) → Network → ver Content-Type = `application/javascript`

### Test 4: Página Principal

```
https://app-audisoft-web.azurewebsites.net
```

✅ Debe cargar la aplicación
✅ Console (F12) NO debe tener errores rojos

---

## 📋 Si Esto No Funciona

### Opción A: Reiniciar la App

En Azure Portal:
- App Services → app-audisoft-web → **Restart** (botón arriba)

### Opción B: Verificar Logs

En Azure Portal:
- App Services → app-audisoft-web → **Log stream**
- Deberías ver el mensaje de inicio del servidor

### Opción C: Verificar el Deployment

En Azure Portal:
- App Services → app-audisoft-web → **Deployment slots**
- O Ir a: **GitHub Actions** y ver si el workflow `deploy-frontend.yml` completó correctamente

---

## 📞 Resumen

**En 3 pasos**:
1. Abrir App Service `app-audisoft-web`
2. Configuration → Application settings
3. Agregar 4 variables y guardar

**En 3 minutos deberá funcionar** ✅

**Documentación completa**: Ver `AZURE_FRONTEND_FIX.md`
