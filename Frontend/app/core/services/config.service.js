(function() {
    'use strict';

    /**
     * Configuration service for API and application settings
     * Centralizes all configuration values and environment-specific settings
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('configService', configService);

    configService.$inject = ['envConfigLoader'];
    function configService(envConfigLoader) {
        
        // Environment detection
        var environment = detectEnvironment();
        
        // Get environment configuration
        var envConfig = envConfigLoader.getConfig();
        
        // Configuration object using environment variables
        var config = {
            // Environment
            environment: environment,
            isProduction: environment === 'production',
            isDevelopment: environment === 'development',
            
            // API Configuration
            api: {
                baseUrl: getApiBaseUrl(environment, envConfig),
                timeout: envConfig.API_TIMEOUT || 30000,
                retryAttempts: envConfig.API_RETRY_ATTEMPTS || 3,
                retryDelay: envConfig.API_RETRY_DELAY || 1000
            },
            
            // Authentication Configuration
            auth: {
                tokenKey: envConfig.AUTH_TOKEN_KEY || 'audisoft_token',
                userKey: envConfig.AUTH_USER_KEY || 'audisoft_user',
                refreshTokenKey: envConfig.AUTH_REFRESH_TOKEN_KEY || 'audisoft_refresh_token',
                tokenExpiryBuffer: envConfig.AUTH_TOKEN_EXPIRY_BUFFER || 300000
            },
            
            // Pagination Configuration
            pagination: {
                defaultPageSize: envConfig.PAGINATION_DEFAULT_PAGE_SIZE || 20,
                maxPageSize: envConfig.PAGINATION_MAX_PAGE_SIZE || 100,
                pageSizes: envConfig.PAGINATION_PAGE_SIZES || [10, 20, 50, 100]
            },
            
            // UI Configuration
            ui: {
                toastDuration: envConfig.UI_TOAST_DURATION || 5000,
                loadingDelay: envConfig.UI_LOADING_DELAY || 300,
                debounceDelay: envConfig.UI_DEBOUNCE_DELAY || 300
            },
            
            // Application Information
            app: {
                name: envConfig.APP_NAME || 'AudiSoft School',
                version: envConfig.APP_VERSION || '1.0.0',
                description: envConfig.APP_DESCRIPTION || 'Sistema de Gestión Escolar'
            }
        };

        var service = {
            // Getters
            getConfig: getConfig,
            getApiConfig: getApiConfig,
            getAuthConfig: getAuthConfig,
            getPaginationConfig: getPaginationConfig,
            getUiConfig: getUiConfig,
            getAppInfo: getAppInfo,
            
            // Environment
            getEnvironment: getEnvironment,
            isProduction: isProduction,
            isDevelopment: isDevelopment,
            
            // Specific getters
            getApiBaseUrl: function() { return config.api.baseUrl; },
            getTokenKey: function() { return config.auth.tokenKey; },
            getUserKey: function() { return config.auth.userKey; },
            getDefaultPageSize: function() { return config.pagination.defaultPageSize; }
        };

        return service;

        ////////////////

        /**
         * Detect current environment based on URL
         * @returns {string} Environment name
         */
        function detectEnvironment() {
            var hostname = window.location.hostname;
            
            // Only two environments like backend: development and production
            if (hostname === 'localhost' || hostname === '127.0.0.1' || hostname.indexOf('192.168.') === 0) {
                return 'development';
            } else {
                return 'production';
            }
        }

        /**
         * Get API base URL based on environment (like backend appsettings pattern)
         * @param {string} env - Environment name
         * @param {Object} envConfig - Environment configuration
         * @returns {string} API base URL
         */
        function getApiBaseUrl(env, envConfig) {
            switch (env) {
                case 'development':
                    return envConfig.API_BASE_URL_DEVELOPMENT || 'http://localhost:5281/api/v1';
                case 'production':
                    return envConfig.API_BASE_URL_PRODUCTION || 'https://api.audisoft.com/api/v1';
                default:
                    return envConfig.API_BASE_URL_DEVELOPMENT || 'http://localhost:5281/api/v1';
            }
        }

        /**
         * Get complete configuration object
         * @returns {Object} Configuration object
         */
        function getConfig() {
            return angular.copy(config);
        }

        /**
         * Get API configuration
         * @returns {Object} API configuration
         */
        function getApiConfig() {
            return angular.copy(config.api);
        }

        /**
         * Get authentication configuration
         * @returns {Object} Auth configuration
         */
        function getAuthConfig() {
            return angular.copy(config.auth);
        }

        /**
         * Get pagination configuration
         * @returns {Object} Pagination configuration
         */
        function getPaginationConfig() {
            return angular.copy(config.pagination);
        }

        /**
         * Get UI configuration
         * @returns {Object} UI configuration
         */
        function getUiConfig() {
            return angular.copy(config.ui);
        }

        /**
         * Get application information
         * @returns {Object} App information
         */
        function getAppInfo() {
            return angular.copy(config.app);
        }

        /**
         * Get current environment
         * @returns {string} Environment name
         */
        function getEnvironment() {
            return config.environment;
        }

        /**
         * Check if running in production
         * @returns {boolean} True if production
         */
        function isProduction() {
            return config.isProduction;
        }

        /**
         * Check if running in development
         * @returns {boolean} True if development
         */
        function isDevelopment() {
            return config.isDevelopment;
        }
    }

})();