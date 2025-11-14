(function () {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .directive('asModal', asModalDirective);

    function asModalDirective() {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                title: '@',
                visible: '=',
                size: '@?',
                variant: '@?',
                onClose: '&?'
            },
            transclude: {
                body: '?body',
                footer: '?footer'
            },
            template:
                '<div>' +
                '  <div class="modal" tabindex="-1" ng-show="visible" style="display: block; background-color: rgba(0,0,0,0.5);">' +
                '    <div class="modal-dialog" ng-class="dialogClass()">' +
                '      <div class="modal-content">' +
                '        <div class="modal-header" ng-class="headerClass()">' +
                '          <h5 class="modal-title">{{ title }}</h5>' +
                '          <button type="button" class="btn-close" ng-class="{\'btn-close-white\': isColoredHeader()}" ng-click="close()"></button>' +
                '        </div>' +
                '        <div class="modal-body" ng-transclude="body"></div>' +
                '        <div class="modal-footer">' +
                '          <div class="d-flex w-100 justify-content-end" ng-transclude="footer"></div>' +
                '        </div>' +
                '      </div>' +
                '    </div>' +
                '  </div>' +
                '  <div class="modal-backdrop fade" ng-show="visible" ng-class="{\'show\': visible}"></div>' +
                '</div>',
            link: function (scope) {
                scope.variant = scope.variant || 'primary';

                scope.dialogClass = function () {
                    if (scope.size === 'sm') return 'modal-sm';
                    if (scope.size === 'lg') return 'modal-lg';
                    if (scope.size === 'xl') return 'modal-xl';
                    return '';
                };

                scope.headerClass = function () {
                    return scope.isColoredHeader() ? ('bg-' + scope.variant + ' text-white') : 'bg-light';
                };

                scope.isColoredHeader = function () {
                    return ['primary', 'secondary', 'success', 'danger', 'warning', 'info', 'dark'].indexOf(scope.variant) !== -1;
                };

                scope.close = function () {
                    scope.visible = false;
                    if (scope.onClose) scope.onClose();
                };
            }
        };
    }
})();
