(function() {
    'use strict';

    /**
     * Dashboard Controller
     * Manages the main dashboard view with statistics and quick actions
     */
    angular
        .module('audiSoftSchoolApp')
        .controller('DashboardController', DashboardController);

    DashboardController.$inject = ['$q', 'routeAuthService', 'apiService'];
    function DashboardController($q, routeAuthService, apiService) {
        var vm = this;

        // Bindable properties
        vm.stats = {};
        vm.recentActivity = [];
        vm.lastLogin = null;
        vm.latestNotas = [];
        

        // Bindable methods
        vm.canViewStudents = canViewStudents;
        vm.canViewProfessors = canViewProfessors;
        vm.canCreateStudent = canCreateStudent;
        vm.canCreateProfessor = canCreateProfessor;
        vm.canCreateNote = canCreateNote;
        vm.isEstudiante = isEstudiante;
        vm.isProfesor = isProfesor;
        vm.isAdmin = isAdmin;

        // Initialize controller
        activate();

        ////////////////

        function activate() {
            loadDashboardData();
            loadRecentActivity();
            loadLatestNotas();
        }

        function loadDashboardData() {
            vm.lastLogin = new Date();

            var paramsCount = { page: 1, pageSize: 1, maxPageSize: 1, sortField: 'id', sortDesc: true };
            var paramsNotas = { page: 1, pageSize: 100, maxPageSize: 100, sortField: 'id', sortDesc: true };

            var notasCountP = apiService.get('Notas', paramsCount)
                .then(function(res) { return res.totalCount || (res.data && res.data.totalCount) || 0; })
                .catch(function() { return 0; });

            var estudiantesCountP = apiService.get('estudiantes', paramsCount)
                .then(function(res) { return res.totalCount || (res.data && res.data.totalCount) || 0; })
                .catch(function() { return 0; });

            var profesoresCountP = apiService.get('profesores', paramsCount)
                .then(function(res) { return res.totalCount || (res.data && res.data.totalCount) || 0; })
                .catch(function() { return 0; });

            var notasAvgP = apiService.get('Notas', paramsNotas)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    if (!items.length) return null;
                    var sum = 0;
                    for (var i = 0; i < items.length; i++) { sum += (items[i].valor || items[i].Valor || 0); }
                    return +(sum / items.length).toFixed(1);
                })
                .catch(function() { return null; });

            $q.all([notasCountP, estudiantesCountP, profesoresCountP, notasAvgP]).then(function(results) {
                vm.stats = {
                    totalNotas: results[0],
                    totalEstudiantes: results[1],
                    totalProfesores: results[2],
                    promedioGeneral: results[3]
                };
            });
        }

        function loadRecentActivity() {
            vm.recentActivity = [];
            
            // Load latest notas
            var notasParams = { page: 1, pageSize: 10, maxPageSize: 10, sortField: 'CreatedAt', sortDesc: true };
            var notasPromise = apiService.get('Notas', notasParams)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    return items.map(function(n) {
                        return {
                            title: 'Nueva nota registrada',
                            description: (n.nombre || n.Nombre || '') + ': ' + (n.valor || n.Valor || 0) + ' puntos',
                            date: new Date(n.createdAt || n.CreatedAt),
                            type: 'nota'
                        };
                    });
                })
                .catch(function() { return []; });

            // Load latest estudiantes
            var estudiantesParams = { page: 1, pageSize: 10, maxPageSize: 10, sortField: 'CreatedAt', sortDesc: true };
            var estudiantesPromise = apiService.get('estudiantes', estudiantesParams)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    return items.map(function(e) {
                        return {
                            title: 'Estudiante creado',
                            description: (e.nombre || e.Nombre || '') + ' agregado al sistema',
                            date: new Date(e.createdAt || e.CreatedAt),
                            type: 'estudiante'
                        };
                    });
                })
                .catch(function() { return []; });

            // Load latest profesores
            var profesoresParams = { page: 1, pageSize: 10, maxPageSize: 10, sortField: 'CreatedAt', sortDesc: true };
            var profesoresPromise = apiService.get('profesores', profesoresParams)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    return items.map(function(p) {
                        return {
                            title: 'Profesor creado',
                            description: (p.nombre || p.Nombre || '') + ' agregado al sistema',
                            date: new Date(p.createdAt || p.CreatedAt),
                            type: 'profesor'
                        };
                    });
                })
                .catch(function() { return []; });

            // Combine all activities and sort by date (newest first)
            $q.all([notasPromise, estudiantesPromise, profesoresPromise])
                .then(function(results) {
                    var allActivities = [];
                    allActivities = allActivities.concat(results[0]); // notas
                    allActivities = allActivities.concat(results[1]); // estudiantes
                    allActivities = allActivities.concat(results[2]); // profesores
                    
                    // Sort by date descending (newest first)
                    allActivities.sort(function(a, b) {
                        return new Date(b.date) - new Date(a.date);
                    });
                    
                    // Keep only the 10 most recent
                    vm.recentActivity = allActivities.slice(0, 10);
                });
        }

        function loadLatestNotas() {
            var params = { page: 1, pageSize: 5, maxPageSize: 5, sortField: 'CreatedAt', sortDesc: true };
            apiService.get('Notas', params)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    vm.latestNotas = items.map(function(n) {
                        return {
                            id: n.id || n.Id,
                            nombre: n.nombre || n.Nombre,
                            valor: n.valor || n.Valor,
                            createdAt: n.createdAt || n.CreatedAt
                        };
                    });
                })
                .catch(function() {
                    vm.latestNotas = [];
                });
        }

        // Permission methods
        function canViewStudents() {
            return routeAuthService.hasRole('Admin') || routeAuthService.hasRole('Profesor');
        }

        function canViewProfessors() {
            return routeAuthService.hasRole('Admin');
        }

        function canCreateStudent() {
            return routeAuthService.hasRole('Admin');
        }

        function canCreateProfessor() {
            return routeAuthService.hasRole('Admin');
        }

        function canCreateNote() {
            return routeAuthService.hasRole('Admin') || routeAuthService.hasRole('Profesor');
        }

        function isEstudiante() {
            return routeAuthService.hasRole('Estudiante');
        }

        function isProfesor() {
            return routeAuthService.hasRole('Profesor');
        }

        function isAdmin() {
            return routeAuthService.hasRole('Admin');
        }
    }

})();