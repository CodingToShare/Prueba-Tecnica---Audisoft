(function() {
    'use strict';

    /**
     * Dashboard Controller
     * Manages the main dashboard view with statistics and quick actions
     */
    angular
        .module('audiSoftSchoolApp')
        .controller('DashboardController', DashboardController);

    DashboardController.$inject = ['$scope', 'routeAuthService', 'apiService', 'configService', 'authService', 'envConfigLoader'];
    function DashboardController($scope, routeAuthService, apiService, configService, authService, envConfigLoader) {
        var vm = this;

        // Bindable properties
        vm.stats = {};
        vm.recentActivity = [];
        vm.lastLogin = null;
        vm.apiStatus = 'Checking...';
        vm.apiConfig = configService.getApiConfig();
        vm.authInfo = {};
        vm.envInfo = {};

        // Bindable methods  
        vm.testLogin = testLogin;
        vm.testLogout = testLogout;

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
            testApiConnection();
            loadAuthInfo();
            loadEnvInfo();
        }

        function testApiConnection() {
            // Test API connectivity - this will likely return 401 (Unauthorized) which is expected
            apiService.get('estudiantes')
                .then(function(response) {
                    vm.apiStatus = 'Connected ✓';
                })
                .catch(function(error) {
                    if (error.status === 401) {
                        vm.apiStatus = 'Connected (Auth Required) ✓';
                    } else if (error.status === 0) {
                        vm.apiStatus = 'Connection Failed ✗';
                    } else {
                        vm.apiStatus = 'Connected (Status: ' + error.status + ') ✓';
                    }
                });
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

        function loadAuthInfo() {
            vm.authInfo = {
                isAuthenticated: authService.isAuthenticated(),
                currentUser: authService.getCurrentUser(),
                token: authService.getToken() ? 'Present' : 'Not found',
                role: authService.getRole(),
                roles: authService.getRoles()
            };
        }

        function testLogin() {
            // Mock login for testing (will fail due to no backend auth endpoint yet)
            authService.login('admin@test.com', 'admin123')
                .then(function(result) {
                    vm.authInfo.loginTest = 'Success: ' + result.user.username;
                    loadAuthInfo();
                })
                .catch(function(error) {
                    vm.authInfo.loginTest = 'Failed: ' + error.message;
                });
        }

        function testLogout() {
            authService.logout()
                .then(function() {
                    vm.authInfo.logoutTest = 'Success';
                    loadAuthInfo();
                })
                .catch(function(error) {
                    vm.authInfo.logoutTest = 'Failed: ' + error.message;
                });
        }

        function loadEnvInfo() {
            vm.envInfo = {
                configLoaded: envConfigLoader.isConfigLoaded(),
                environment: configService.getEnvironment(),
                envFile: envConfigLoader.getEnvironmentFile(),
                apiBaseUrl: configService.getApiBaseUrl(),
                appName: configService.getAppInfo().name,
                appVersion: configService.getAppInfo().version,
                debugMode: configService.get('DEBUG_MODE', false)
            };
        }
    }

})();