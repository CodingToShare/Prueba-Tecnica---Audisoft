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
        
        // Will add auth interceptor in META 5
        // $httpProvider.interceptors.push('authInterceptor');
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
    MainController.$inject = ['$scope', '$location', 'routeAuthService'];
    function MainController($scope, $location, routeAuthService) {
        var vm = this;

        // Bindable properties and methods
        vm.isAuthenticated = isAuthenticated;
        vm.getCurrentUser = getCurrentUser;
        vm.logout = logout;

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
            localStorage.removeItem('audisoft_token');
            localStorage.removeItem('audisoft_user');
            $location.path('/login');
        }
    }

})();