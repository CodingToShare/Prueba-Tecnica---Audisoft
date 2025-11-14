(function() {
    'use strict';

    /**
     * Generic API Service for HTTP operations
     * Provides centralized HTTP methods for consuming the .NET 8 backend API
     * 
     * Features:
     * - Standard REST methods (GET, POST, PUT, DELETE)
     * - Centralized error handling
     * - Query parameter handling
     * - Response transformation
     * - Configurable base URL and headers
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('apiService', apiService);

    apiService.$inject = ['$http', '$q', '$log', 'configService'];
    function apiService($http, $q, $log, configService) {
        
        // Get configuration from config service
        var apiConfig = configService.getApiConfig();
        
        // Service configuration
        var config = {
            baseUrl: apiConfig.baseUrl,
            timeout: apiConfig.timeout,
            defaultHeaders: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            }
        };

        var service = {
            // Configuration methods
            setBaseUrl: setBaseUrl,
            getBaseUrl: getBaseUrl,
            setDefaultHeader: setDefaultHeader,
            
            // HTTP methods
            get: get,
            post: post,
            put: put,
            delete: deleteMethod,
            
            // Utility methods
            buildUrl: buildUrl,
            buildQueryString: buildQueryString
        };

        return service;

        ////////////////

        /**
         * Set the base URL for API calls
         * @param {string} url - The base URL
         */
        function setBaseUrl(url) {
            config.baseUrl = url.replace(/\/$/, ''); // Remove trailing slash
            $log.debug('API Service: Base URL set to', config.baseUrl);
        }

        /**
         * Get the current base URL
         * @returns {string} The current base URL
         */
        function getBaseUrl() {
            return config.baseUrl;
        }

        /**
         * Set a default header for all requests
         * @param {string} name - Header name
         * @param {string} value - Header value
         */
        function setDefaultHeader(name, value) {
            config.defaultHeaders[name] = value;
            $log.debug('API Service: Header set', name, value);
        }

        /**
         * Perform HTTP GET request
         * @param {string} endpoint - API endpoint (relative to base URL)
         * @param {Object} params - Query parameters object
         * @param {Object} options - Additional HTTP options
         * @returns {Promise} Promise that resolves with response data
         */
        function get(endpoint, params, options) {
            var url = buildUrl(endpoint);
            
            // For GET requests, don't send Content-Type header to avoid CORS preflight
            var headers = {
                'Accept': 'application/json'
            };
            
            var httpConfig = angular.extend({
                method: 'GET',
                url: url,
                headers: headers,
                timeout: config.timeout,
                params: params || {}
            }, options || {});

            $log.debug('API Service GET:', url, params);

            return executeRequest(httpConfig);
        }

        /**
         * Perform HTTP POST request
         * @param {string} endpoint - API endpoint (relative to base URL)
         * @param {Object} data - Request body data
         * @param {Object} options - Additional HTTP options
         * @returns {Promise} Promise that resolves with response data
         */
        function post(endpoint, data, options) {
            var url = buildUrl(endpoint);
            
            var httpConfig = angular.extend({
                method: 'POST',
                url: url,
                headers: angular.copy(config.defaultHeaders),
                timeout: config.timeout,
                data: data || {}
            }, options || {});

            $log.debug('API Service POST:', url, data);

            return executeRequest(httpConfig);
        }

        /**
         * Perform HTTP PUT request
         * @param {string} endpoint - API endpoint (relative to base URL)
         * @param {Object} data - Request body data
         * @param {Object} options - Additional HTTP options
         * @returns {Promise} Promise that resolves with response data
         */
        function put(endpoint, data, options) {
            var url = buildUrl(endpoint);
            
            var httpConfig = angular.extend({
                method: 'PUT',
                url: url,
                headers: angular.copy(config.defaultHeaders),
                timeout: config.timeout,
                data: data || {}
            }, options || {});

            $log.debug('API Service PUT:', url, data);

            return executeRequest(httpConfig);
        }

        /**
         * Perform HTTP DELETE request
         * @param {string} endpoint - API endpoint (relative to base URL)
         * @param {Object} options - Additional HTTP options
         * @returns {Promise} Promise that resolves with response data
         */
        function deleteMethod(endpoint, options) {
            var url = buildUrl(endpoint);
            
            var httpConfig = angular.extend({
                method: 'DELETE',
                url: url,
                headers: angular.copy(config.defaultHeaders),
                timeout: config.timeout
            }, options || {});

            $log.debug('API Service DELETE:', url);

            return executeRequest(httpConfig);
        }

        /**
         * Execute HTTP request with error handling
         * @param {Object} httpConfig - HTTP configuration object
         * @returns {Promise} Promise that resolves with processed response
         */
        function executeRequest(httpConfig) {
            return $http(httpConfig)
                .then(function(response) {
                    return processSuccessResponse(response);
                })
                .catch(function(error) {
                    return handleErrorResponse(error);
                });
        }

        /**
         * Process successful response
         * @param {Object} response - HTTP response object
         * @returns {Object} Processed response data
         */
        function processSuccessResponse(response) {
            $log.debug('API Service Success:', response.status, response.config.url);
            
            // Extract headers - handle both function and object formats
            var headersObj = {};
            if (typeof response.headers === 'function') {
                headersObj = response.headers() || {};
            } else if (typeof response.headers === 'object') {
                headersObj = response.headers || {};
            }
            
            // Extract pagination headers if present
            var result = {
                data: response.data,
                status: response.status,
                headers: headersObj
            };

            // Add pagination info if available (X-Total-Count header)
            var totalCount = headersObj['x-total-count'] || headersObj['X-Total-Count'];
            if (totalCount) {
                result.totalCount = parseInt(totalCount, 10);
            }

            return result;
        }

        /**
         * Handle error response
         * @param {Object} error - HTTP error object
         * @returns {Promise} Rejected promise with processed error
         */
        function handleErrorResponse(error) {
            var errorInfo = {
                status: error.status || 0,
                message: 'Unknown error occurred',
                data: null,
                config: error.config
            };

            // Process different error types
            if (error.status === 0) {
                errorInfo.message = 'No se pudo conectar con el servidor. Verifique su conexión a internet.';
            } else if (error.status === 400) {
                errorInfo.message = error.data && error.data.message ? error.data.message : 'Datos inválidos enviados al servidor.';
                errorInfo.data = error.data;
            } else if (error.status === 401) {
                errorInfo.message = 'No está autorizado. Inicie sesión nuevamente.';
            } else if (error.status === 403) {
                errorInfo.message = 'No tiene permisos para realizar esta acción.';
            } else if (error.status === 404) {
                errorInfo.message = 'El recurso solicitado no fue encontrado.';
            } else if (error.status === 409) {
                errorInfo.message = error.data && error.data.message ? error.data.message : 'Conflicto con el estado actual del recurso.';
                errorInfo.data = error.data;
            } else if (error.status >= 500) {
                errorInfo.message = 'Error interno del servidor. Intente nuevamente más tarde.';
            } else {
                errorInfo.message = error.data && error.data.message ? error.data.message : 'Error desconocido.';
                errorInfo.data = error.data;
            }

            $log.error('API Service Error:', errorInfo.status, errorInfo.message, error.config.url);

            return $q.reject(errorInfo);
        }

        /**
         * Build complete URL from endpoint
         * @param {string} endpoint - API endpoint
         * @returns {string} Complete URL
         */
        function buildUrl(endpoint) {
            // Remove leading slash from endpoint if present
            var cleanEndpoint = endpoint.replace(/^\//, '');
            
            return config.baseUrl + '/' + cleanEndpoint;
        }

        /**
         * Build query string from parameters object
         * @param {Object} params - Parameters object
         * @returns {string} Query string
         */
        function buildQueryString(params) {
            if (!params || typeof params !== 'object') {
                return '';
            }

            var queryParts = [];
            
            for (var key in params) {
                if (params.hasOwnProperty(key) && params[key] !== null && params[key] !== undefined) {
                    var value = params[key];
                    
                    // Handle arrays
                    if (Array.isArray(value)) {
                        for (var i = 0; i < value.length; i++) {
                            queryParts.push(encodeURIComponent(key) + '=' + encodeURIComponent(value[i]));
                        }
                    } else {
                        queryParts.push(encodeURIComponent(key) + '=' + encodeURIComponent(value));
                    }
                }
            }

            return queryParts.length > 0 ? '?' + queryParts.join('&') : '';
        }
    }

})();