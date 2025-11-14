(function() {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .factory('reportesService', reportesService);

    reportesService.$inject = ['apiService', '$log'];
    function reportesService(apiService, $log) {
        // Base endpoint relative to configured API baseUrl (which already includes /api/v1)
        var baseEndpoint = 'Reportes';

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
