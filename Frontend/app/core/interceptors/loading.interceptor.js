(function () {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .factory('loadingInterceptor', loadingInterceptor)
        .config(configureInterceptor);

    loadingInterceptor.$inject = ['$q', '$injector'];
    function loadingInterceptor($q, $injector) {
        function isApiRequest(url) {
            if (!url) return false;
            try {
                var configService = $injector.get('configService');
                var apiBase = (configService.getApiConfig && configService.getApiConfig().baseUrl) || '';
                if (apiBase) {
                    var base = String(apiBase).replace(/\/$/, '');
                    var target = String(url);
                    return target.indexOf(base) === 0;
                }
            } catch (e) {}
            return String(url).indexOf('/api/') !== -1;
        }

        return {
            request: function (config) {
                if (isApiRequest(config.url) && !config.suppressLoading) {
                    $injector.get('loadingService').show();
                }
                return config;
            },
            response: function (response) {
                if (isApiRequest(response.config.url) && !response.config.suppressLoading) {
                    $injector.get('loadingService').hide();
                }
                return response;
            },
            responseError: function (rejection) {
                if (rejection && rejection.config && isApiRequest(rejection.config.url) && !rejection.config.suppressLoading) {
                    $injector.get('loadingService').hide();
                }
                return $q.reject(rejection);
            }
        };
    }

    configureInterceptor.$inject = ['$httpProvider'];
    function configureInterceptor($httpProvider) {
        // Register after authInterceptor to ensure token injection occurs regardless of loading behavior
        $httpProvider.interceptors.push('loadingInterceptor');
    }
})();
