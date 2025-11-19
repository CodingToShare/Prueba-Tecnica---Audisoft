(function() {
    'use strict';

    /**
     * Routing configuration for AudiSoft School application
     * 
     * Defines all routes with their templates, controllers and access control
     * Implements route protection based on authentication and user roles
     */
    angular
        .module('audiSoftSchoolApp')
        .config(routeConfig);

    routeConfig.$inject = ['$routeProvider'];
    function routeConfig($routeProvider) {
        
        $routeProvider
            // ===== ROOT ROUTE =====
            // Redirect to login if not authenticated, else to dashboard
            .when('/', {
                templateUrl: 'app/features/login/login.html',
                controller: 'LoginController',
                controllerAs: 'login',
                access: {
                    requiresLogin: false,
                    allowedRoles: []
                }
            })
            
            // ===== PUBLIC ROUTES =====
            .when('/login', {
                templateUrl: 'app/features/login/login.html',
                controller: 'LoginController',
                controllerAs: 'login',
                access: {
                    requiresLogin: false,
                    allowedRoles: []
                }
            })

            // ===== PROTECTED ROUTES =====
            // Dashboard - All authenticated users
            .when('/dashboard', {
                templateUrl: 'app/features/dashboard/dashboard.html',
                controller: 'DashboardController',
                controllerAs: 'dashboard',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin', 'Profesor', 'Estudiante']
                }
            })

            // ===== ESTUDIANTES ROUTES =====
            // Lista de estudiantes - Admin y Profesor
            .when('/estudiantes', {
                templateUrl: 'app/features/estudiantes/estudiantes.html',
                controller: 'EstudiantesController',
                controllerAs: 'vm',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin', 'Profesor']
                }
            })

            // ===== PROFESORES ROUTES =====
            // Lista de profesores con CRUD inline - Solo Admin
            .when('/profesores', {
                templateUrl: 'app/features/profesores/profesores.html',
                controller: 'ProfesoresController',
                controllerAs: 'vm',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin']
                }
            })

            // ===== NOTAS ROUTES =====
            // Lista de notas con CRUD inline - Todos los roles autenticados
            .when('/notas', {
                templateUrl: 'app/features/notas/notas.html',
                controller: 'NotasController',
                controllerAs: 'vm',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin', 'Profesor', 'Estudiante']
                }
            })

            // ===== REPORTES ROUTES =====
            // Reportes de notas - Todos los roles autenticados (datos filtrados por servidor)
            .when('/reportes', {
                templateUrl: 'app/features/reportes/reportes.html',
                controller: 'ReportesController',
                controllerAs: 'vm',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin', 'Profesor', 'Estudiante']
                }
            })

            // ===== UTILITY ROUTES =====
            // Acceso denegado
            .when('/unauthorized', {
                templateUrl: 'app/shared/views/unauthorized.html',
                controller: 'UnauthorizedController',
                controllerAs: 'unauth',
                access: {
                    requiresLogin: false,
                    allowedRoles: []
                }
            })

            // Error 404
            .when('/not-found', {
                templateUrl: 'app/shared/views/not-found.html',
                controller: 'NotFoundController',
                controllerAs: 'vm',
                access: {
                    requiresLogin: false,
                    allowedRoles: []
                }
            })

            // Default route - robust check for valid JWT before redirecting
            .when('/', {
                redirectTo: function() {
                    var tokenKey = 'audisoft_token';
                    var token = localStorage.getItem(tokenKey);

                    function isValidJwt(t) {
                        if (!t || typeof t !== 'string' || t.split('.').length !== 3) return false;
                        try {
                            var payload = JSON.parse(atob(t.split('.')[1]));
                            if (!payload || !payload.exp) return false;
                            var nowMs = Date.now();
                            var expMs = payload.exp * 1000;
                            var bufferMs = 300000; // 5 min buffer
                            return (expMs - bufferMs) > nowMs;
                        } catch (e) {
                            return false;
                        }
                    }

                    if (isValidJwt(token)) {
                        return '/dashboard';
                    }

                    // Clear any stale session keys to avoid flicker
                    try {
                        localStorage.removeItem(tokenKey);
                        localStorage.removeItem('audisoft_user');
                        localStorage.removeItem('audisoft_refresh_token');
                    } catch (e) {}

                    return '/login';
                }
            })

            // Fallback route
            .otherwise({
                redirectTo: '/not-found'
            });
    }

})();