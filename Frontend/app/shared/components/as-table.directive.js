(function () {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .directive('asTable', asTableDirective);

    function asTableDirective() {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                items: '=',
                columns: '=',
                total: '<',
                page: '=',
                pageSize: '=',
                pageSizes: '=?',
                totalPages: '=?',
                loading: '<',
                orderBy: '=',
                orderDirection: '=',
                emptyMessage: '@?',
                onPageChange: '&',
                onPageSizeChange: '&',
                onSort: '&'
            },
            transclude: {
                actions: '?actions'
            },
            template:
                '<div class="card">' +
                '  <div class="table-responsive" ng-if="(items && items.length) && !loading">' +
                '    <table class="table table-hover mb-0">' +
                '      <thead class="table-light">' +
                '        <tr>' +
                '          <th ng-repeat="col in columns | filter:{ visible: undefined } track by $index" ng-style="{width: col.width}">' +
                '            <span ng-if="!col.sortable">{{ col.label }}</span>' +
                '            <a href="" ng-if="col.sortable" ng-click="sort(col)" class="text-decoration-none">' +
                '              {{ col.label }} ' +
                '              <i class="bi" ng-class="{\'bi-caret-up-fill\': isSorted(col, \'asc\'), \'bi-caret-down-fill\': isSorted(col, \'desc\')}"></i>' +
                '            </a>' +
                '          </th>' +
                '          <th ng-if="hasActions" style="width: 120px; text-align: center;">Acciones</th>' +
                '        </tr>' +
                '      </thead>' +
                '      <tbody>' +
                '        <tr ng-repeat="row in items track by $index">' +
                '          <td ng-repeat="col in columns | filter:{ visible: undefined } track by $index" ng-init="$parent.$parent.$parent.currentRow = row">' +
                '            <span ng-switch="col.type">' +
                '              <span ng-switch-when="badge" class="badge" ng-class="col.badgeClass || \'bg-secondary\'">{{ getValue(row, col) }}</span>' +
                '              <small ng-switch-when="small" class="text-muted">{{ getValue(row, col) }}</small>' +
                '              <code ng-switch-when="code">{{ getValue(row, col) }}</code>' +
                '              <span ng-switch-default>{{ getValue(row, col) }}</span>' +
                '            </span>' +
                '          </td>' +
                '          <td ng-if="hasActions" style="text-align: center;">' +
                '            <div class="btn-group btn-group-sm" role="group" ng-transclude="actions"></div>' +
                '          </td>' +
                '        </tr>' +
                '      </tbody>' +
                '    </table>' +
                '  </div>' +
                '  <div class="card-body text-center text-muted" ng-if="(!items || !items.length) && !loading">' +
                '    <i class="bi bi-inbox display-1 mb-3 d-block"></i>' +
                '    <p>{{ emptyMessage || \"No se encontraron registros\" }}</p>' +
                '  </div>' +
                '  <div class="card-body text-center" ng-if="loading">' +
                '    <div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div>' +
                '  </div>' +
                '  <div class="card-footer bg-light" ng-if="showPagination()">' +
                '    <div class="row align-items-center">' +
                '      <div class="col-md-6">' +
                '        <label class="form-label mb-0">' +
                '          Elementos por página: ' +
                '          <select class="form-select d-inline-block w-auto" ng-model="pageSize" ng-change="handlePageSizeChange()" ng-options="size for size in (pageSizes || [5,10,20,50])"></select>' +
                '        </label>' +
                '      </div>' +
                '      <div class="col-md-6">' +
                '        <nav aria-label="Page navigation" class="float-end">' +
                '          <ul class="pagination mb-0 justify-content-end">' +
                '            <li class="page-item" ng-class="{\'disabled\': page === 1}">' +
                '              <a class="page-link" ng-click="goTo(page - 1)" ng-if="page > 1"><i class="bi bi-chevron-left"></i> Anterior</a>' +
                '              <span class="page-link" ng-if="page === 1"><i class="bi bi-chevron-left"></i> Anterior</span>' +
                '            </li>' +
                '            <li class="page-item" ng-repeat="p in [] | range:1:(computedTotalPages()+1)" ng-class="{\'active\': p === page}" ng-if="p >= page - 2 && p <= page + 2">' +
                '              <a class="page-link" ng-click="goTo(p)" ng-if="p !== page">{{ p }}</a>' +
                '              <span class="page-link" ng-if="p === page">{{ p }} <span class="visually-hidden">(current)</span></span>' +
                '            </li>' +
                '            <li class="page-item" ng-class="{\'disabled\': page === computedTotalPages()}">' +
                '              <a class="page-link" ng-click="goTo(page + 1)" ng-if="page < computedTotalPages()">Siguiente <i class="bi bi-chevron-right"></i></a>' +
                '              <span class="page-link" ng-if="page === computedTotalPages()">Siguiente <i class="bi bi-chevron-right"></i></span>' +
                '            </li>' +
                '          </ul>' +
                '        </nav>' +
                '      </div>' +
                '    </div>' +
                '    <div class="small text-muted text-center mt-2">' +
                '      Mostrando {{ (page - 1) * pageSize + 1 }} a {{ Math.min(page * pageSize, total) }} de {{ total }} registros' +
                '    </div>' +
                '  </div>' +
                '</div>',
            link: function (scope, element, attrs, ctrl, $transclude) {
                scope.Math = Math;
                scope.hasActions = $transclude && $transclude.isSlotFilled && $transclude.isSlotFilled('actions');

                scope.getValue = function (row, col) {
                    if (!col) return '';
                    if (typeof col.get === 'function') {
                        try { return col.get(row); } catch (e) { return ''; }
                    }
                    var v = row && col.field ? row[col.field] : '';
                    if (col.format === 'date') {
                        return v ? (new Date(v)) : '';
                    }
                    return v;
                };

                scope.isSorted = function (col, dir) {
                    return scope.orderBy === col.field && scope.orderDirection === dir;
                };

                scope.sort = function (col) {
                    if (!col || !col.sortable) return;
                    var field = col.field;
                    scope.onSort({ field: field });
                };

                scope.computedTotalPages = function () {
                    if (scope.totalPages) return scope.totalPages;
                    if (!scope.total || !scope.pageSize) return 1;
                    return Math.max(1, Math.ceil(scope.total / scope.pageSize));
                };

                scope.showPagination = function () {
                    return scope.computedTotalPages() > 1;
                };

                scope.goTo = function (p) {
                    if (p < 1 || p > scope.computedTotalPages()) return;
                    scope.page = p;
                    scope.onPageChange({ page: p });
                };

                scope.handlePageSizeChange = function () {
                    scope.page = 1;
                    scope.onPageSizeChange();
                };
            }
        };
    }
})();
