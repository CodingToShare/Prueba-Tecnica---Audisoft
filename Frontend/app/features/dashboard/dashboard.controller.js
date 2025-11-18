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
            
            // Load latest notas (created, updated, or deleted)
            var notasParams = { page: 1, pageSize: 10, maxPageSize: 10, sortField: 'UpdatedAt', sortDesc: true };
            var notasPromise = apiService.get('Notas', notasParams)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    return items.map(function(n) {
                        var createdAt = new Date(n.createdAt || n.CreatedAt);
                        var updatedAt = new Date(n.updatedAt || n.UpdatedAt);
                        var isNew = (updatedAt.getTime() - createdAt.getTime()) < 5000;
                        var actionBy = (n.updatedBy || n.UpdatedBy) || (n.createdBy || n.CreatedBy) || 'Sistema';
                        
                        return {
                            title: isNew ? 'Nueva nota registrada' : 'Nota actualizada',
                            description: (n.nombre || n.Nombre || '') + ': ' + (n.valor || n.Valor || 0) + ' puntos',
                            actionBy: actionBy,
                            date: updatedAt,
                            type: 'nota'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Notas:', err);
                    return []; 
                });

            // Load latest deleted notas
            var notasDeletedPromise = apiService.get('Notas/deleted', notasParams)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    return items.map(function(n) {
                        var updatedAt = new Date(n.updatedAt || n.UpdatedAt);
                        var actionBy = (n.deletedBy || n.DeletedBy) || 'Sistema';
                        
                        return {
                            title: 'Nota eliminada',
                            description: (n.nombre || n.Nombre || '') + ': ' + (n.valor || n.Valor || 0) + ' puntos',
                            actionBy: actionBy,
                            date: updatedAt,
                            type: 'nota'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Notas eliminadas:', err);
                    return []; 
                });

            // Load latest estudiantes (created, updated, or deleted)
            var estudiantesParams = { page: 1, pageSize: 10, maxPageSize: 10, sortField: 'UpdatedAt', sortDesc: true };
            var estudiantesPromise = apiService.get('estudiantes', estudiantesParams)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    return items.map(function(e) {
                        var createdAt = new Date(e.createdAt || e.CreatedAt);
                        var updatedAt = new Date(e.updatedAt || e.UpdatedAt);
                        var isNew = (updatedAt.getTime() - createdAt.getTime()) < 5000;
                        var actionBy = (e.updatedBy || e.UpdatedBy) || (e.createdBy || e.CreatedBy) || 'Sistema';
                        
                        return {
                            title: isNew ? 'Estudiante creado' : 'Estudiante actualizado',
                            description: (e.nombre || e.Nombre || '') + (isNew ? ' agregado al sistema' : ' modificado'),
                            actionBy: actionBy,
                            date: updatedAt,
                            type: 'estudiante'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Estudiantes:', err);
                    return []; 
                });

            // Load latest deleted estudiantes
            var estudiantesDeletedPromise = apiService.get('estudiantes/deleted', estudiantesParams)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    return items.map(function(e) {
                        var updatedAt = new Date(e.updatedAt || e.UpdatedAt);
                        var actionBy = (e.deletedBy || e.DeletedBy) || 'Sistema';
                        
                        return {
                            title: 'Estudiante eliminado',
                            description: (e.nombre || e.Nombre || ''),
                            actionBy: actionBy,
                            date: updatedAt,
                            type: 'estudiante'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Estudiantes eliminados:', err);
                    return []; 
                });

            // Load latest profesores (created, updated, or deleted)
            var profesoresParams = { page: 1, pageSize: 10, maxPageSize: 10, sortField: 'UpdatedAt', sortDesc: true };
            var profesoresPromise = apiService.get('profesores', profesoresParams)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    return items.map(function(p) {
                        var createdAt = new Date(p.createdAt || p.CreatedAt);
                        var updatedAt = new Date(p.updatedAt || p.UpdatedAt);
                        var isNew = (updatedAt.getTime() - createdAt.getTime()) < 5000;
                        var actionBy = (p.updatedBy || p.UpdatedBy) || (p.createdBy || p.CreatedBy) || 'Sistema';
                        
                        return {
                            title: isNew ? 'Profesor creado' : 'Profesor actualizado',
                            description: (p.nombre || p.Nombre || '') + (isNew ? ' agregado al sistema' : ' modificado'),
                            actionBy: actionBy,
                            date: updatedAt,
                            type: 'profesor'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Profesores:', err);
                    return []; 
                });

            // Load latest deleted profesores
            var profesoresDeletedPromise = apiService.get('profesores/deleted', profesoresParams)
                .then(function(res) {
                    var items = (res.data && (res.data.items || res.data.Items)) || [];
                    return items.map(function(p) {
                        var updatedAt = new Date(p.updatedAt || p.UpdatedAt);
                        var actionBy = (p.deletedBy || p.DeletedBy) || 'Sistema';
                        
                        return {
                            title: 'Profesor eliminado',
                            description: (p.nombre || p.Nombre || ''),
                            actionBy: actionBy,
                            date: updatedAt,
                            type: 'profesor'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Profesores eliminados:', err);
                    return []; 
                });

            // Combine all activities and sort by date (newest first)
            $q.all([notasPromise, notasDeletedPromise, estudiantesPromise, estudiantesDeletedPromise, profesoresPromise, profesoresDeletedPromise])
                .then(function(results) {
                    var allActivities = [];
                    allActivities = allActivities.concat(results[0]); // notas
                    allActivities = allActivities.concat(results[1]); // notas eliminadas
                    allActivities = allActivities.concat(results[2]); // estudiantes
                    allActivities = allActivities.concat(results[3]); // estudiantes eliminados
                    allActivities = allActivities.concat(results[4]); // profesores
                    allActivities = allActivities.concat(results[5]); // profesores eliminados
                    
                    // Sort by date descending (newest first)
                    allActivities.sort(function(a, b) {
                        return new Date(b.date) - new Date(a.date);
                    });
                    
                    // Keep only the 5 most recent
                    vm.recentActivity = allActivities.slice(0, 5);
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