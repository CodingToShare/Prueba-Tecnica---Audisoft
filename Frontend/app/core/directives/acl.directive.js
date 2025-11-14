(function() {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .directive('hasRole', hasRoleDirective)
        .directive('hasAnyRole', hasAnyRoleDirective);

    hasRoleDirective.$inject = ['authService'];
    function hasRoleDirective(authService) {
        return {
            restrict: 'A',
            link: function(scope, element, attrs) {
                // Start hidden to prevent flicker until evaluated
                element.addClass('d-none');

                function isAllowed() {
                    var role = (attrs.hasRole || '').trim();
                    if (!role) return false;
                    return !!(authService.isAuthenticated() && authService.hasRole(role));
                }

                function updateVisibility() {
                    if (isAllowed()) {
                        element.removeClass('d-none');
                    } else {
                        element.addClass('d-none');
                    }
                }

                // Initial evaluation
                updateVisibility();

                // Watch for user/role changes
                scope.$watch(function() { return authService.getCurrentUser(); }, function() {
                    updateVisibility();
                }, true);
            }
        };
    }

    hasAnyRoleDirective.$inject = ['authService'];
    function hasAnyRoleDirective(authService) {
        return {
            restrict: 'A',
            link: function(scope, element, attrs) {
                // Start hidden to prevent flicker until evaluated
                element.addClass('d-none');

                function parseRoles(value) {
                    if (!value) return [];
                    // Accept comma, pipe or space separated lists
                    return value
                        .split(/[|,\s]+/)
                        .map(function(r) { return (r || '').trim(); })
                        .filter(function(r) { return !!r; });
                }

                function isAllowed() {
                    var roles = parseRoles(attrs.hasAnyRole);
                    if (!roles.length) return false;
                    if (!authService.isAuthenticated()) return false;

                    var i;
                    for (i = 0; i < roles.length; i++) {
                        if (authService.hasRole(roles[i])) return true;
                    }
                    return false;
                }

                function updateVisibility() {
                    if (isAllowed()) {
                        element.removeClass('d-none');
                    } else {
                        element.addClass('d-none');
                    }
                }

                // Initial evaluation
                updateVisibility();

                // Watch for user/role changes
                scope.$watch(function() { return authService.getCurrentUser(); }, function() {
                    updateVisibility();
                }, true);
            }
        };
    }
})();
