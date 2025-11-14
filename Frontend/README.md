# Frontend - AudiSoft School

Esta carpeta contiene la aplicación frontend desarrollada en **AngularJS 1.x** que consume la API REST del backend .NET 8.

## 📁 Estructura del Proyecto

```
Frontend/
├── index.html                    # Punto de entrada principal
├── app.module.js                # Módulo raíz de AngularJS
├── app.routes.js                # Configuración de rutas
├── assets/                      # Recursos estáticos
│   ├── css/
│   │   └── app.css              # Estilos personalizados
│   ├── js/
│   │   └── libs/                # Librerías externas
│   └── images/                  # Recursos gráficos
└── app/                         # Código de la aplicación
    ├── core/                    # Servicios core del sistema
    │   ├── auth/               # Servicios de autenticación
    │   ├── interceptors/       # HTTP interceptors
    │   └── services/           # Servicios base (API, etc.)
    ├── shared/                 # Componentes reutilizables
    │   ├── components/         # Componentes compartidos
    │   ├── directives/         # Directivas personalizadas
    │   └── utils/              # Utilidades y helpers
    └── features/               # Funcionalidades por módulo
        ├── login/              # Módulo de autenticación
        ├── estudiantes/        # Gestión de estudiantes
        ├── profesores/         # Gestión de profesores
        └── notas/              # Gestión de notas
```

## 🚀 Tecnologías

- **AngularJS 1.8.3** - Framework principal
- **Bootstrap 5.3.2** - Framework CSS
- **Bootstrap Icons** - Iconografía
- **Angular Route** - Enrutamiento SPA
- **Angular Resource** - Consumo de APIs REST

## 📋 Estado Actual

### ✅ Completado (META 1)
- [x] Estructura de carpetas por features
- [x] Configuración base de AngularJS
- [x] Integración con Bootstrap 5
- [x] Layout principal con navegación
- [x] Estilos personalizados
- [x] Configuración HTML5 para SEO

### 🔄 Pendiente
- [ ] Configuración de rutas (META 2)
- [ ] Servicios de API genéricos (META 3)
- [ ] Sistema de autenticación (META 4-5)
- [ ] Pantallas de funcionalidades (META 6-9)
- [ ] Componentes reutilizables (META 11-13)

## 🎯 Próximos Pasos

1. **META 2**: Configurar sistema de rutas
2. **META 3**: Crear servicios para consumo de API
3. **META 4**: Implementar autenticación JWT
4. **META 5**: Configurar interceptors HTTP

---

**Desarrollado siguiendo principios SOLID y Clean Architecture**