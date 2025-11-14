(function() {
    'use strict';

    /**
     * Authentication Service
     * Handles JWT token management, user session, login/logout functionality,
     * and role-based permissions for the AudiSoft School application
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('authService', authService);

    authService.$inject = ['$q', '$log', '$window', 'apiService', 'configService'];
    function authService($q, $log, $window, apiService, configService) {
        
        // Configuration
        var authConfig = configService.getAuthConfig();
        
        // Private variables
        var currentUser = null;
        var authToken = null;
        var refreshToken = null;
        var tokenExpiryTime = null;
        
        // Service interface
        var service = {
            // Authentication methods
            login: login,
            logout: logout,
            refreshAccessToken: refreshAccessToken,
            
            // User session methods
            getCurrentUser: getCurrentUser,
            setCurrentUser: setCurrentUser,
            isAuthenticated: isAuthenticated,
            
            // Token management
            getToken: getToken,
            setToken: setToken,
            clearToken: clearToken,
            isTokenExpired: isTokenExpired,
            getTokenExpiryTime: getTokenExpiryTime,
            
            // Role and permission methods
            hasRole: hasRole,
            hasPermission: hasPermission,
            getUserRoles: getUserRoles,
            
            // Storage methods
            saveAuthState: saveAuthState,
            loadAuthState: loadAuthState,
            clearAuthState: clearAuthState,
            
            // Utility methods
            redirectToLogin: redirectToLogin,
            getRedirectUrl: getRedirectUrl,
            setRedirectUrl: setRedirectUrl
        };

        // Initialize service
        initialize();

        return service;

        ////////////////

        /**
         * Initialize authentication service
         */
        function initialize() {
            loadAuthState();
            $log.debug('AuthService: Initialized');
        }

        /**
         * Authenticate user with credentials
         * @param {Object|string} credentials - User credentials object or username
         * @param {string} [credentials.userName] - Username (preferred)
         * @param {string} [credentials.username] - Username (alias)
         * @param {string} [credentials.email] - Email (alias)
         * @param {string} credentials.password - Password
         * @param {boolean} [credentials.rememberMe=true] - Persist session
         * @returns {Promise} Promise that resolves with user data
         */
        function login(credentials) {
            var deferred = $q.defer();

            // Normalize parameters: allow string username + password signature
            if (typeof credentials === 'string') {
                credentials = { userName: credentials };
            }

            var userName = credentials && (credentials.userName || credentials.username || credentials.email);
            var password = credentials && credentials.password;
            var rememberMe = credentials && (credentials.rememberMe === undefined ? true : !!credentials.rememberMe);

            if (!userName || !password) {
                deferred.reject({
                    message: 'Usuario y contraseña son requeridos',
                    type: 'validation'
                });
                return deferred.promise;
            }

            $log.debug('AuthService: Attempting login for', userName);

            var payload = { userName: userName, password: password, rememberMe: rememberMe };

            apiService.post('Auth/login', payload)
                .then(function(response) {
                    var authData = response.data;
                    
                    // Expected shape: { accessToken, refreshToken?, user }
                    if (authData && authData.accessToken && authData.user) {
                        setToken(authData.accessToken);
                        setCurrentUser(authData.user);
                        
                        if (authData.refreshToken) {
                            setRefreshToken(authData.refreshToken);
                        }
                        
                        saveAuthState();
                        
                        $log.info('AuthService: Login successful for user', authData.user.userName || authData.user.email || '');
                        deferred.resolve(authData.user);
                    } else {
                        $log.error('AuthService: Invalid authentication response structure');
                        deferred.reject({
                            message: 'Respuesta de autenticación inválida',
                            type: 'auth_error'
                        });
                    }
                })
                .catch(function(error) {
                    $log.error('AuthService: Login failed', error);
                    
                    var errorMessage = 'Error de autenticación';
                    if (error.status === 401) {
                        errorMessage = 'Usuario o contraseña incorrectos';
                    } else if (error.status === 0) {
                        errorMessage = 'No se puede conectar al servidor';
                    } else if (error.data && error.data.message) {
                        errorMessage = error.data.message;
                    }
                    
                    deferred.reject({
                        message: errorMessage,
                        type: 'auth_error',
                        status: error.status
                    });
                });

            return deferred.promise;
        }

        /**
         * Logout current user
         * @param {boolean} callApi - Whether to call logout API endpoint
         * @returns {Promise} Promise that resolves when logout is complete
         */
        function logout(callApi) {
            var deferred = $q.defer();
            
            $log.debug('AuthService: Logging out user');
            
            if (callApi && authToken) {
                // Call API to invalidate token on server
                apiService.post('auth/logout', { token: authToken })
                    .finally(function() {
                        performLocalLogout();
                        deferred.resolve();
                    });
            } else {
                performLocalLogout();
                deferred.resolve();
            }
            
            return deferred.promise;
        }

        /**
         * Perform local logout operations
         */
        function performLocalLogout() {
            currentUser = null;
            authToken = null;
            refreshToken = null;
            tokenExpiryTime = null;
            
            clearAuthState();
            
            $log.info('AuthService: User logged out');
        }

        /**
         * Refresh access token using refresh token
         * @returns {Promise} Promise that resolves with new token
         */
        function refreshAccessToken() {
            var deferred = $q.defer();

            if (!refreshToken) {
                deferred.reject({
                    message: 'No refresh token available',
                    type: 'auth_error'
                });
                return deferred.promise;
            }

            $log.debug('AuthService: Refreshing access token');

            apiService.post('Auth/refresh', { refreshToken: refreshToken })
                .then(function(response) {
                    var authData = response.data;
                    
                    if (authData && (authData.accessToken || authData.token)) {
                        setToken(authData.accessToken || authData.token);
                        
                        if (authData.refreshToken) {
                            setRefreshToken(authData.refreshToken);
                        }
                        
                        saveAuthState();
                        
                        $log.info('AuthService: Token refreshed successfully');
                        deferred.resolve(authData.accessToken || authData.token);
                    } else {
                        throw new Error('Invalid refresh response');
                    }
                })
                .catch(function(error) {
                    $log.error('AuthService: Token refresh failed', error);
                    
                    // Clear auth state on refresh failure
                    performLocalLogout();
                    
                    deferred.reject({
                        message: 'No se pudo renovar la sesión',
                        type: 'auth_error'
                    });
                });

            return deferred.promise;
        }

        /**
         * Get current authenticated user
         * @returns {Object|null} Current user object or null
         */
        function getCurrentUser() {
            return currentUser;
        }

        /**
         * Set current user
         * @param {Object} user - User object
         */
        function setCurrentUser(user) {
            currentUser = user;
            $log.debug('AuthService: Current user set', user ? user.email : 'null');
        }

        /**
         * Check if user is authenticated
         * @returns {boolean} True if authenticated
         */
        function isAuthenticated() {
            return !!(authToken && currentUser && !isTokenExpired());
        }

        /**
         * Get current authentication token
         * @returns {string|null} Current token or null
         */
        function getToken() {
            return authToken;
        }

        /**
         * Set authentication token
         * @param {string} token - JWT token
         */
        function setToken(token) {
            authToken = token;
            
            if (token) {
                // Parse token to get expiry time
                try {
                    var payload = parseJwtPayload(token);
                    if (payload.exp) {
                        tokenExpiryTime = new Date(payload.exp * 1000);
                    }
                } catch (error) {
                    $log.warn('AuthService: Could not parse token expiry', error);
                }
            } else {
                tokenExpiryTime = null;
            }
            
            $log.debug('AuthService: Token set', token ? 'present' : 'null');
        }

        /**
         * Set refresh token
         * @param {string} token - Refresh token
         */
        function setRefreshToken(token) {
            refreshToken = token;
            $log.debug('AuthService: Refresh token set', token ? 'present' : 'null');
        }

        /**
         * Clear authentication token
         */
        function clearToken() {
            authToken = null;
            refreshToken = null;
            tokenExpiryTime = null;
        }

        /**
         * Check if current token is expired
         * @returns {boolean} True if token is expired
         */
        function isTokenExpired() {
            if (!authToken || !tokenExpiryTime) {
                return true;
            }
            
            // Add buffer time for token expiry check
            var now = new Date();
            var expiryWithBuffer = new Date(tokenExpiryTime.getTime() - authConfig.tokenExpiryBuffer);
            
            return now >= expiryWithBuffer;
        }

        /**
         * Get token expiry time
         * @returns {Date|null} Token expiry time or null
         */
        function getTokenExpiryTime() {
            return tokenExpiryTime;
        }

        /**
         * Check if current user has specific role
         * @param {string} role - Role to check
         * @returns {boolean} True if user has role
         */
        function hasRole(role) {
            if (!currentUser || !currentUser.roles) {
                return false;
            }
            
            return currentUser.roles.indexOf(role) !== -1;
        }

        /**
         * Check if current user has specific permission
         * @param {string} permission - Permission to check
         * @returns {boolean} True if user has permission
         */
        function hasPermission(permission) {
            if (!currentUser || !currentUser.permissions) {
                return false;
            }
            
            return currentUser.permissions.indexOf(permission) !== -1;
        }

        /**
         * Get current user roles
         * @returns {Array} Array of user roles
         */
        function getUserRoles() {
            return currentUser && currentUser.roles ? currentUser.roles : [];
        }

        /**
         * Save authentication state to local storage
         */
        function saveAuthState() {
            try {
                if (authToken) {
                    $window.localStorage.setItem(authConfig.tokenKey, authToken);
                }
                
                if (refreshToken) {
                    $window.localStorage.setItem(authConfig.refreshTokenKey, refreshToken);
                }
                
                if (currentUser) {
                    $window.localStorage.setItem(authConfig.userKey, angular.toJson(currentUser));
                }
                
                $log.debug('AuthService: Authentication state saved');
            } catch (error) {
                $log.error('AuthService: Failed to save auth state', error);
            }
        }

        /**
         * Load authentication state from local storage
         */
        function loadAuthState() {
            try {
                var storedToken = $window.localStorage.getItem(authConfig.tokenKey);
                var storedRefreshToken = $window.localStorage.getItem(authConfig.refreshTokenKey);
                var storedUser = $window.localStorage.getItem(authConfig.userKey);
                
                if (storedToken) {
                    setToken(storedToken);
                }
                
                if (storedRefreshToken) {
                    setRefreshToken(storedRefreshToken);
                }
                
                if (storedUser) {
                    try {
                        currentUser = angular.fromJson(storedUser);
                    } catch (parseError) {
                        $log.warn('AuthService: Could not parse stored user data');
                        clearAuthState();
                    }
                }
                
                // Validate loaded state
                if (isTokenExpired()) {
                    $log.debug('AuthService: Stored token expired, clearing state');
                    clearAuthState();
                }
                
                $log.debug('AuthService: Authentication state loaded');
            } catch (error) {
                $log.error('AuthService: Failed to load auth state', error);
                clearAuthState();
            }
        }

        /**
         * Clear authentication state from local storage
         */
        function clearAuthState() {
            try {
                $window.localStorage.removeItem(authConfig.tokenKey);
                $window.localStorage.removeItem(authConfig.refreshTokenKey);
                $window.localStorage.removeItem(authConfig.userKey);
                
                $log.debug('AuthService: Authentication state cleared');
            } catch (error) {
                $log.error('AuthService: Failed to clear auth state', error);
            }
        }

        /**
         * Redirect to login page
         */
        function redirectToLogin() {
            var currentUrl = $window.location.hash;
            if (currentUrl && currentUrl !== '#!/login') {
                setRedirectUrl(currentUrl);
            }
            
            $window.location.hash = '#!/login';
        }

        /**
         * Get redirect URL after login
         * @returns {string} Redirect URL
         */
        function getRedirectUrl() {
            var url = $window.localStorage.getItem('redirect_url');
            return url || '#!/dashboard';
        }

        /**
         * Set redirect URL for after login
         * @param {string} url - URL to redirect to
         */
        function setRedirectUrl(url) {
            if (url && url !== '#!/login') {
                $window.localStorage.setItem('redirect_url', url);
            }
        }

        /**
         * Parse JWT payload
         * @param {string} token - JWT token
         * @returns {Object} Parsed payload
         */
        function parseJwtPayload(token) {
            var parts = token.split('.');
            if (parts.length !== 3) {
                throw new Error('Invalid token format');
            }
            
            var payload = parts[1];
            var decoded = $window.atob(payload);
            return angular.fromJson(decoded);
        }
    }

})();