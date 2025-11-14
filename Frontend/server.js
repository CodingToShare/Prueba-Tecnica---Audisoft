const express = require('express');
const path = require('path');
require('dotenv').config();

const app = express();

// Middleware para servir archivos estáticos
app.use(express.static(path.join(__dirname, 'public'), {
  maxAge: '1d',
  etag: false
}));

// Health check endpoint (no cacheado)
app.get('/health', (req, res) => {
  res.status(200).json({ 
    status: 'ok', 
    timestamp: new Date().toISOString(),
    app: 'AudiSoft School Frontend'
  });
});

// Ruta catch-all para SPA (redirigir a index.html)
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// Error handling
app.use((err, req, res, next) => {
  console.error('Error:', err);
  res.status(500).json({ 
    status: 'error',
    message: 'Internal Server Error'
  });
});

const PORT = process.env.PORT || 8080;
app.listen(PORT, () => {
  console.log(`🚀 AudiSoft Frontend running on port ${PORT}`);
  console.log(`📡 API: ${process.env.API_BASE_URL_PRODUCTION || 'http://localhost:5281/api/v1'}`);
  console.log(`🌍 Environment: ${process.env.DEBUG_MODE === 'true' ? 'Development' : 'Production'}`);
});
