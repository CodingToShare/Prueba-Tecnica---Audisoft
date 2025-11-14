(function() {
    'use strict';

    /**
     * Environment Configuration Loader
     * Loads environment variables from .env file and provides them to the application
     * 
     * This service loads configuration from .env file and environment-specific overrides
     * It provides a centralized way to access all environment variables
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('envConfigLoader', envConfigLoader);

    envConfigLoader.$inject = ['$http', '$q', '$log'];
    function envConfigLoader($http, $q, $log) {
        
        var envConfig = {};
        var isLoaded = false;
        
        var service = {
            loadConfig: loadConfig,
            getConfig: getConfig,
            get: get,
            isConfigLoaded: isConfigLoaded,
            getEnvironmentFile: getEnvironmentFile
        };

        return service;

        ////////////////

        /**
         * Load environment configuration from API endpoint or .env files
         * @returns {Promise} Promise that resolves when config is loaded
         */
        function loadConfig() {
            if (isLoaded) {
                return $q.resolve(envConfig);
            }

            // Try 1: Load from /api/config endpoint (recommended)
            $log.debug('EnvConfigLoader: Attempt 1 - Loading from /api/config');
            return $http.get('/api/config', { timeout: 5000 })
                .then(function(response) {
                    envConfig = response.data;
                    isLoaded = true;
                    $log.info('EnvConfigLoader: Configuration loaded from /api/config', envConfig);
                    return envConfig;
                })
                .catch(function(error) {
                    $log.warn('EnvConfigLoader: /api/config failed, attempting .env files');
                    
                    // Try 2: Load from environment-specific .env file
                    var envFile = getEnvironmentFile();
                    $log.debug('EnvConfigLoader: Attempt 2 - Loading from', envFile);
                    
                    return $http.get(envFile, { timeout: 5000 })
                        .then(function(response) {
                            envConfig = parseEnvFile(response.data);
                            isLoaded = true;
                            $log.info('EnvConfigLoader: Configuration loaded from', envFile);
                            return envConfig;
                        })
                        .catch(function(error2) {
                            $log.warn('EnvConfigLoader: Environment-specific file failed, trying .env fallback');
                            
                            // Try 3: Load from default .env file
                            return $http.get('.env', { timeout: 5000 })
                                .then(function(response) {
                                    envConfig = parseEnvFile(response.data);
                                    isLoaded = true;
                                    $log.info('EnvConfigLoader: Configuration loaded from .env fallback');
                                    return envConfig;
                                })
                                .catch(function(error3) {
                                    $log.error('EnvConfigLoader: All config sources failed, using defaults');
                                    envConfig = getDefaultConfig();
                                    isLoaded = true;
                                    return envConfig;
                                });
                        });
                });
        }

        /**
         * Get complete configuration object
         * @returns {Object} Configuration object
         */
        function getConfig() {
            return angular.copy(envConfig);
        }

        /**
         * Get specific configuration value
         * @param {string} key - Configuration key
         * @param {*} defaultValue - Default value if key not found
         * @returns {*} Configuration value
         */
        function get(key, defaultValue) {
            return envConfig.hasOwnProperty(key) ? envConfig[key] : defaultValue;
        }

        /**
         * Check if configuration is loaded
         * @returns {boolean} True if loaded
         */
        function isConfigLoaded() {
            return isLoaded;
        }

        /**
         * Parse .env file content
         * @param {string} content - .env file content
         * @returns {Object} Parsed configuration
         */
        function parseEnvFile(content) {
            var config = {};
            var lines = content.split('\n');

            lines.forEach(function(line) {
                // Skip comments and empty lines
                line = line.trim();
                if (!line || line.startsWith('#')) {
                    return;
                }

                // Parse key=value pairs
                var equalIndex = line.indexOf('=');
                if (equalIndex > 0) {
                    var key = line.substring(0, equalIndex).trim();
                    var value = line.substring(equalIndex + 1).trim();

                    // Remove quotes if present
                    if ((value.startsWith('"') && value.endsWith('"')) ||
                        (value.startsWith("'") && value.endsWith("'"))) {
                        value = value.slice(1, -1);
                    }

                    // Convert specific types
                    config[key] = convertValue(value);
                }
            });

            return config;
        }

        /**
         * Convert string values to appropriate types
         * @param {string} value - String value from .env
         * @returns {*} Converted value
         */
        function convertValue(value) {
            // Boolean conversion
            if (value.toLowerCase() === 'true') return true;
            if (value.toLowerCase() === 'false') return false;

            // Number conversion
            if (!isNaN(value) && !isNaN(parseFloat(value))) {
                return parseFloat(value);
            }

            // Array conversion (comma-separated)
            if (value.indexOf(',') > -1) {
                return value.split(',').map(function(item) {
                    return convertValue(item.trim());
                });
            }

            // String value
            return value;
        }

        /**
         * Get environment-specific file name based on hostname (like backend appsettings pattern)
         * @returns {string} Environment file name
         */
        function getEnvironmentFile() {
            var hostname = window.location.hostname;
            
            // Only two environments like backend: development and production
            if (hostname === 'localhost' || hostname === '127.0.0.1' || hostname.indexOf('192.168.') === 0) {
                return '.env.development';
            } else {
                return '.env'; // Production
            }
        }

        /**
         * Get default configuration (fallback)
         * @returns {Object} Default configuration
         */
        function getDefaultConfig() {
            return {
                API_BASE_URL_DEVELOPMENT: 'http://localhost:5281/api/v1',
                API_BASE_URL_PRODUCTION: 'https://api.audisoft.com/api/v1',
                API_TIMEOUT: 30000,
                API_RETRY_ATTEMPTS: 3,
                API_RETRY_DELAY: 1000,
                AUTH_TOKEN_KEY: 'audisoft_token',
                AUTH_USER_KEY: 'audisoft_user',
                AUTH_REFRESH_TOKEN_KEY: 'audisoft_refresh_token',
                AUTH_TOKEN_EXPIRY_BUFFER: 300000,
                PAGINATION_DEFAULT_PAGE_SIZE: 20,
                PAGINATION_MAX_PAGE_SIZE: 100,
                PAGINATION_PAGE_SIZES: [10, 20, 50, 100],
                UI_TOAST_DURATION: 5000,
                UI_LOADING_DELAY: 300,
                UI_DEBOUNCE_DELAY: 300,
                APP_NAME: 'AudiSoft School',
                APP_VERSION: '1.0.0',
                APP_DESCRIPTION: 'Sistema de Gestión Escolar',
                DEBUG_MODE: true,
                LOG_LEVEL: 'debug'
            };
        }
    }

})();