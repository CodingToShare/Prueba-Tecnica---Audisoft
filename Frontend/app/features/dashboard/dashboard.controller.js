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
                .then(function(res) { return res.totalCount || 0; })
                .catch(function() { return 0; });

            var estudiantesCountP = apiService.get('estudiantes', paramsCount)
                .then(function(res) { return res.totalCount || 0; })
                .catch(function() { return 0; });

            var profesoresCountP = apiService.get('profesores', paramsCount)
                .then(function(res) { return res.totalCount || 0; })
                .catch(function() { return 0; });

            var notasAvgP = apiService.get('Notas', paramsNotas)
                .then(function(res) {
                    var items = res.data && res.data.Items ? res.data.Items : [];
                    if (!items.length) return null;
                    var sum = 0;
                    for (var i = 0; i < items.length; i++) { sum += (items[i].Valor || 0); }
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
            
            // Parámetros para traer últimos cambios
            var params = { page: 1, pageSize: 20, maxPageSize: 20, sortField: 'UpdatedAt', sortDesc: true };
            
            // Promesa 1: Traer notas
            var notasPromise = apiService.get('Notas', params)
                .then(function(res) {
                    console.log('DEBUG - Notas Response:', res);
                    // res.data es el objeto PagedResult<T> completo con Items, TotalCount, etc.
                    var items = res.data && res.data.Items ? res.data.Items : [];
                    console.log('DEBUG - Notas Items count:', items.length);
                    
                    return items.map(function(n) {
                        var createdAt = new Date(n.CreatedAt || new Date());
                        var updatedAt = n.UpdatedAt ? new Date(n.UpdatedAt) : createdAt;
                        var isNew = (updatedAt.getTime() - createdAt.getTime()) < 5000;
                        var user = n.UpdatedBy || n.CreatedBy || 'Sistema';
                        
                        console.log('DEBUG - Nota ID:', n.Id, 'Nombre:', n.Nombre, 'UpdatedBy:', n.UpdatedBy, 'CreatedBy:', n.CreatedBy, 'User final:', user);
                        
                        return {
                            title: isNew ? 'Nueva nota registrada' : 'Nota actualizada',
                            description: (n.Nombre || '') + ': ' + (n.Valor || 0) + ' puntos',
                            actionBy: user,
                            date: updatedAt,
                            type: 'nota'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Notas:', err);
                    return []; 
                });

            // Promesa 2: Traer estudiantes
            var estudiantesPromise = apiService.get('estudiantes', params)
                .then(function(res) {
                    console.log('DEBUG - Estudiantes Response:', res);
                    // res.data es el objeto PagedResult<T> completo con Items, TotalCount, etc.
                    var items = res.data && res.data.Items ? res.data.Items : [];
                    console.log('DEBUG - Estudiantes Items count:', items.length);
                    
                    return items.map(function(e) {
                        var createdAt = new Date(e.CreatedAt || new Date());
                        var updatedAt = e.UpdatedAt ? new Date(e.UpdatedAt) : createdAt;
                        var isNew = (updatedAt.getTime() - createdAt.getTime()) < 5000;
                        var user = e.UpdatedBy || e.CreatedBy || 'Sistema';
                        
                        console.log('DEBUG - Estudiante ID:', e.Id, 'Nombre:', e.Nombre, 'UpdatedBy:', e.UpdatedBy, 'CreatedBy:', e.CreatedBy, 'User final:', user);
                        
                        return {
                            title: isNew ? 'Estudiante creado' : 'Estudiante actualizado',
                            description: (e.Nombre || '') + (isNew ? ' agregado al sistema' : ' modificado'),
                            actionBy: user,
                            date: updatedAt,
                            type: 'estudiante'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Estudiantes:', err);
                    return []; 
                });

            // Promesa 3: Traer profesores
            var profesoresPromise = apiService.get('profesores', params)
                .then(function(res) {
                    console.log('DEBUG - Profesores Response:', res);
                    // res.data es el objeto PagedResult<T> completo con Items, TotalCount, etc.
                    var items = res.data && res.data.Items ? res.data.Items : [];
                    console.log('DEBUG - Profesores Items count:', items.length);
                    
                    return items.map(function(p) {
                        var createdAt = new Date(p.CreatedAt || new Date());
                        var updatedAt = p.UpdatedAt ? new Date(p.UpdatedAt) : createdAt;
                        var isNew = (updatedAt.getTime() - createdAt.getTime()) < 5000;
                        var user = p.UpdatedBy || p.CreatedBy || 'Sistema';
                        
                        console.log('DEBUG - Profesor ID:', p.Id, 'Nombre:', p.Nombre, 'UpdatedBy:', p.UpdatedBy, 'CreatedBy:', p.CreatedBy, 'User final:', user);
                        
                        return {
                            title: isNew ? 'Profesor creado' : 'Profesor actualizado',
                            description: (p.Nombre || '') + (isNew ? ' agregado al sistema' : ' modificado'),
                            actionBy: user,
                            date: updatedAt,
                            type: 'profesor'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Profesores:', err);
                    return []; 
                });

            // Combinar resultados y ordenar
            $q.all([notasPromise, estudiantesPromise, profesoresPromise])
                .then(function(results) {
                    var allActivities = [];
                    
                    // Concatenar todo
                    if (results[0] && results[0].length > 0) {
                        console.log('DEBUG - Adding', results[0].length, 'notas');
                        allActivities = allActivities.concat(results[0]);
                    }
                    if (results[1] && results[1].length > 0) {
                        console.log('DEBUG - Adding', results[1].length, 'estudiantes');
                        allActivities = allActivities.concat(results[1]);
                    }
                    if (results[2] && results[2].length > 0) {
                        console.log('DEBUG - Adding', results[2].length, 'profesores');
                        allActivities = allActivities.concat(results[2]);
                    }
                    
                    console.log('DEBUG - Total activities before sort:', allActivities.length);
                    
                    // Ordenar por fecha (más reciente primero)
                    allActivities.sort(function(a, b) {
                        var aTime = new Date(a.date).getTime();
                        var bTime = new Date(b.date).getTime();
                        return bTime - aTime;
                    });
                    
                    console.log('DEBUG - Total activities after sort:', allActivities.length);
                    console.log('DEBUG - Activities (top 10):');
                    allActivities.slice(0, 10).forEach(function(a, idx) {
                        console.log(idx, ':', a.title, '-', a.actionBy, '-', new Date(a.date).toLocaleString());
                    });
                    
                    // Mostrar solo 5 más recientes
                    vm.recentActivity = allActivities.slice(0, 5);
                    console.log('DEBUG - Final recent activity count:', vm.recentActivity.length);
                });
        }

        function loadLatestNotas() {
            var params = { page: 1, pageSize: 5, maxPageSize: 5, sortField: 'CreatedAt', sortDesc: true };
            apiService.get('Notas', params)
                .then(function(res) {
                    var items = res.data && res.data.Items ? res.data.Items : [];
                    vm.latestNotas = items.map(function(n) {
                        return {
                            id: n.Id,
                            nombre: n.Nombre,
                            valor: n.Valor,
                            createdAt: n.CreatedAt
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