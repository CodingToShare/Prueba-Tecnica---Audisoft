(function() {
    'use strict';

    /**
     * Route authentication service
     * Handles route protection based on user authentication and roles
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('routeAuthService', routeAuthService);

    routeAuthService.$inject = ['$location', '$q'];
    function routeAuthService($location, $q) {
        
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
            // TODO: Implement with authService in META 4
            // For now, check if token exists in localStorage
            var token = localStorage.getItem('audisoft_token');
            return !!token;
        }

        /**
         * Check if current user has specific role
         * @param {string} role - Role to check
         * @returns {boolean} True if user has role, false otherwise
         */
        function hasRole(role) {
            // TODO: Implement with authService in META 4
            var user = getCurrentUser();
            return user && user.roles && user.roles.indexOf(role) !== -1;
        }

        /**
         * Get current user information
         * @returns {Object|null} User object or null if not authenticated
         */
        function getCurrentUser() {
            // TODO: Implement with authService in META 4
            // For now, return mock data if token exists
            var token = localStorage.getItem('audisoft_token');
            if (!token) {
                return null;
            }

            // Mock user data - replace with real implementation in META 4
            try {
                var userData = localStorage.getItem('audisoft_user');
                return userData ? JSON.parse(userData) : null;
            } catch (e) {
                console.error('Error parsing user data:', e);
                return null;
            }
        }

        /**
         * Redirect to login page
         */
        function redirectToLogin() {
            $location.path('/login');
        }

        /**
         * Redirect to unauthorized page
         */
        function redirectToUnauthorized() {
            $location.path('/unauthorized');
        }
    }

})();