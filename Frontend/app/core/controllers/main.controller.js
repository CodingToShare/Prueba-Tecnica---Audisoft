(function() {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .controller('MainController', MainController);

    MainController.$inject = ['$scope', '$rootScope', 'authService'];

    function MainController($scope, $rootScope, authService) {
        var vm = this;

        vm.isAuthenticated = authService.isAuthenticated;
        vm.getCurrentUser = authService.getCurrentUser;
        vm.logout = authService.logout;

        /**
         * Verifica si el usuario actual tiene alguno de los roles especificados.
         * @param {string[]} roles - Un array de nombres de roles a verificar.
         * @returns {boolean} - True si el usuario tiene al menos uno de los roles, de lo contrario false.
         */
        vm.hasAnyRole = function(roles) {
            if (!vm.isAuthenticated() || !roles || roles.length === 0) {
                return false;
            }
            var currentUserRoles = vm.getCurrentUser().roles;
            if (!currentUserRoles) return false;
            
            return roles.some(function(role) {
                return currentUserRoles.indexOf(role) !== -1;
            });
        };

        // Observador para actualizar el scope cuando cambia el estado de autenticación
        $rootScope.$on('auth:login-success', function() {
            $scope.$applyAsync();
        });

        $rootScope.$on('auth:logout-success', function() {
            $scope.$applyAsync();
        });
    }
})();
