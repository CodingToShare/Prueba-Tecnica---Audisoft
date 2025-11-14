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
            // Lista de profesores - Solo Admin
            .when('/profesores', {
                templateUrl: 'app/features/profesores/profesores-list.html',
                controller: 'ProfesoresListController',
                controllerAs: 'profesoresList',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin']
                }
            })
            
            // Crear profesor - Solo Admin
            .when('/profesores/new', {
                templateUrl: 'app/features/profesores/profesor-form.html',
                controller: 'ProfesorFormController',
                controllerAs: 'profesorForm',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin']
                }
            })
            
            // Ver/Editar profesor - Solo Admin
            .when('/profesores/:id', {
                templateUrl: 'app/features/profesores/profesor-detail.html',
                controller: 'ProfesorDetailController',
                controllerAs: 'profesorDetail',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin']
                }
            })

            // ===== NOTAS ROUTES =====
            // Lista de notas - Filtrado por rol
            .when('/notas', {
                templateUrl: 'app/features/notas/notas-list.html',
                controller: 'NotasListController',
                controllerAs: 'notasList',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin', 'Profesor', 'Estudiante']
                }
            })
            
            // Crear nota - Admin y Profesor
            .when('/notas/new', {
                templateUrl: 'app/features/notas/nota-form.html',
                controller: 'NotaFormController',
                controllerAs: 'notaForm',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin', 'Profesor']
                }
            })
            
            // Ver/Editar nota - Permisos por rol y ownership
            .when('/notas/:id', {
                templateUrl: 'app/features/notas/nota-detail.html',
                controller: 'NotaDetailController',
                controllerAs: 'notaDetail',
                access: {
                    requiresLogin: true,
                    allowedRoles: ['Admin', 'Profesor', 'Estudiante']
                }
            })

            // ===== UTILITY ROUTES =====
            // Acceso denegado
            .when('/unauthorized', {
                templateUrl: 'app/shared/views/unauthorized.html',
                access: {
                    requiresLogin: false,
                    allowedRoles: []
                }
            })

            // Error 404
            .when('/not-found', {
                templateUrl: 'app/shared/views/not-found.html',
                access: {
                    requiresLogin: false,
                    allowedRoles: []
                }
            })

            // Default route - redirect to dashboard or login
            .when('/', {
                redirectTo: function(routeParams, path, search) {
                    // Check if user is authenticated
                    var token = localStorage.getItem('audisoft_token');
                    return token ? '/dashboard' : '/login';
                }
            })

            // Fallback route
            .otherwise({
                redirectTo: '/not-found'
            });
    }

})();