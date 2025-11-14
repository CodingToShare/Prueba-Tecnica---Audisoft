(function() {
    'use strict';

    /**
     * Profesores Controller
     * Manages teacher list view with CRUD operations, filtering, sorting, and pagination
     */
    angular
        .module('audiSoftSchoolApp')
        .controller('ProfesoresController', ProfesoresController);

    ProfesoresController.$inject = ['$scope', '$location', 'authService', 'profesoresService', 'configService', '$log'];
    function ProfesoresController($scope, $location, authService, profesoresService, configService, $log) {
        var vm = this;

        // UI State
        vm.isLoading = false;
        vm.error = null;
        vm.success = null;
        vm.showModal = false;
        vm.isEditMode = false;
        vm.showDeleteConfirm = false;
        vm.isDeleting = false;
        vm.profesorToDelete = null;

        // Data
        vm.profesores = [];
        vm.totalCount = 0;
        vm.currentProfesor = null;

        // Pagination
        vm.currentPage = 1;
        vm.pageSize = configService.getPaginationConfig().defaultPageSize;
        vm.totalPages = 0;
        vm.pageSizes = configService.getPaginationConfig().pageSizes;

        // Filtering & Sorting
        vm.searchFilters = {
            nombre: ''
        };
        vm.orderBy = 'nombre';
        vm.orderDirection = 'asc';
        vm.availableSortFields = [
            { value: 'nombre', label: 'Nombre' },
            { value: 'createdAt', label: 'Fecha Creación' },
            { value: 'id', label: 'ID' }
        ];

        // User Permissions
        vm.canCreate = false;
        vm.canEdit = false;
        vm.canDelete = false;

        // Expose Math object to template
        vm.Math = Math;

        // Bindable Methods
        vm.loadProfesores = loadProfesores;
        vm.openCreateModal = openCreateModal;
        vm.openEditModal = openEditModal;
        vm.saveProfesor = saveProfesor;
        vm.deleteProfesor = deleteProfesor;
        vm.openDeleteConfirm = openDeleteConfirm;
        vm.closeDeleteConfirm = closeDeleteConfirm;
        vm.confirmDelete = confirmDelete;
        vm.closeModal = closeModal;
        vm.applyFilters = applyFilters;
        vm.clearFilters = clearFilters;
        vm.goToPage = goToPage;
        vm.changePageSize = changePageSize;
        vm.toggleSortOrder = toggleSortOrder;

        // Initialize
        activate();

        ////////////////

        /**
         * Initialize controller
         */
        function activate() {
            $log.debug('ProfesoresController: Activating');
            
            // Check permissions based on user role
            var userRoles = authService.getRoles() || [];
            vm.canCreate = userRoles.indexOf('Admin') >= 0;
            vm.canEdit = userRoles.indexOf('Admin') >= 0;
            vm.canDelete = userRoles.indexOf('Admin') >= 0;

            $log.info('ProfesoresController: Permissions', { canCreate: vm.canCreate, canEdit: vm.canEdit, canDelete: vm.canDelete });

            loadProfesores();
        }

        /**
         * Load paginated list of profesores
         */
        function loadProfesores() {
            vm.isLoading = true;
            vm.error = null;

            var queryParams = {
                page: vm.currentPage,
                pageSize: vm.pageSize,
                filters: buildActiveFilters(),
                orderBy: vm.orderBy,
                orderDirection: vm.orderDirection
            };

            profesoresService.getProfesores(queryParams)
                .then(function(result) {
                    // The service returns { data: [...], totalCount: N, page: X, pageSize: Y }
                    // We need to assign the array from result.data to vm.profesores
                    vm.profesores = result.data || [];
                    vm.totalCount = result.totalCount || 0;
                    vm.totalPages = vm.totalCount > 0 ? Math.ceil(vm.totalCount / vm.pageSize) : 0;

                    $log.info('ProfesoresController: Loaded', vm.profesores.length, 'profesores of', vm.totalCount, 'total');
                })
                .catch(function(error) {
                    vm.error = {
                        message: error && error.message ? error.message : 'Error cargando profesores',
                        status: error && error.status ? error.status : null
                    };
                    vm.profesores = [];
                    vm.totalCount = 0;
                    $log.error('ProfesoresController: Failed to load profesores', error);
                })
                .finally(function() {
                    vm.isLoading = false;
                });
        }

        /**
         * Build active filter object from search input
         * @returns {Object} Active filters
         */
        function buildActiveFilters() {
            var filters = {};

            if (vm.searchFilters.nombre && vm.searchFilters.nombre.trim()) {
                filters.nombre = vm.searchFilters.nombre.trim();
            }

            return filters;
        }

        /**
         * Apply current filters and reset to page 1
         */
        function applyFilters() {
            vm.currentPage = 1;
            loadProfesores();
        }

        /**
         * Clear all filters
         */
        function clearFilters() {
            vm.searchFilters = {
                nombre: ''
            };
            vm.currentPage = 1;
            loadProfesores();
        }

        /**
         * Open create modal
         */
        function openCreateModal() {
            if (!vm.canCreate) {
                vm.error = { message: 'No tienes permisos para crear profesores' };
                return;
            }

            vm.isEditMode = false;
            vm.currentProfesor = { nombre: '' };
            vm.showModal = true;
        }

        /**
         * Open edit modal
         * @param {Object} profesor - Teacher to edit
         */
        function openEditModal(profesor) {
            if (!vm.canEdit) {
                vm.error = { message: 'No tienes permisos para editar profesores' };
                return;
            }

            vm.isEditMode = true;
            vm.currentProfesor = angular.copy(profesor);
            vm.showModal = true;
        }

        /**
         * Save profesor (create or update)
         */
        function saveProfesor() {
            if (!vm.currentProfesor.nombre || !vm.currentProfesor.nombre.trim()) {
                vm.error = { message: 'El nombre es obligatorio' };
                return;
            }

            vm.isLoading = true;
            vm.error = null;

            // Limpiar el nombre: trim
            var nombreLimpio = vm.currentProfesor.nombre.trim();

            // Validar que el nombre cumpla con la expresión regular del backend
            // ^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$ (letras y espacios)
            var nombreRegex = /^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$/;
            if (!nombreRegex.test(nombreLimpio)) {
                vm.error = { message: 'El nombre solo puede contener letras y espacios' };
                vm.isLoading = false;
                $log.warn('ProfesoresController: Nombre no válido:', nombreLimpio);
                return;
            }

            var profesorData = {
                nombre: nombreLimpio
            };

            var promise = vm.isEditMode
                ? profesoresService.updateProfesor(vm.currentProfesor.id, profesorData)
                : profesoresService.createProfesor(profesorData);

            promise
                .then(function(result) {
                    var action = vm.isEditMode ? 'actualizado' : 'creado';
                    vm.success = { message: 'Profesor ' + action + ' exitosamente' };

                    closeModal();
                    loadProfesores();

                    // Clear success message after 3 seconds
                    setTimeout(function() {
                        vm.success = null;
                    }, 3000);
                })
                .catch(function(error) {
                    vm.error = {
                        message: error && error.message ? error.message : 'Error guardando profesor',
                        status: error && error.status ? error.status : null
                    };
                    $log.error('ProfesoresController: Failed to save profesor', error);
                })
                .finally(function() {
                    vm.isLoading = false;
                });
        }

        /**
         * Delete profesor with confirmation
         * @param {Object} profesor - Teacher to delete
         */
        function deleteProfesor(profesor) {
            // Este método ya no se usa, solo para backward compatibility
            openDeleteConfirm(profesor);
        }

        function openDeleteConfirm(profesor) {
            if (!vm.canDelete) {
                vm.error = { message: 'No tienes permisos para eliminar profesores' };
                return;
            }

            vm.profesorToDelete = profesor;
            vm.showDeleteConfirm = true;
        }

        function closeDeleteConfirm() {
            vm.showDeleteConfirm = false;
            vm.profesorToDelete = null;
            vm.isDeleting = false;
        }

        function confirmDelete() {
            if (!vm.profesorToDelete || vm.isDeleting) {
                return;
            }

            vm.isDeleting = true;
            vm.error = null;

            profesoresService.deleteProfesor(vm.profesorToDelete.id)
                .then(function() {
                    vm.success = { message: 'Profesor eliminado exitosamente' };
                    closeDeleteConfirm();

                    loadProfesores();

                    // Clear success message after 3 seconds
                    setTimeout(function() {
                        $scope.$apply(function() {
                            vm.success = null;
                        });
                    }, 3000);
                })
                .catch(function(error) {
                    vm.isDeleting = false;
                    vm.error = {
                        message: error && error.message ? error.message : 'Error eliminando profesor',
                        status: error && error.status ? error.status : null
                    };
                    $log.error('ProfesoresController: Failed to delete profesor', error);
                });
        }

        /**
         * Close modal
         */
        function closeModal() {
            vm.showModal = false;
            vm.isEditMode = false;
            vm.currentProfesor = null;
            vm.error = null;
        }

        /**
         * Go to specific page
         * @param {number} pageNumber - Page to go to
         */
        function goToPage(pageNumber) {
            if (pageNumber >= 1 && pageNumber <= vm.totalPages) {
                vm.currentPage = pageNumber;
                loadProfesores();
            }
        }

        /**
         * Change page size
         */
        function changePageSize() {
            vm.currentPage = 1;
            loadProfesores();
        }

        /**
         * Toggle sort order
         */
        function toggleSortOrder() {
            vm.orderDirection = vm.orderDirection === 'asc' ? 'desc' : 'asc';
            loadProfesores();
        }
    }

})();
