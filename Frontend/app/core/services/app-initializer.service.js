(function() {
    'use strict';

    /**
     * Application Initialization Service
     * Handles the loading of environment configuration before app startup
     * 
     * This service ensures that environment variables are loaded
     * before any other services that depend on them are initialized
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('appInitializer', appInitializer);

    appInitializer.$inject = ['$q', '$log', 'envConfigLoader'];
    function appInitializer($q, $log, envConfigLoader) {
        
        var service = {
            initialize: initialize
        };

        return service;

        ////////////////

        /**
         * Initialize application by loading environment configuration
         * @returns {Promise} Promise that resolves when initialization is complete
         */
        function initialize() {
            $log.info('AppInitializer: Starting application initialization...');
            
            return envConfigLoader.loadConfig()
                .then(function(config) {
                    $log.info('AppInitializer: Environment configuration loaded successfully');
                    $log.debug('AppInitializer: Loaded config keys:', Object.keys(config));
                    return config;
                })
                .catch(function(error) {
                    $log.error('AppInitializer: Failed to load environment configuration:', error);
                    // Don't fail the app startup, use defaults
                    return {};
                })
                .finally(function() {
                    $log.info('AppInitializer: Application initialization completed');
                });
        }
    }

})();