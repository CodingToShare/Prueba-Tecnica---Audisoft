(function() {
    'use strict';

    /**
     * Route authentication service
     * Handles route protection based on user authentication and roles
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('routeAuthService', routeAuthService);

    routeAuthService.$inject = ['$location', '$q', 'authService'];
    function routeAuthService($location, $q, authService) {
        
        var service = {
            checkRouteAccess: checkRouteAccess,
            isAuthenticated: isAuthenticated,
            hasRole: hasRole,
            getCurrentUser: getCurrentUser,
            redirectToLogin: redirectToLogin,
            redirectToUnauthorized: redirectToUnauthorized
        };

        return service;

        ////////////////

        /**
         * Check if user has access to a specific route
         * @param {Object} routeAccess - Route access configuration
         * @returns {Promise} Promise that resolves if access granted, rejects if denied
         */
        function checkRouteAccess(routeAccess) {
            var deferred = $q.defer();

            // If route doesn't require login, allow access
            if (!routeAccess.requiresLogin) {
                deferred.resolve();
                return deferred.promise;
            }

            // Check if user is authenticated
            if (!isAuthenticated()) {
                redirectToLogin();
                deferred.reject('Not authenticated');
                return deferred.promise;
            }

            // Check if user has required role
            if (routeAccess.allowedRoles && routeAccess.allowedRoles.length > 0) {
                var hasRequiredRole = routeAccess.allowedRoles.some(function(role) {
                    return hasRole(role);
                });

                if (!hasRequiredRole) {
                    redirectToUnauthorized();
                    deferred.reject('Insufficient permissions');
                    return deferred.promise;
                }
            }

            deferred.resolve();
            return deferred.promise;
        }

        /**
         * Check if user is currently authenticated
         * @returns {boolean} True if authenticated, false otherwise
         */
        function isAuthenticated() {
            return authService.isAuthenticated();
        }

        /**
         * Check if current user has specific role
         * @param {string} role - Role to check
         * @returns {boolean} True if user has role, false otherwise
         */
        function hasRole(role) {
            return authService.hasRole(role);
        }

        /**
         * Get current user information
         * @returns {Object|null} User object or null if not authenticated
         */
        function getCurrentUser() {
            return authService.getCurrentUser();
        }

        /**
         * Redirect to login page
         */
        function redirectToLogin() {
            authService.redirectToLogin();
        }

        /**
         * Redirect to unauthorized page
         */
        function redirectToUnauthorized() {
            $location.path('/unauthorized');
        }
    }

})();