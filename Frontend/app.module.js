(function() {
    'use strict';

    /**
     * Main module for AudiSoft School application
     * 
     * Dependencies:
     * - ngRoute: For client-side routing
     * - ngResource: For RESTful resource consumption
     */
    angular
        .module('audiSoftSchoolApp', [
            'ngRoute',
            'ngResource'
        ])
        .config(configureApp)
        .run(runApp)
        .controller('MainController', MainController);

    // Configuration function
    configureApp.$inject = ['$httpProvider', '$locationProvider'];
    function configureApp($httpProvider, $locationProvider) {
        // Configure HTML5 mode for clean URLs (optional)
        $locationProvider.hashPrefix('!'); // SEO-friendly URLs with #!

        // Configure HTTP provider
        $httpProvider.defaults.headers.common['Content-Type'] = 'application/json';
        
        // Add auth interceptor for automatic JWT token injection and 401/403 handling
        $httpProvider.interceptors.push('authInterceptor');
    }

    // Run function - executes after module bootstrap
    runApp.$inject = ['$rootScope', '$location', 'routeAuthService', 'appInitializer'];
    function runApp($rootScope, $location, routeAuthService, appInitializer) {
        
        // Initialize environment configuration
        appInitializer.initialize();
        // Global event listeners for route protection
        $rootScope.$on('$routeChangeStart', function(event, next, current) {
            console.log('Route change started:', next.originalPath);
            
            // Check route access if route has access configuration
            if (next && next.access) {
                routeAuthService.checkRouteAccess(next.access)
                    .catch(function(error) {
                        console.log('Route access denied:', error);
                        event.preventDefault();
                    });
            }
        });

        $rootScope.$on('$routeChangeSuccess', function(event, current, previous) {
            console.log('Route change successful');
            hideLoading();
        });

        $rootScope.$on('$routeChangeError', function(event, current, previous, rejection) {
            console.error('Route change error:', rejection);
            hideLoading();
        });

        // Utility functions
        function hideLoading() {
            var loadingOverlay = document.getElementById('loading-overlay');
            if (loadingOverlay) {
                loadingOverlay.classList.add('d-none');
            }
        }
    }

    // Main Controller for navigation and global state
    MainController.$inject = ['$scope', '$location', 'routeAuthService', 'authService'];
    function MainController($scope, $location, routeAuthService, authService) {
        var vm = this;

        // Bindable properties and methods
        vm.isAuthenticated = isAuthenticated;
        vm.getCurrentUser = getCurrentUser;
        vm.logout = logout;
        vm.logoutAndLogin = logoutAndLogin;

        // Initialize controller
        activate();

        ////////////////

        function activate() {
            // Initialize main controller
            console.log('Main Controller initialized');
        }

        function isAuthenticated() {
            // Delegate to routeAuthService
            return routeAuthService.isAuthenticated();
        }

        function getCurrentUser() {
            // Delegate to routeAuthService
            return routeAuthService.getCurrentUser() || {
                userName: 'Usuario',
                roles: ['Guest']
            };
        }

        function logout() {
            // Will implement with authService in META 4
            console.log('Logout clicked');
            // Clear localStorage for now
            authService.logout();
            $location.path('/login');
        }

        /**
         * Logout current session and redirect to login
         * Used when user is in unauthorized page and wants to login with different credentials
         */
        function logoutAndLogin() {
            console.log('Logout and Login clicked - clearing session and redirecting to login');
            // Clear current session completely
            authService.logout();
            // Redirect to login with a flag to show success message
            $location.path('/login').search({ forceLogin: true });
        }
    }

})();