(function() {
    'use strict';

    /**
     * Dashboard Controller
     * Manages the main dashboard view with statistics and quick actions
     */
    angular
        .module('audiSoftSchoolApp')
        .controller('DashboardController', DashboardController);

    DashboardController.$inject = ['$scope', 'routeAuthService'];
    function DashboardController($scope, routeAuthService) {
        var vm = this;

        // Bindable properties
        vm.stats = {};
        vm.recentActivity = [];
        vm.lastLogin = null;

        // Bindable methods
        vm.canViewStudents = canViewStudents;
        vm.canViewProfessors = canViewProfessors;
        vm.canCreateStudent = canCreateStudent;
        vm.canCreateProfessor = canCreateProfessor;
        vm.canCreateNote = canCreateNote;

        // Initialize controller
        activate();

        ////////////////

        function activate() {
            loadDashboardData();
            loadRecentActivity();
        }

        function loadDashboardData() {
            // TODO: Replace with real API calls in META 3
            // Mock data for development
            vm.stats = {
                totalEstudiantes: 156,
                totalProfesores: 24,
                totalNotas: 342,
                promedioGeneral: 78.5
            };

            vm.lastLogin = new Date();
        }

        function loadRecentActivity() {
            // TODO: Replace with real API calls in META 3
            // Mock recent activity data
            vm.recentActivity = [
                {
                    title: 'Nueva nota registrada',
                    description: 'Matemáticas - Juan Pérez: 95 puntos',
                    date: new Date(Date.now() - 1000 * 60 * 30) // 30 minutes ago
                },
                {
                    title: 'Estudiante creado',
                    description: 'María González agregada al sistema',
                    date: new Date(Date.now() - 1000 * 60 * 60 * 2) // 2 hours ago
                },
                {
                    title: 'Nota actualizada',
                    description: 'Historia - Carlos Ruiz: 88 puntos',
                    date: new Date(Date.now() - 1000 * 60 * 60 * 4) // 4 hours ago
                }
            ];
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
    }

})();