(function() {
    'use strict';

    /**
     * Authentication Service
     * Handles user authentication, JWT token management, and role-based access control
     * 
     * Features:
     * - Login/logout functionality
     * - JWT token storage and validation
     * - Role-based permissions
     * - Session management
     * - Authentication state management
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('authService', authService);

    authService.$inject = ['$q', '$log', '$window', 'apiService', 'configService'];
    function authService($q, $log, $window, apiService, configService) {
        
        // Get configuration
        var authConfig = configService.getAuthConfig();
        
        // Current user state
        var currentUser = null;
        var isLoggedIn = false;
        
        // Authentication listeners
        var authListeners = [];

        var service = {
            // Authentication methods
            login: login,
            logout: logout,
            refreshToken: refreshToken,
            
            // State methods
            isAuthenticated: isAuthenticated,
            getCurrentUser: getCurrentUser,
            getToken: getToken,
            getRole: getRole,
            getRoles: getRoles,
            
            // Permission methods
            hasRole: hasRole,
            hasAnyRole: hasAnyRole,
            hasPermission: hasPermission,
            
            // Session management
            saveSession: saveSession,
            clearSession: clearSession,
            loadSession: loadSession,
            isSessionExpired: isSessionExpired,
            
            // Event handling
            onAuthStateChange: onAuthStateChange,
            removeAuthListener: removeAuthListener,
            
            // Navigation helpers
            redirectToLogin: redirectToLogin
        };

        // Initialize service
        activate();

        return service;

        ////////////////

        /**
         * Initialize authentication service
         */
        function activate() {
            loadSession();
            $log.debug('AuthService: Initialized');
        }

        /**
         * Authenticate user with username and password
         * @param {string} username - User's username or email
         * @param {string} password - User's password
         * @returns {Promise} Promise that resolves with user data
         */
        function login(username, password) {
            if (!username || !password) {
                return $q.reject({ message: 'Username and password are required' });
            }

            $log.debug('AuthService: Attempting login for user:', username);

            var loginData = {
                username: username,
                password: password
            };

            return apiService.post('auth/login', loginData)
                .then(function(response) {
                    var userData = response.data;
                    
                    // Validate response structure
                    if (!userData.token || !userData.user) {
                        throw new Error('Invalid login response format');
                    }

                    // Save authentication data
                    saveSession(userData.token, userData.refreshToken, userData.user);
                    
                    // Update current state
                    currentUser = userData.user;
                    isLoggedIn = true;

                    $log.info('AuthService: Login successful for user:', username);
                    
                    // Notify listeners
                    notifyAuthStateChange(true, currentUser);

                    return {
                        user: currentUser,
                        token: userData.token,
                        success: true
                    };
                })
                .catch(function(error) {
                    $log.error('AuthService: Login failed:', error);
                    
                    var errorMessage = 'Login failed';
                    if (error.status === 401) {
                        errorMessage = 'Invalid username or password';
                    } else if (error.status === 403) {
                        errorMessage = 'Account is locked or inactive';
                    } else if (error.data && error.data.message) {
                        errorMessage = error.data.message;
                    }

                    return $q.reject({ 
                        message: errorMessage,
                        status: error.status 
                    });
                });
        }

        /**
         * Logout current user
         * @returns {Promise} Promise that resolves when logout is complete
         */
        function logout() {
            $log.debug('AuthService: Logging out user');

            var logoutPromise = $q.resolve();

            // Call logout endpoint if user is authenticated
            if (isLoggedIn && getToken()) {
                logoutPromise = apiService.post('auth/logout', {})
                    .catch(function(error) {
                        // Log error but don't fail logout process
                        $log.warn('AuthService: Logout endpoint failed:', error);
                    });
            }

            return logoutPromise.then(function() {
                // Clear session data
                clearSession();
                
                // Update current state
                currentUser = null;
                isLoggedIn = false;

                $log.info('AuthService: Logout completed');

                // Notify listeners
                notifyAuthStateChange(false, null);

                return { success: true };
            });
        }

        /**
         * Refresh authentication token
         * @returns {Promise} Promise that resolves with new token
         */
        function refreshToken() {
            var refreshToken = localStorage.getItem(authConfig.refreshTokenKey);
            
            if (!refreshToken) {
                return $q.reject({ message: 'No refresh token available' });
            }

            $log.debug('AuthService: Refreshing token');

            return apiService.post('auth/refresh', { refreshToken: refreshToken })
                .then(function(response) {
                    var tokenData = response.data;
                    
                    // Update stored token
                    localStorage.setItem(authConfig.tokenKey, tokenData.token);
                    
                    if (tokenData.refreshToken) {
                        localStorage.setItem(authConfig.refreshTokenKey, tokenData.refreshToken);
                    }

                    $log.debug('AuthService: Token refreshed successfully');

                    return tokenData.token;
                })
                .catch(function(error) {
                    $log.error('AuthService: Token refresh failed:', error);
                    
                    // Clear session on refresh failure
                    clearSession();
                    currentUser = null;
                    isLoggedIn = false;
                    
                    notifyAuthStateChange(false, null);

                    return $q.reject(error);
                });
        }

        /**
         * Check if user is authenticated
         * @returns {boolean} True if authenticated
         */
        function isAuthenticated() {
            return isLoggedIn && !!getToken() && !isSessionExpired();
        }

        /**
         * Get current authenticated user
         * @returns {Object|null} Current user object or null
         */
        function getCurrentUser() {
            return currentUser;
        }

        /**
         * Get authentication token
         * @returns {string|null} JWT token or null
         */
        function getToken() {
            return localStorage.getItem(authConfig.tokenKey);
        }

        /**
         * Get user's primary role
         * @returns {string|null} Primary role or null
         */
        function getRole() {
            if (!currentUser || !currentUser.role) {
                return null;
            }
            
            // Return primary role (first role if multiple)
            return Array.isArray(currentUser.role) ? currentUser.role[0] : currentUser.role;
        }

        /**
         * Get all user roles
         * @returns {Array} Array of roles
         */
        function getRoles() {
            if (!currentUser || !currentUser.role) {
                return [];
            }
            
            return Array.isArray(currentUser.role) ? currentUser.role : [currentUser.role];
        }

        /**
         * Check if user has specific role
         * @param {string} role - Role to check
         * @returns {boolean} True if user has role
         */
        function hasRole(role) {
            if (!role || !isAuthenticated()) {
                return false;
            }

            var userRoles = getRoles();
            return userRoles.indexOf(role) !== -1;
        }

        /**
         * Check if user has any of the specified roles
         * @param {Array} roles - Array of roles to check
         * @returns {boolean} True if user has any of the roles
         */
        function hasAnyRole(roles) {
            if (!Array.isArray(roles) || !isAuthenticated()) {
                return false;
            }

            var userRoles = getRoles();
            return roles.some(function(role) {
                return userRoles.indexOf(role) !== -1;
            });
        }

        /**
         * Check if user has specific permission
         * @param {string} permission - Permission to check
         * @returns {boolean} True if user has permission
         */
        function hasPermission(permission) {
            if (!permission || !isAuthenticated()) {
                return false;
            }

            // Check role-based permissions
            switch (permission) {
                case 'view_students':
                    return hasAnyRole(['Admin', 'Profesor']);
                case 'create_students':
                case 'edit_students':
                case 'delete_students':
                    return hasRole('Admin');
                case 'view_professors':
                case 'create_professors':
                case 'edit_professors':
                case 'delete_professors':
                    return hasRole('Admin');
                case 'view_notes':
                case 'create_notes':
                case 'edit_notes':
                    return hasAnyRole(['Admin', 'Profesor']);
                case 'delete_notes':
                    return hasRole('Admin');
                default:
                    return false;
            }
        }

        /**
         * Save authentication session
         * @param {string} token - JWT token
         * @param {string} refreshToken - Refresh token (optional)
         * @param {Object} user - User data
         */
        function saveSession(token, refreshToken, user) {
            try {
                localStorage.setItem(authConfig.tokenKey, token);
                localStorage.setItem(authConfig.userKey, JSON.stringify(user));
                
                if (refreshToken) {
                    localStorage.setItem(authConfig.refreshTokenKey, refreshToken);
                }

                $log.debug('AuthService: Session saved');
            } catch (error) {
                $log.error('AuthService: Failed to save session:', error);
            }
        }

        /**
         * Clear authentication session
         */
        function clearSession() {
            try {
                localStorage.removeItem(authConfig.tokenKey);
                localStorage.removeItem(authConfig.userKey);
                localStorage.removeItem(authConfig.refreshTokenKey);

                $log.debug('AuthService: Session cleared');
            } catch (error) {
                $log.error('AuthService: Failed to clear session:', error);
            }
        }

        /**
         * Load authentication session from storage
         */
        function loadSession() {
            try {
                var token = localStorage.getItem(authConfig.tokenKey);
                var userStr = localStorage.getItem(authConfig.userKey);

                if (token && userStr) {
                    currentUser = JSON.parse(userStr);
                    
                    // Check if session is still valid
                    if (!isSessionExpired()) {
                        isLoggedIn = true;
                        $log.debug('AuthService: Session loaded for user:', currentUser.username);
                    } else {
                        clearSession();
                        $log.debug('AuthService: Expired session cleared');
                    }
                }
            } catch (error) {
                $log.error('AuthService: Failed to load session:', error);
                clearSession();
            }
        }

        /**
         * Check if current session is expired
         * @returns {boolean} True if session is expired
         */
        function isSessionExpired() {
            var token = getToken();
            
            if (!token) {
                return true;
            }

            try {
                // Decode JWT token to check expiration
                var payload = JSON.parse(atob(token.split('.')[1]));
                var expirationTime = payload.exp * 1000; // Convert to milliseconds
                var currentTime = Date.now();
                var bufferTime = authConfig.tokenExpiryBuffer || 300000; // 5 minutes buffer

                return (expirationTime - bufferTime) <= currentTime;
            } catch (error) {
                $log.error('AuthService: Failed to decode token:', error);
                return true;
            }
        }

        /**
         * Register auth state change listener
         * @param {Function} callback - Callback function
         * @returns {Function} Unregister function
         */
        function onAuthStateChange(callback) {
            if (typeof callback === 'function') {
                authListeners.push(callback);
                
                return function() {
                    removeAuthListener(callback);
                };
            }
        }

        /**
         * Remove auth state change listener
         * @param {Function} callback - Callback function to remove
         */
        function removeAuthListener(callback) {
            var index = authListeners.indexOf(callback);
            if (index !== -1) {
                authListeners.splice(index, 1);
            }
        }

        /**
         * Notify all auth state change listeners
         * @param {boolean} authenticated - Authentication state
         * @param {Object} user - User data
         */
        function notifyAuthStateChange(authenticated, user) {
            authListeners.forEach(function(callback) {
                try {
                    callback(authenticated, user);
                } catch (error) {
                    $log.error('AuthService: Auth listener error:', error);
                }
            });
        }

        /**
         * Redirect to login page
         */
        function redirectToLogin() {
            // Store current path for redirect after login
            var currentPath = $window.location.hash || '#!/dashboard';
            if (currentPath !== '#!/login') {
                localStorage.setItem('redirectAfterLogin', currentPath);
            }
            
            // Navigate to login
            $window.location.hash = '#!/login';
        }
    }

})();