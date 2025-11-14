(function() {
    'use strict';

    /**
     * Authentication HTTP Interceptor
     * Automatically injects JWT tokens into outgoing requests and handles authentication responses
     * 
     * Features:
     * - Automatic JWT token injection
     * - Response authentication error handling
     * - Token refresh on 401 responses
     * - Request queuing during token refresh
     * - Redirect to login on authentication failures
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('authInterceptor', authInterceptor)
        .config(configureInterceptor);

    authInterceptor.$inject = ['$q', '$injector', '$log'];
    function authInterceptor($q, $injector, $log) {
        
        var isRefreshingToken = false;
        var requestQueue = [];
        
        var interceptor = {
            request: requestInterceptor,
            responseError: responseErrorInterceptor
        };

        return interceptor;

        ////////////////

        /**
         * Intercept outgoing requests to add authentication token
         * @param {Object} config - HTTP request configuration
         * @returns {Object} Modified request configuration
         */
        function requestInterceptor(config) {
            // Get authService using $injector to avoid circular dependency
            var authService = $injector.get('authService');
            var token = authService.getToken();

            // Add Authorization header if token exists and URL is an API endpoint
            if (token && isApiRequest(config.url)) {
                config.headers = config.headers || {};
                config.headers.Authorization = 'Bearer ' + token;
                
                $log.debug('AuthInterceptor: Added Bearer token to request', config.url);
            }

            return config;
        }

        /**
         * Intercept response errors to handle authentication failures
         * @param {Object} rejection - HTTP response error
         * @returns {Promise} Promise that resolves with retry or rejects with error
         */
        function responseErrorInterceptor(rejection) {
            var authService = $injector.get('authService');
            var $http = $injector.get('$http');
            
            // Only handle API requests
            if (!isApiRequest(rejection.config.url)) {
                return $q.reject(rejection);
            }
            
            // Handle 401 Unauthorized responses
            if (rejection.status === 401) {
                $log.warn('AuthInterceptor: 401 Unauthorized - Token expired/invalid for', rejection.config.url);
                
                // Skip token refresh for login/auth endpoints to avoid infinite loops
                if (isAuthEndpoint(rejection.config.url)) {
                    $log.debug('AuthInterceptor: Skipping token refresh for auth endpoint');
                    return handleAuthenticationFailure(authService);
                }
                
                // If we're already refreshing token, queue this request
                if (isRefreshingToken) {
                    return queueRequest(rejection.config, $http);
                }
                
                // Attempt token refresh if refresh token is available
                return attemptTokenRefresh(rejection, authService, $http);
            }
            
            // Handle 403 Forbidden responses
            if (rejection.status === 403) {
                return handleForbiddenError(rejection, authService);
            }
            
            // Handle other response errors
            if (rejection.status >= 400) {
                logResponseError(rejection);
            }

            return $q.reject(rejection);
        }

        /**
         * Attempt to refresh authentication token
         * @param {Object} originalRejection - Original 401 response
         * @param {Object} authService - Authentication service
         * @param {Object} $http - HTTP service
         * @returns {Promise} Promise for retry or final rejection
         */
        function attemptTokenRefresh(originalRejection, authService, $http) {
            isRefreshingToken = true;
            
            return authService.refreshAccessToken()
                .then(function(newToken) {
                    $log.info('AuthInterceptor: Token refreshed successfully');
                    
                    // Update the original request with new token
                    originalRejection.config.headers.Authorization = 'Bearer ' + newToken;
                    
                    // Retry the original request
                    return $http(originalRejection.config);
                })
                .catch(function(refreshError) {
                    $log.error('AuthInterceptor: Token refresh failed', refreshError);
                    
                    // Redirect to login on refresh failure
                    authService.redirectToLogin();
                    
                    return $q.reject(originalRejection);
                })
                .finally(function() {
                    isRefreshingToken = false;
                    processRequestQueue($http);
                });
        }

        /**
         * Queue request during token refresh
         * @param {Object} config - Request configuration
         * @param {Object} $http - HTTP service
         * @returns {Promise} Promise for queued request
         */
        function queueRequest(config, $http) {
            var deferred = $q.defer();
            
            requestQueue.push({
                config: config,
                deferred: deferred
            });
            
            $log.debug('AuthInterceptor: Request queued during token refresh', config.url);
            
            return deferred.promise;
        }

        /**
         * Process queued requests after token refresh
         * @param {Object} $http - HTTP service
         */
        function processRequestQueue($http) {
            if (requestQueue.length === 0) {
                return;
            }
            
            $log.debug('AuthInterceptor: Processing', requestQueue.length, 'queued requests');
            
            var authService = $injector.get('authService');
            var token = authService.getToken();
            
            requestQueue.forEach(function(queuedRequest) {
                if (token) {
                    // Update request with new token
                    queuedRequest.config.headers = queuedRequest.config.headers || {};
                    queuedRequest.config.headers.Authorization = 'Bearer ' + token;
                    
                    // Retry the request
                    $http(queuedRequest.config)
                        .then(function(response) {
                            queuedRequest.deferred.resolve(response);
                        })
                        .catch(function(error) {
                            queuedRequest.deferred.reject(error);
                        });
                } else {
                    // No token available, reject the request
                    queuedRequest.deferred.reject({
                        status: 401,
                        statusText: 'Unauthorized',
                        data: { message: 'Authentication required' }
                    });
                }
            });
            
            // Clear the queue
            requestQueue = [];
        }

        /**
         * Check if URL is an API request that needs authentication
         * @param {string} url - Request URL
         * @returns {boolean} True if API request
         */
        function isApiRequest(url) {
            if (!url) {
                return false;
            }

            try {
                var configService = $injector.get('configService');
                var apiBase = (configService.getApiConfig && configService.getApiConfig().baseUrl) || '';
                if (apiBase) {
                    // Normalize both URL and base
                    var base = String(apiBase).replace(/\/$/, '');
                    var target = String(url);
                    return target.indexOf(base) === 0;
                }
            } catch (e) {
                // Fall through to heuristic if configService not available
            }

            // Heuristic fallback: treat paths containing '/api/' as API
            return String(url).indexOf('/api/') !== -1;
        }

        /**
         * Check if URL is an authentication endpoint
         * @param {string} url - Request URL
         * @returns {boolean} True if auth endpoint
         */
        function isAuthEndpoint(url) {
            if (!url) {
                return false;
            }
            var u = String(url).toLowerCase();
            return u.indexOf('/auth/login') !== -1 || 
                   u.indexOf('/auth/register') !== -1 ||
                   u.indexOf('/auth/forgot-password') !== -1;
        }

        /**
         * Handle 403 Forbidden errors - Insufficient permissions
         * @param {Object} rejection - HTTP response error
         * @param {Object} authService - Authentication service
         * @returns {Promise} Rejected promise after handling
         */
        function handleForbiddenError(rejection, authService) {
            $log.warn('AuthInterceptor: 403 Forbidden - Insufficient permissions for', rejection.config.url);
            
            // Show user-friendly message
            showErrorMessage('No tienes permisos para realizar esta acción.');
            
            // If user is authenticated but lacks permissions, redirect to unauthorized page
            if (authService.isAuthenticated()) {
                var $location = $injector.get('$location');
                $location.path('/unauthorized');
            } else {
                // User is not authenticated, redirect to login
                return handleAuthenticationFailure(authService);
            }
            
            return $q.reject(rejection);
        }

        /**
         * Handle authentication failure by clearing session and redirecting to login
         * @param {Object} authService - Authentication service
         * @returns {Promise} Rejected promise
         */
        function handleAuthenticationFailure(authService) {
            $log.info('AuthInterceptor: Handling authentication failure - redirecting to login');
            
            // Show user-friendly message
            showErrorMessage('Tu sesión ha expirado. Por favor, inicia sesión nuevamente.');
            
            // Clear user session and redirect to login
            authService.logout().finally(function() {
                authService.redirectToLogin();
            });
            
            return $q.reject({ 
                message: 'Authentication failed',
                redirectToLogin: true 
            });
        }

        /**
         * Show error message to user
         * @param {string} message - Error message to display
         */
        function showErrorMessage(message) {
            // For now, use console warn. In a real app, this would be a toast/notification service
            console.warn('AuthInterceptor Error:', message);
            
            // TODO: Integrate with toast notification service when available
            // Example: notificationService.showError(message);
        }

        /**
         * Log response error for debugging
         * @param {Object} rejection - HTTP response error
         */
        function logResponseError(rejection) {
            var errorMessage = 'HTTP ' + rejection.status;
            if (rejection.statusText) {
                errorMessage += ' ' + rejection.statusText;
            }
            
            if (rejection.data && rejection.data.message) {
                errorMessage += ': ' + rejection.data.message;
            }
            
            $log.error('AuthInterceptor: Response error for', rejection.config.url, '-', errorMessage);
        }
    }

    /**
     * Configure the interceptor in the HTTP provider
     */
    configureInterceptor.$inject = ['$httpProvider'];
    function configureInterceptor($httpProvider) {
        $httpProvider.interceptors.push('authInterceptor');
    }

})();