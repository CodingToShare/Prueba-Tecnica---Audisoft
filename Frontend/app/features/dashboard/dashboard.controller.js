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
                .then(function(res) { 
                    console.log('Notas count response:', res);
                    return res.totalCount || 0; 
                })
                .catch(function(err) { 
                    console.error('Error getting Notas count:', err);
                    return 0; 
                });

            var estudiantesCountP = apiService.get('estudiantes', paramsCount)
                .then(function(res) { 
                    console.log('Estudiantes count response:', res);
                    return res.totalCount || 0; 
                })
                .catch(function(err) { 
                    console.error('Error getting Estudiantes count:', err);
                    return 0; 
                });

            var profesoresCountP = apiService.get('profesores', paramsCount)
                .then(function(res) { 
                    console.log('Profesores count response:', res);
                    return res.totalCount || 0; 
                })
                .catch(function(err) { 
                    console.error('Error getting Profesores count:', err);
                    return 0; 
                });

            var notasAvgP = apiService.get('Notas', paramsNotas)
                .then(function(res) {
                    var items = res.data && res.data.items ? res.data.items : [];
                    if (!items.length) return null;
                    var sum = 0;
                    for (var i = 0; i < items.length; i++) { sum += (items[i].valor || 0); }
                    return +(sum / items.length).toFixed(1);
                })
                .catch(function() { return null; });

            $q.all([notasCountP, estudiantesCountP, profesoresCountP, notasAvgP]).then(function(results) {
                console.log('Dashboard stats - Notas:', results[0], 'Estudiantes:', results[1], 'Profesores:', results[2], 'Promedio:', results[3]);
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
            
            // Parámetros para traer últimos cambios (creados, actualizados)
            var params = { page: 1, pageSize: 20, maxPageSize: 20, sortField: 'UpdatedAt', sortDesc: true };
            
            // Promesa 1: Traer notas activas (creadas y actualizadas)
            var notasPromise = apiService.get('Notas', params)
                .then(function(res) {
                    var items = res.data && res.data.items ? res.data.items : [];
                    return items.map(function(n) {
                        var createdAt = new Date(n.createdAt || new Date());
                        var updatedAt = n.updatedAt ? new Date(n.updatedAt) : createdAt;
                        var isNew = (updatedAt.getTime() - createdAt.getTime()) < 5000;
                        var user = n.updatedBy || n.createdBy || 'Sistema';
                        
                        return {
                            title: isNew ? 'Nueva nota registrada' : 'Nota actualizada',
                            description: (n.nombre || '') + ': ' + (n.valor || 0) + ' puntos',
                            actionBy: user,
                            date: updatedAt,
                            type: 'nota',
                            action: isNew ? 'creada' : 'actualizada'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Notas:', err);
                    return []; 
                });

            // Promesa 2: Traer notas eliminadas
            var notasDeletedPromise = apiService.get('Notas/deleted', params)
                .then(function(res) {
                    var items = res.data && res.data.items ? res.data.items : [];
                    return items.map(function(n) {
                        var deletedAt = n.deletedAt ? new Date(n.deletedAt) : new Date();
                        var user = n.deletedBy || 'Sistema';
                        
                        return {
                            title: 'Nota eliminada',
                            description: (n.nombre || '') + ' (eliminada)',
                            actionBy: user,
                            date: deletedAt,
                            type: 'nota',
                            action: 'eliminada'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Notas eliminadas:', err);
                    return []; 
                });

            // Promesa 3: Traer estudiantes activos (creados y actualizados)
            var estudiantesPromise = apiService.get('estudiantes', params)
                .then(function(res) {
                    var items = res.data && res.data.items ? res.data.items : [];
                    return items.map(function(e) {
                        var createdAt = new Date(e.createdAt || new Date());
                        var updatedAt = e.updatedAt ? new Date(e.updatedAt) : createdAt;
                        var isNew = (updatedAt.getTime() - createdAt.getTime()) < 5000;
                        var user = e.updatedBy || e.createdBy || 'Sistema';
                        
                        // Extraer grado del nombre (número después del último "-")
                        var nombrePartes = (e.nombre || '').split(' - ');
                        var nombre = nombrePartes[0] || e.nombre || '';
                        var grado = nombrePartes.length > 1 ? nombrePartes[nombrePartes.length - 1] : '';
                        var descripcion = nombre;
                        
                        return {
                            title: isNew ? 'Estudiante creado' : 'Estudiante actualizado',
                            description: descripcion + (isNew ? ' agregado al sistema' : ' modificado'),
                            actionBy: user,
                            date: updatedAt,
                            type: 'estudiante',
                            action: isNew ? 'creado' : 'actualizado',
                            grado: grado
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Estudiantes:', err);
                    return []; 
                });

            // Promesa 4: Traer estudiantes eliminados
            var estudiantesDeletedPromise = apiService.get('estudiantes/deleted', params)
                .then(function(res) {
                    var items = res.data && res.data.items ? res.data.items : [];
                    return items.map(function(e) {
                        var deletedAt = e.deletedAt ? new Date(e.deletedAt) : new Date();
                        var user = e.deletedBy || 'Sistema';
                        
                        // Extraer grado del nombre (número después del último "-")
                        var nombrePartes = (e.nombre || '').split(' - ');
                        var nombre = nombrePartes[0] || e.nombre || '';
                        var grado = nombrePartes.length > 1 ? nombrePartes[nombrePartes.length - 1] : '';
                        
                        return {
                            title: 'Estudiante eliminado',
                            description: nombre + ' (eliminado)',
                            actionBy: user,
                            date: deletedAt,
                            type: 'estudiante',
                            action: 'eliminado',
                            grado: grado
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Estudiantes eliminados:', err);
                    return []; 
                });

            // Promesa 5: Traer profesores activos (creados y actualizados)
            var profesoresPromise = apiService.get('profesores', params)
                .then(function(res) {
                    var items = res.data && res.data.items ? res.data.items : [];
                    return items.map(function(p) {
                        var createdAt = new Date(p.createdAt || new Date());
                        var updatedAt = p.updatedAt ? new Date(p.updatedAt) : createdAt;
                        var isNew = (updatedAt.getTime() - createdAt.getTime()) < 5000;
                        var user = p.updatedBy || p.createdBy || 'Sistema';
                        
                        return {
                            title: isNew ? 'Profesor creado' : 'Profesor actualizado',
                            description: (p.nombre || '') + (isNew ? ' agregado al sistema' : ' modificado'),
                            actionBy: user,
                            date: updatedAt,
                            type: 'profesor',
                            action: isNew ? 'creado' : 'actualizado'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Profesores:', err);
                    return []; 
                });

            // Promesa 6: Traer profesores eliminados
            var profesoresDeletedPromise = apiService.get('profesores/deleted', params)
                .then(function(res) {
                    var items = res.data && res.data.items ? res.data.items : [];
                    return items.map(function(p) {
                        var deletedAt = p.deletedAt ? new Date(p.deletedAt) : new Date();
                        var user = p.deletedBy || 'Sistema';
                        
                        return {
                            title: 'Profesor eliminado',
                            description: (p.nombre || '') + ' (eliminado)',
                            actionBy: user,
                            date: deletedAt,
                            type: 'profesor',
                            action: 'eliminado'
                        };
                    });
                })
                .catch(function(err) { 
                    console.error('Error cargando Profesores eliminados:', err);
                    return []; 
                });

            // Combinar resultados de todas las promesas
            $q.all([
                notasPromise, 
                notasDeletedPromise,
                estudiantesPromise, 
                estudiantesDeletedPromise,
                profesoresPromise, 
                profesoresDeletedPromise
            ])
                .then(function(results) {
                    var allActivities = [];
                    
                    // Concatenar todo
                    allActivities = allActivities.concat(results[0]); // notas activas
                    allActivities = allActivities.concat(results[1]); // notas eliminadas
                    allActivities = allActivities.concat(results[2]); // estudiantes activos
                    allActivities = allActivities.concat(results[3]); // estudiantes eliminados
                    allActivities = allActivities.concat(results[4]); // profesores activos
                    allActivities = allActivities.concat(results[5]); // profesores eliminados
                    
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
                    var items = res.data && res.data.items ? res.data.items : [];
                    vm.latestNotas = items.map(function(n) {
                        return {
                            id: n.id,
                            nombre: n.nombre,
                            valor: n.valor,
                            createdAt: n.createdAt
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