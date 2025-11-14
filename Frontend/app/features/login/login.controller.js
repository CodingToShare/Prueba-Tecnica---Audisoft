(function() {
    'use strict';

    /**
     * Login Controller
     * Handles user authentication, form validation, and login flow
     */
    angular
        .module('audiSoftSchoolApp')
        .controller('LoginController', LoginController);

    LoginController.$inject = ['$scope', '$location', 'authService', 'configService'];
    function LoginController($scope, $location, authService, configService) {
        var vm = this;

        // Bindable properties
        vm.credentials = {
            username: '',
            password: ''
        };
        vm.loginForm = {};
        vm.isLoading = false;
        vm.error = null;
        vm.appInfo = configService.getAppInfo();

        // Bindable methods
        vm.login = login;
        vm.clearError = clearError;
        vm.isFormValid = isFormValid;

        // Initialize controller
        activate();

        ////////////////

        function activate() {
            // Redirect if already authenticated
            if (authService.isAuthenticated()) {
                var currentUser = authService.getCurrentUser();
                redirectBasedOnRole(currentUser);
                return;
            }

            // Clear any existing error state
            clearError();
            
            // Focus on username field
            setTimeout(function() {
                var usernameField = document.getElementById('username');
                if (usernameField) {
                    usernameField.focus();
                }
            }, 100);
        }

        /**
         * Authenticate user with provided credentials
         */
        function login() {
            if (!isFormValid()) {
                vm.error = {
                    message: 'Por favor, complete todos los campos requeridos',
                    type: 'validation'
                };
                return;
            }

            vm.isLoading = true;
            vm.error = null;

            authService.login(vm.credentials.username, vm.credentials.password)
                .then(function(result) {
                    vm.isLoading = false;
                    
                    // Show success message briefly
                    var userName = result.user.nombre || result.user.username || 'Usuario';
                    showSuccessMessage('Bienvenido, ' + userName);
                    
                    // Redirect based on user role
                    redirectBasedOnRole(result.user);
                })
                .catch(function(error) {
                    vm.isLoading = false;
                    vm.error = {
                        message: error.message || 'Error al iniciar sesión',
                        type: error.type || 'auth_error'
                    };
                    
                    // Focus back to username if validation error
                    if (error.status === 401) {
                        setTimeout(function() {
                            var usernameField = document.getElementById('username');
                            if (usernameField) {
                                usernameField.focus();
                                usernameField.select();
                            }
                        }, 100);
                    }
                });
        }

        /**
         * Clear current error message
         */
        function clearError() {
            vm.error = null;
        }

        /**
         * Check if login form is valid
         * @returns {boolean} True if form is valid
         */
        function isFormValid() {
            return vm.loginForm.loginForm && vm.loginForm.loginForm.$valid;
        }

        /**
         * Show success message
         * @param {string} message - Success message to display
         */
        function showSuccessMessage(message) {
            // Create temporary success element
            var successEl = document.createElement('div');
            successEl.className = 'alert alert-success alert-dismissible fade show position-fixed';
            successEl.style.top = '20px';
            successEl.style.right = '20px';
            successEl.style.zIndex = '9999';
            successEl.innerHTML = '<i class="bi bi-check-circle-fill me-2"></i>' + message;
            
            document.body.appendChild(successEl);
            
            // Remove after 3 seconds
            setTimeout(function() {
                if (successEl.parentNode) {
                    successEl.parentNode.removeChild(successEl);
                }
            }, 3000);
        }

        /**
         * Redirect user based on their role after successful login
         * @param {Object} user - User object with role information
         */
        function redirectBasedOnRole(user) {
            // Check for stored redirect URL first
            var storedRedirect = localStorage.getItem('redirectAfterLogin');
            if (storedRedirect && storedRedirect !== '#!/login') {
                localStorage.removeItem('redirectAfterLogin');
                $location.url(storedRedirect.substring(3));
                return;
            }
            
            // Redirect based on user role
            var userRole = authService.getRole();
            
            switch (userRole) {
                case 'Admin':
                    $location.path('/dashboard');
                    break;
                case 'Profesor':
                    $location.path('/dashboard'); // Profesores también van al dashboard
                    break;
                case 'Estudiante':
                    $location.path('/dashboard'); // Por ahora todos van al dashboard
                    break;
                default:
                    $location.path('/dashboard');
            }
        }

        // Watch for form validity changes
        $scope.$watch('login.loginForm.loginForm.$valid', function(isValid) {
            if (isValid && vm.error && vm.error.type === 'validation') {
                clearError();
            }
        });

        // Listen for Enter key press
        $scope.$on('$destroy', function() {
            // Clean up any event listeners if needed
        });
    }

})();