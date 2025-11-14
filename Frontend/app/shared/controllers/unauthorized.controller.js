(function() {
    'use strict';

    /**
     * Unauthorized Controller
     * Handles the unauthorized/access denied page
     */
    angular
        .module('audiSoftSchoolApp')
        .controller('UnauthorizedController', UnauthorizedController);

    UnauthorizedController.$inject = ['$location', 'authService', 'routeAuthService'];
    function UnauthorizedController($location, authService, routeAuthService) {
        var vm = this;

        // Bindable properties
        vm.isAuthenticated = isAuthenticated;
        vm.getCurrentUser = getCurrentUser;
        vm.logoutAndLogin = logoutAndLogin;

        ////////////////

        function isAuthenticated() {
            return routeAuthService.isAuthenticated();
        }

        function getCurrentUser() {
            return routeAuthService.getCurrentUser() || {
                userName: 'Usuario',
                roles: ['Guest']
            };
        }

        /**
         * Clear current session and redirect to login
         * Allows user to login with different credentials
         */
        function logoutAndLogin() {
            console.log('UnauthorizedController: logoutAndLogin - clearing session and redirecting to login');
            
            authService.logout().then(function() {
                console.log('Session cleared, redirecting to login');
                $location.path('/login');
            });
        }
    }

})();
