const express = require('express');
const path = require('path');
const fs = require('fs');
require('dotenv').config();

const PORT = process.env.PORT || 8080;
const app = express();

console.log(`
╔══════════════════════════════════════════╗
║ 🚀 AudiSoft Frontend Server (Starting)   ║
╠══════════════════════════════════════════╣
║ Port:     ${PORT.toString().padEnd(34)}║
║ API:      ${(process.env.API_BASE_URL_PRODUCTION || process.env.API_BASE_URL_DEVELOPMENT || 'http://localhost:5281/api/v1').substring(0, 34).padEnd(34)}║
║ Mode:     ${(process.env.DEBUG_MODE === 'true' ? 'Development' : 'Production').padEnd(34)}║
║ CWD:      ${__dirname.substring(0, 34).padEnd(34)}║
╚══════════════════════════════════════════╝
`);

// ===========================
// 1. API Endpoints (Específicos)
// ===========================

app.get('/health', (req, res) => {
  res.status(200).json({ 
    status: 'ok', 
    timestamp: new Date().toISOString(),
    app: 'AudiSoft School Frontend',
    node: process.version
  });
});

app.get('/api/config', (req, res) => {
  const config = {
    apiBaseUrl: process.env.API_BASE_URL_PRODUCTION || process.env.API_BASE_URL_DEVELOPMENT || 'http://localhost:5281/api/v1',
    debugMode: process.env.DEBUG_MODE === 'true',
    version: '1.0.0',
    environment: process.env.NODE_ENV || 'production'
  };
  res.setHeader('Content-Type', 'application/json; charset=utf-8');
  res.json(config);
});

app.get('/.env', (req, res) => {
  res.setHeader('Content-Type', 'text/plain; charset=utf-8');
  res.send(`API_BASE_URL_PRODUCTION=${process.env.API_BASE_URL_PRODUCTION || 'https://app-audisoft-api.azurewebsites.net/api/v1'}
DEBUG_MODE=${process.env.DEBUG_MODE || 'false'}`);
});

app.get('/debug/config', (req, res) => {
  res.status(200).json({ 
    apiBase: process.env.API_BASE_URL_PRODUCTION || process.env.API_BASE_URL_DEVELOPMENT || 'NOT SET',
    debugMode: process.env.DEBUG_MODE,
    port: PORT,
    cwd: __dirname,
    nodeVersion: process.version,
    env: process.env.NODE_ENV || 'production'
  });
});

// ===========================
// 2. Static Files (ANTES de catch-all)
// ===========================

// Servir archivos estáticos con MIME types explícitos
app.use((req, res, next) => {
  // Extensiones específicas que NO queremos que vayan a index.html
  const staticExtensions = ['.js', '.css', '.json', '.png', '.jpg', '.jpeg', '.gif', '.svg', '.ico', '.woff', '.woff2', '.ttf', '.eot', '.map'];
  
  if (staticExtensions.some(ext => req.url.endsWith(ext))) {
    // Es un archivo estático - permitir que Express lo sirva
    const filePath = path.join(__dirname, req.url);
    
    // Verificar que el archivo existe
    if (fs.existsSync(filePath)) {
      return next(); // Dejar que continue
    }
  }
  
  next();
});

// Middleware para MIME types y Cache Control optimizado
app.use((req, res, next) => {
  // Para archivos con query string (versioning), cachear agresivamente
  const hasVersionParam = /[?&]v=\d+/.test(req.url);
  
  if (req.url.endsWith('.js')) {
    res.setHeader('Content-Type', 'application/javascript; charset=utf-8');
    // Con versioning: cache por 1 año. Sin versioning: no cachear
    res.setHeader('Cache-Control', hasVersionParam ? 'public, max-age=31536000, immutable' : 'public, max-age=0, must-revalidate');
  } else if (req.url.endsWith('.css')) {
    res.setHeader('Content-Type', 'text/css; charset=utf-8');
    // Con versioning: cache por 1 año. Sin versioning: no cachear
    res.setHeader('Cache-Control', hasVersionParam ? 'public, max-age=31536000, immutable' : 'public, max-age=0, must-revalidate');
  } else if (req.url.endsWith('.html')) {
    res.setHeader('Content-Type', 'text/html; charset=utf-8');
    // HTML siempre: no cachear para evitar problemas con SPA
    res.setHeader('Cache-Control', 'public, max-age=0, must-revalidate');
  }
  next();
});

// Servir archivos estáticos desde raíz
app.use(express.static(path.join(__dirname), {
  etag: false,
  lastModified: true,
  maxAge: '1d'
}));

// ===========================
// 3. SPA Fallback (Catch-all ÚLTIMO)
// ===========================

// Ruta catch-all para SPA - solo para rutas de navegación (NO para archivos)
app.get('*', (req, res, next) => {
  // NO redirigir a index.html si es una extensión de archivo
  if (path.extname(req.url)) {
    return res.status(404).send('Not Found');
  }
  
  // Redirigir a index.html para SPA
  const indexPath = path.join(__dirname, 'index.html');
  if (fs.existsSync(indexPath)) {
    res.setHeader('Content-Type', 'text/html; charset=utf-8');
    res.sendFile(indexPath);
  } else {
    res.status(404).send('index.html not found');
  }
});

// ===========================
// 4. Error Handling
// ===========================

app.use((err, req, res, next) => {
  console.error('Error:', err);
  res.status(500).json({ 
    status: 'error',
    message: 'Internal Server Error',
    error: process.env.DEBUG_MODE === 'true' ? err.message : undefined
  });
});

// ===========================
// 5. Start Server
// ===========================

app.listen(PORT, () => {
  console.log(`
╔══════════════════════════════════════════╗
║ ✅ AudiSoft Frontend Running              ║
╠══════════════════════════════════════════╣
║ Port:     ${PORT.toString().padEnd(34)}║
║ URL:      http://localhost:${PORT.toString().padEnd(25)}║
║ API:      ${(process.env.API_BASE_URL_PRODUCTION || 'http://localhost:5281/api/v1').substring(0, 34).padEnd(34)}║
║ Mode:     ${(process.env.DEBUG_MODE === 'true' ? 'Development' : 'Production').padEnd(34)}║
╚══════════════════════════════════════════╝
  `);
});
