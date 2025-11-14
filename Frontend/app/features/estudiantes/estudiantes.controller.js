(function() {
    'use strict';

    /**
     * Estudiantes Controller
     * Manages student list view with CRUD operations, filtering, sorting, and pagination
     */
    angular
        .module('audiSoftSchoolApp')
        .controller('EstudiantesController', EstudiantesController);

    EstudiantesController.$inject = ['$scope', '$location', 'authService', 'estudiantesService', 'configService', '$log'];
    function EstudiantesController($scope, $location, authService, estudiantesService, configService, $log) {
        var vm = this;

        // UI State
        vm.isLoading = false;
        vm.error = null;
        vm.success = null;
        vm.showModal = false;
        vm.isEditMode = false;

        // Data
        vm.estudiantes = [];
        vm.totalCount = 0;
        vm.currentEstudiante = null;

        // Pagination
        vm.currentPage = 1;
        vm.pageSize = configService.getPaginationConfig().defaultPageSize;
        vm.totalPages = 0;
        vm.pageSizes = configService.getPaginationConfig().pageSizes;

        // Filtering & Sorting
        vm.searchFilters = {
            nombre: '',
            grado: ''
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

        // Bindable Methods
        vm.loadEstudiantes = loadEstudiantes;
        vm.openCreateModal = openCreateModal;
        vm.openEditModal = openEditModal;
        vm.saveEstudiante = saveEstudiante;
        vm.deleteEstudiante = deleteEstudiante;
        vm.closeModal = closeModal;
        vm.applyFilters = applyFilters;
        vm.clearFilters = clearFilters;
        vm.goToPage = goToPage;
        vm.changePageSize = changePageSize;
        vm.toggleSortOrder = toggleSortOrder;

        // Initialize
        activate();

        ////////////////

        function activate() {
            checkPermissions();
            loadEstudiantes();
        }

        /**
         * Check user permissions for CRUD operations
         */
        function checkPermissions() {
            // Only Admin can create/edit/delete students
            vm.canCreate = authService.hasRole('Admin');
            vm.canEdit = authService.hasRole('Admin');
            vm.canDelete = authService.hasRole('Admin');

            $log.debug('EstudiantesController: Permissions', {
                canCreate: vm.canCreate,
                canEdit: vm.canEdit,
                canDelete: vm.canDelete
            });
        }

        /**
         * Load estudiantes with current filters and pagination
         */
        function loadEstudiantes() {
            vm.isLoading = true;
            vm.error = null;

            var queryParams = {
                page: vm.currentPage,
                pageSize: vm.pageSize,
                filters: buildActiveFilters(),
                orderBy: vm.orderBy,
                orderDirection: vm.orderDirection
            };

            estudiantesService.getEstudiantes(queryParams)
                .then(function(result) {
                    // The service returns { data: [...], totalCount: N, page: X, pageSize: Y }
                    // We need to assign the array from result.data to vm.estudiantes
                    vm.estudiantes = result.data || [];
                    vm.totalCount = result.totalCount || 0;
                    vm.totalPages = vm.totalCount > 0 ? Math.ceil(vm.totalCount / vm.pageSize) : 0;

                    $log.info('EstudiantesController: Loaded', vm.estudiantes.length, 'estudiantes of', vm.totalCount, 'total');
                })
                .catch(function(error) {
                    vm.error = {
                        message: error && error.message ? error.message : 'Error cargando estudiantes',
                        status: error && error.status ? error.status : null
                    };
                    vm.estudiantes = [];
                    vm.totalCount = 0;
                    $log.error('EstudiantesController: Failed to load estudiantes', error);
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

            if (vm.searchFilters.grado && vm.searchFilters.grado.trim()) {
                filters.grado = vm.searchFilters.grado.trim();
            }

            return filters;
        }

        /**
         * Apply current filters and reset to page 1
         */
        function applyFilters() {
            vm.currentPage = 1;
            loadEstudiantes();
        }

        /**
         * Clear all filters
         */
        function clearFilters() {
            vm.searchFilters = {
                nombre: '',
                grado: ''
            };
            vm.currentPage = 1;
            loadEstudiantes();
        }

        /**
         * Open modal for creating new estudiante
         */
        function openCreateModal() {
            vm.isEditMode = false;
            vm.currentEstudiante = {
                nombre: '',
                grado: ''
            };
            vm.showModal = true;
        }

        /**
         * Open modal for editing existing estudiante
         * @param {Object} estudiante - Student to edit
         */
        function openEditModal(estudiante) {
            if (!vm.canEdit) {
                vm.error = { message: 'No tienes permisos para editar estudiantes' };
                return;
            }

            vm.isEditMode = true;
            vm.currentEstudiante = angular.copy(estudiante);
            vm.showModal = true;
        }

        /**
         * Save estudiante (create or update)
         */
        function saveEstudiante() {
            if (!vm.currentEstudiante.nombre || !vm.currentEstudiante.nombre.trim()) {
                vm.error = { message: 'El nombre es obligatorio' };
                return;
            }

            vm.isLoading = true;
            vm.error = null;

            var promise = vm.isEditMode
                ? estudiantesService.updateEstudiante(vm.currentEstudiante.id, vm.currentEstudiante)
                : estudiantesService.createEstudiante(vm.currentEstudiante);

            promise
                .then(function(result) {
                    var action = vm.isEditMode ? 'actualizado' : 'creado';
                    vm.success = { message: 'Estudiante ' + action + ' exitosamente' };

                    closeModal();
                    loadEstudiantes();

                    // Clear success message after 3 seconds
                    setTimeout(function() {
                        vm.success = null;
                    }, 3000);
                })
                .catch(function(error) {
                    vm.error = {
                        message: error && error.message ? error.message : 'Error guardando estudiante',
                        status: error && error.status ? error.status : null
                    };
                    $log.error('EstudiantesController: Failed to save estudiante', error);
                })
                .finally(function() {
                    vm.isLoading = false;
                });
        }

        /**
         * Delete estudiante with confirmation
         * @param {Object} estudiante - Student to delete
         */
        function deleteEstudiante(estudiante) {
            if (!vm.canDelete) {
                vm.error = { message: 'No tienes permisos para eliminar estudiantes' };
                return;
            }

            if (!confirm('¿Está seguro de que desea eliminar a ' + estudiante.nombre + '?')) {
                return;
            }

            vm.isLoading = true;
            vm.error = null;

            estudiantesService.deleteEstudiante(estudiante.id)
                .then(function() {
                    vm.success = { message: 'Estudiante eliminado exitosamente' };

                    loadEstudiantes();

                    // Clear success message after 3 seconds
                    setTimeout(function() {
                        vm.success = null;
                    }, 3000);
                })
                .catch(function(error) {
                    vm.error = {
                        message: error && error.message ? error.message : 'Error eliminando estudiante',
                        status: error && error.status ? error.status : null
                    };
                    $log.error('EstudiantesController: Failed to delete estudiante', error);
                })
                .finally(function() {
                    vm.isLoading = false;
                });
        }

        /**
         * Close modal and reset form
         */
        function closeModal() {
            vm.showModal = false;
            vm.isEditMode = false;
            vm.currentEstudiante = null;
            vm.error = null;
        }

        /**
         * Navigate to specific page
         * @param {number} page - Page number
         */
        function goToPage(page) {
            if (page >= 1 && page <= vm.totalPages) {
                vm.currentPage = page;
                loadEstudiantes();
            }
        }

        /**
         * Change page size and reset to page 1
         */
        function changePageSize() {
            vm.currentPage = 1;
            loadEstudiantes();
        }

        /**
         * Toggle sort order
         */
        function toggleSortOrder() {
            vm.orderDirection = vm.orderDirection === 'asc' ? 'desc' : 'asc';
            loadEstudiantes();
        }

        // Watch for enter key on search
        $scope.$watch(function() { return vm.searchFilters.nombre; }, function(newVal) {
            if (newVal && newVal.length > 2) {
                applyFilters();
            }
        }, true);
    }

})();
