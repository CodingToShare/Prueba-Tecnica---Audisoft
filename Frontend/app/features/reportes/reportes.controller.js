(function () {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .controller('ReportesController', ReportesController);

    ReportesController.$inject = ['$scope', '$filter', '$log', 'reportesService', 'toastService'];
    function ReportesController($scope, $filter, $log, reportesService, toastService) {
        var vm = this;

        vm.loading = false;
        vm.summary = null;
        vm.filters = {
            from: null,
            to: null
        };

        vm.refresh = refresh;
        vm.clearFilters = clearFilters;
        vm.downloadCsv = downloadCsv;

        activate();

        function activate() {
            refresh();
        }

        function buildParams() {
            var params = {};
            if (vm.filters.from) params.from = $filter('date')(vm.filters.from, 'yyyy-MM-dd');
            if (vm.filters.to) params.to = $filter('date')(vm.filters.to, 'yyyy-MM-dd');
            return params;
        }

        function refresh() {
            vm.loading = true;
            var params = buildParams();
            return reportesService.getNotasResumen(params)
                .then(function (data) {
                    vm.summary = data;
                })
                .catch(function (err) {
                    $log.error('Error cargando resumen de reportes', err);
                    toastService.error(err.message || 'No se pudo cargar el reporte');
                })
                .finally(function () { vm.loading = false; });
        }

        function clearFilters() {
            vm.filters.from = null;
            vm.filters.to = null;
            refresh();
        }

        function downloadCsv() {
            var params = buildParams();
            return reportesService.exportNotasCsv(params)
                .then(function (res) {
                    var blob = new Blob([res.data], { type: 'text/csv;charset=utf-8' });
                    var url = window.URL.createObjectURL(blob);
                    var a = document.createElement('a');
                    a.href = url;
                    a.download = 'notas_' + (new Date()).toISOString().slice(0,19).replace(/[:T]/g, '-') + '.csv';
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    window.URL.revokeObjectURL(url);
                    toastService.success('Exportación CSV iniciada');
                })
                .catch(function (err) {
                    $log.error('Error exportando CSV', err);
                    toastService.error(err.message || 'No se pudo exportar el CSV');
                });
        }
    }
})();
