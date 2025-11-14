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
    runApp.$inject = ['$rootScope', '$location'];
    function runApp($rootScope, $location) {
        // Global event listeners
        $rootScope.$on('$routeChangeStart', function(event, next, current) {
            // Will add authentication checks in META 4
            console.log('Route change started:', next.originalPath);
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
    MainController.$inject = ['$scope', '$location'];
    function MainController($scope, $location) {
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
            // Will implement with authService in META 4
            // For now, return false to show login state
            return false;
        }

        function getCurrentUser() {
            // Will implement with authService in META 4
            // Return mock user for development
            return {
                userName: 'Usuario',
                roles: ['Guest']
            };
        }

        function logout() {
            // Will implement with authService in META 4
            console.log('Logout clicked');
            $location.path('/login');
        }
    }

})();