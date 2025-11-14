const express = require('express');
const path = require('path');
require('dotenv').config();

const app = express();

// MIME types para archivos JS y CSS
app.use((req, res, next) => {
  if (req.url.endsWith('.js')) {
    res.setHeader('Content-Type', 'application/javascript');
  } else if (req.url.endsWith('.css')) {
    res.setHeader('Content-Type', 'text/css');
  }
  next();
});

// Middleware para servir archivos estáticos desde raíz de Frontend
app.use(express.static(path.join(__dirname), {
  maxAge: '1d',
  etag: false,
  setHeaders: (res, path, stat) => {
    if (path.endsWith('.js')) {
      res.setHeader('Content-Type', 'application/javascript; charset=utf-8');
    } else if (path.endsWith('.css')) {
      res.setHeader('Content-Type', 'text/css; charset=utf-8');
    } else if (path.endsWith('.html')) {
      res.setHeader('Content-Type', 'text/html; charset=utf-8');
    }
  }
}));

// API Endpoint: Serve environment configuration as JSON
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

// Serve .env file (for backward compatibility, but redirect to /api/config)
app.get('/.env', (req, res) => {
  res.setHeader('Content-Type', 'text/plain; charset=utf-8');
  res.send(`API_BASE_URL_PRODUCTION=${process.env.API_BASE_URL_PRODUCTION || 'https://app-audisoft-api.azurewebsites.net/api/v1'}
DEBUG_MODE=${process.env.DEBUG_MODE || 'false'}`);
});

// Health check endpoint (no cacheado)
app.get('/health', (req, res) => {
  res.status(200).json({ 
    status: 'ok', 
    timestamp: new Date().toISOString(),
    app: 'AudiSoft School Frontend'
  });
});

// Health check endpoint (sin cache)
app.get('/health', (req, res) => {
  res.status(200).json({ 
    status: 'ok', 
    timestamp: new Date().toISOString(),
    app: 'AudiSoft School Frontend',
    node: process.version
  });
});

// Debug endpoint para verificar rutas
app.get('/debug/config', (req, res) => {
  res.status(200).json({ 
    apiBase: process.env.API_BASE_URL_PRODUCTION || process.env.API_BASE_URL_DEVELOPMENT || 'NOT SET',
    debugMode: process.env.DEBUG_MODE,
    port: PORT,
    cwd: __dirname
  });
});

// Ruta catch-all para SPA (redirigir a index.html)
// Esta DEBE ser la última ruta
app.get('*', (req, res) => {
  const indexPath = path.join(__dirname, 'index.html');
  res.sendFile(indexPath);
});

// Error handling
app.use((err, req, res, next) => {
  console.error('Error:', err);
  res.status(500).json({ 
    status: 'error',
    message: 'Internal Server Error',
    error: process.env.DEBUG_MODE === 'true' ? err.message : undefined
  });
});

const PORT = process.env.PORT || 8080;
app.listen(PORT, () => {
  console.log(`
╔══════════════════════════════════════════╗
║ 🚀 AudiSoft Frontend Server              ║
╠══════════════════════════════════════════╣
║ Port:     ${PORT.toString().padEnd(34)}║
║ API:      ${(process.env.API_BASE_URL_PRODUCTION || process.env.API_BASE_URL_DEVELOPMENT || 'http://localhost:5281/api/v1').substring(0, 34).padEnd(34)}║
║ Mode:     ${(process.env.DEBUG_MODE === 'true' ? 'Development' : 'Production').padEnd(34)}║
║ CWD:      ${__dirname.substring(0, 34).padEnd(34)}║
╚══════════════════════════════════════════╝
  `);
});
