(function() {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .factory('reportesService', reportesService);

    reportesService.$inject = ['apiService', '$log'];
    function reportesService(apiService, $log) {
        var baseEndpoint = '/api/v1/reportes';

        var service = {
            getNotasResumen: getNotasResumen,
            exportNotasCsv: exportNotasCsv
        };

        return service;

        function getNotasResumen(params) {
            var endpoint = baseEndpoint + '/notas/resumen';
            return apiService.get(endpoint, params)
                .then(function(res) { return res.data; });
        }

        function exportNotasCsv(params) {
            var endpoint = baseEndpoint + '/notas/export';
            return apiService.get(endpoint, params, {
                responseType: 'arraybuffer',
                headers: { 'Accept': 'text/csv' }
            }).then(function(res) {
                return res; // caller handles blob creation
            });
        }
    }
})();
