(function() {
    'use strict';

    /**
     * Notas Controller
     * Manages grades/notes view with CRUD operations filtered by user role
     * Admin: CRUD total, Profesor: ver/editar solo suyas, Estudiante: solo lectura de sus notas
     */
    angular
        .module('audiSoftSchoolApp')
        .controller('NotasController', NotasController);

        NotasController.$inject = ['$scope', '$location', 'authService', 'notasService', 'estudiantesService', 
                                                             'profesoresService', 'configService', '$log', 'toastService'];
    function NotasController($scope, $location, authService, notasService, estudiantesService, 
                                                         profesoresService, configService, $log, toastService) {
        var vm = this;

        // UI State
        vm.isLoading = false;
        vm.error = null;
        vm.success = null;
        vm.showModal = false;
        vm.isEditMode = false;
        vm.showDeleteConfirm = false;
        vm.isDeleting = false;
        vm.notaToDelete = { id: null, nombre: null, valor: null };

        // Data
        vm.notas = [];
        vm.totalCount = 0;
        vm.currentNota = null;
        vm.profesores = [];
        vm.estudiantes = [];
        vm.userRole = null;
        vm.currentUserId = null;

        // Pagination
        vm.currentPage = 1;
        vm.pageSize = configService.getPaginationConfig().defaultPageSize;
        vm.totalPages = 0;
        vm.pageSizes = configService.getPaginationConfig().pageSizes;

        // Filtering & Sorting
        vm.searchFilters = {
            nombre: '',
            idProfesor: '',
            idEstudiante: '',
            minValor: '',
            maxValor: ''
        };
        vm.orderBy = 'nombre';
        vm.orderDirection = 'asc';
        vm.availableSortFields = [
            { value: 'nombre', label: 'Nombre' },
            { value: 'valor', label: 'Valor' },
            { value: 'createdAt', label: 'Fecha Creación' },
            { value: 'id', label: 'ID' }
        ];

        // User Permissions
        vm.canCreate = false;
        vm.canEdit = false;
        vm.canDelete = false;
        vm.canViewAll = false;

        // Expose Math object to template
        vm.Math = Math;

        // Bindable Methods
        vm.loadNotas = loadNotas;
        vm.openCreateModal = openCreateModal;
        vm.openEditModal = openEditModal;
        vm.openEditModalById = openEditModalById;
        vm.saveNota = saveNota;
        vm.deleteNota = deleteNota;
        vm.openDeleteConfirm = openDeleteConfirm;
        vm.openDeleteConfirmById = openDeleteConfirmById;
        vm.closeDeleteConfirm = closeDeleteConfirm;
        vm.confirmDelete = confirmDelete;
        vm.closeModal = closeModal;
        vm.applyFilters = applyFilters;
        vm.clearFilters = clearFilters;
        vm.goToPage = goToPage;
        vm.changePageSize = changePageSize;
        vm.toggleSortOrder = toggleSortOrder;
        vm.setSort = setSort;

        // Table columns configuration for reusable table component (Estudiante + Grado en misma celda)
        vm.tableColumns = [
            { field: 'id', label: 'ID', width: '60px', sortable: true, type: 'code' },
            { field: 'nombre', label: 'Nombre', sortable: true },
            { field: 'valor', label: 'Valor', sortable: true, type: 'badge', badgeClass: 'bg-success' },
            { field: 'profesor', label: 'Profesor', sortable: false, visible: vm.canViewAll ? undefined : false, get: function(row){ return getProfesorNombre(row.idProfesor); }, type: 'small' },
            { field: 'estudiante', label: 'Estudiante', sortable: false, visible: vm.canViewAll ? undefined : false, get: function(row){ return { nombre: getEstudianteNombre(row.idEstudiante), grado: getEstudianteGrado(row.idEstudiante) }; }, type: 'estudiante' },
            { field: 'createdAt', label: 'Fecha Creación', sortable: true, get: function(row){ return $scope.$eval("row.createdAt | date:'short'", { row: row }); }, type: 'small' }
        ];

        // Initialize
        activate();

        ////////////////

        /**
         * Initialize controller
         */
        function activate() {
            $log.debug('NotasController: Activating');
            
            // Get current user info
            var user = authService.getCurrentUser();
            vm.userRole = user && user.roles ? user.roles[0] : null;
            vm.currentUserId = user && user.id ? user.id : null;
            // For Profesor role, get the idProfesor to compare with nota.idProfesor
            vm.currentProfesorId = user && user.idProfesor ? user.idProfesor : null;

            $log.debug('NotasController: User role =', vm.userRole, 'ID =', vm.currentUserId, 'ProfesorID =', vm.currentProfesorId);

            // Check permissions based on user role
            vm.canViewAll = authService.hasRole('Admin');
            vm.canCreate = authService.hasRole('Admin') || authService.hasRole('Profesor');
            vm.canEdit = authService.hasRole('Admin') || authService.hasRole('Profesor');
            vm.canDelete = authService.hasRole('Admin') || authService.hasRole('Profesor');

            $log.debug('NotasController: Permissions', {
                canCreate: vm.canCreate,
                canEdit: vm.canEdit,
                canDelete: vm.canDelete,
                canViewAll: vm.canViewAll
            });

            // Load reference data
            loadReferenceData();
            loadNotas();
            
            // Watch for page size changes
            $scope.$watch(function() { return vm.pageSize; }, function(newVal, oldVal) {
                if (newVal !== oldVal && newVal) {
                    $log.debug('NotasController: Page size changed from ' + oldVal + ' to ' + newVal);
                    vm.currentPage = 1;
                    loadNotas();
                }
            });
        }

        /**
         * Load profesores and estudiantes for dropdowns
         */
        function loadReferenceData() {
            $log.debug('NotasController: Loading reference data');

            // Load profesores
            profesoresService.getProfesores({
                page: 1,
                pageSize: 1000,
                orderBy: 'nombre',
                orderDirection: 'asc'
            })
                .then(function(result) {
                    // Handle both array and PagedResult formats
                    var items = (result.data && Array.isArray(result.data)) ? result.data : 
                               (result.items && Array.isArray(result.items)) ? result.items :
                               (Array.isArray(result)) ? result : [];
                    vm.profesores = items;
                    $log.debug('NotasController: Loaded ' + vm.profesores.length + ' profesores');
                })
                .catch(function(error) {
                    $log.error('NotasController: Error loading profesores', error);
                    vm.profesores = [];
                });

            // Load estudiantes (parse nombre to extract grado like in EstudiantesController)
            estudiantesService.getEstudiantes({
                page: 1,
                pageSize: 1000,
                orderBy: 'nombre',
                orderDirection: 'asc'
            })
            .then(function(result) {
                // result.data is the array, or result.items if it's a PagedResult
                var items = (result.data && Array.isArray(result.data)) ? result.data : 
                           (result.items && Array.isArray(result.items)) ? result.items :
                           (Array.isArray(result)) ? result : [];
                
                if (items.length === 0) {
                    $log.warn('NotasController: No estudiantes loaded, result:', result);
                    vm.estudiantes = [];
                    return;
                }
                
                vm.estudiantes = items.map(function(est) {
                    if (!est || typeof est !== 'object') {
                        $log.warn('NotasController: Invalid estudiante object:', est);
                        return null;
                    }
                    var gradoMatch = est.nombre && est.nombre.match(/-\s*(\d+)°?/);
                    var grado = (gradoMatch && gradoMatch[1]) ? (gradoMatch[1] + '°') : '';
                    var nombreSinGrado = est.nombre ? est.nombre.replace(/-\s*\d+°?/, '').trim() : '';
                    return {
                        id: est.id,
                        nombre: nombreSinGrado,
                        nombreSinGrado: nombreSinGrado,
                        grado: grado
                    };
                }).filter(function(est) { return est !== null; }); // Remove null entries
                
                $log.debug('NotasController: Loaded ' + vm.estudiantes.length + ' estudiantes');
            })
            .catch(function(error) {
                $log.error('NotasController: Error loading estudiantes', error);
                vm.estudiantes = [];
            });
        }

        function getProfesorNombre(idProfesor) {
            var match = (vm.profesores || []).find(function(p){ return p.id === idProfesor; });
            return match ? match.nombre : 'N/A';
        }

        function getEstudianteNombre(idEstudiante) {
            var match = (vm.estudiantes || []).find(function(e){ return e.id === idEstudiante; });
            return match ? (match.nombreSinGrado || match.nombre) : 'N/A';
        }

        function getEstudianteGrado(idEstudiante) {
            var match = (vm.estudiantes || []).find(function(e){ return e.id === idEstudiante; });
            return match ? match.grado : '';
        }

        /**
         * Load notas with current filters and pagination
         */
        function loadNotas() {
            vm.isLoading = true;
            vm.error = null;

            var queryParams = {
                page: vm.currentPage,
                pageSize: vm.pageSize,
                filter: buildActiveFilters(),
                orderBy: vm.orderBy,
                orderDirection: vm.orderDirection
            };

            notasService.getNotas(queryParams)
                .then(function(result) {
                    vm.notas = result.data || result.items || result;
                    vm.totalCount = result.totalCount || 0;
                    vm.totalPages = result.totalPages || Math.ceil(vm.totalCount / vm.pageSize);

                    $log.info('NotasController: Fetched ' + vm.notas.length + ' notas de ' + vm.totalCount + ' total');
                    
                    vm.isLoading = false;
                })
                .catch(function(error) {
                    vm.isLoading = false;
                    vm.error = {
                        message: error && error.message ? error.message : 'Error cargando notas',
                        status: error && error.status ? error.status : null
                    };
                    $log.error('NotasController: Error loading notas', error);
                });
        }

        /**
         * Open modal for creating new nota
         */
        function openCreateModal() {
            if (!vm.canCreate) {
                vm.error = { message: 'No tienes permisos para crear notas' };
                return;
            }

            vm.isEditMode = false;
            vm.currentNota = {
                nombre: '',
                valor: '',
                idProfesor: null,
                idEstudiante: null
            };
            vm.showModal = true;
            vm.error = null;

            $log.debug('NotasController: Opening create modal');
        }

        /**
         * Open modal for editing existing nota
         */
        function openEditModal(nota) {
            if (!vm.canEdit) {
                vm.error = { message: 'No tienes permisos para editar notas' };
                return;
            }

            // Check if user can edit this nota (only own notes for Profesor)
            if (vm.userRole === 'Profesor' && nota.idProfesor !== vm.currentProfesorId) {
                vm.error = { message: 'Solo puedes editar tus propias notas' };
                return;
            }

            vm.isEditMode = true;
            vm.currentNota = angular.copy(nota);
            vm.showModal = true;
            vm.error = null;

            $log.debug('NotasController: Opening edit modal for nota', nota.id);
        }

        /**
         * Open edit modal by ID (retrieves full nota from list)
         */
        function openEditModalById(notaId) {
            var fullNota = (vm.notas || []).find(function(n) { return n.id === notaId; });
            if (fullNota) {
                openEditModal(fullNota);
            } else {
                $log.warn('NotasController: Nota not found for ID', notaId);
                vm.error = { message: 'Nota no encontrada' };
            }
        }

        /**
         * Save nota (create or update)
         */
        function saveNota() {
            // Access the form from $scope
            var form = $scope.notaForm;
            
            // Mark all fields as touched to show validation errors
            if (form) {
                form.$setSubmitted();
                
                // If form is invalid, show error and return
                if (form.$invalid) {
                    vm.error = { 
                        message: 'Por favor completa todos los campos correctamente' 
                    };
                    $log.warn('NotasController: Form is invalid', form.$error);
                    return;
                }
            }

            vm.isLoading = true;
            vm.error = null;

            var promise = vm.isEditMode 
                ? notasService.updateNota(vm.currentNota.id, vm.currentNota)
                : notasService.createNota(vm.currentNota);

            promise
                .then(function(result) {
                    var successMsg = vm.isEditMode ? 'Nota actualizada exitosamente' : 'Nota creada exitosamente';
                    vm.success = { message: successMsg };
                    toastService.success(successMsg);
                    closeModal();
                    loadNotas();

                    // Clear success message after 3 seconds
                    setTimeout(function() {
                        $scope.$apply(function() {
                            vm.success = null;
                        });
                    }, 3000);
                })
                .catch(function(error) {
                    vm.isLoading = false;
                    var errMsg = (error && (error.message || (error.data && error.data.message))) || 'Error guardando nota';
                    vm.error = { message: errMsg, status: error && error.status ? error.status : null };
                    toastService.error(errMsg);
                    $log.error('NotasController: Error saving nota', error);
                });
        }

        /**
         * Delete nota (calls confirmation modal first)
         */
        function deleteNota(nota) {
            openDeleteConfirm(nota);
        }

        /**
         * Open delete confirmation modal
         */
        function openDeleteConfirm(nota) {
            if (!vm.canDelete) {
                vm.error = { message: 'No tienes permisos para eliminar notas' };
                return;
            }

            // Check if user can delete this nota (only own notes for Profesor)
            if (vm.userRole === 'Profesor' && nota.idProfesor !== vm.currentProfesorId) {
                vm.error = { message: 'Solo puedes eliminar tus propias notas' };
                return;
            }

            // Get the complete nota object from vm.notas to ensure all fields are present
            // Use nota.id to find the complete object with all properties (including valor)
            var notaId = nota.id;
            var fullNota = (vm.notas || []).find(function(n) { 
                return n.id === notaId; 
            });
            
            // Ensure we have all fields, especially valor
            vm.notaToDelete = fullNota ? angular.copy(fullNota) : angular.copy(nota);
            
            $log.debug('NotasController: Delete confirm - notaToDelete:', vm.notaToDelete);
            vm.showDeleteConfirm = true;
        }

        /**
         * Open delete confirmation modal by ID (retrieves full nota from list)
         */
        function openDeleteConfirmById(notaId) {
            // Find the complete nota from vm.notas by ID
            var fullNota = (vm.notas || []).find(function(n) { return n.id === notaId; });
            
            if (fullNota) {
                // Call the regular openDeleteConfirm with the full nota object
                openDeleteConfirm(fullNota);
            } else {
                $log.warn('NotasController: Nota not found for ID', notaId);
                vm.error = { message: 'Nota no encontrada' };
            }
        }

        /**
         * Close delete confirmation modal
         */
        function closeDeleteConfirm() {
            vm.showDeleteConfirm = false;
            vm.notaToDelete = { id: null, nombre: null, valor: null };
            vm.isDeleting = false;
        }

        /**
         * Confirm and execute delete
         */
        function confirmDelete() {
            if (!vm.notaToDelete || vm.isDeleting) {
                return;
            }

            vm.isDeleting = true;
            vm.error = null;

            notasService.deleteNota(vm.notaToDelete.id)
                .then(function() {
                    vm.success = { message: 'Nota eliminada exitosamente' };
                    toastService.success('Nota eliminada exitosamente');
                    closeDeleteConfirm();
                    loadNotas();

                    // Clear success message after 3 seconds
                    setTimeout(function() {
                        $scope.$apply(function() {
                            vm.success = null;
                        });
                    }, 3000);
                })
                .catch(function(error) {
                    vm.isDeleting = false;
                    var delErr = (error && (error.message || (error.data && error.data.message))) || 'Error eliminando nota';
                    vm.error = { message: delErr, status: error && error.status ? error.status : null };
                    toastService.error(delErr);
                    $log.error('NotasController: Error deleting nota', error);
                });
        }

        /**
         * Close modal and reset form
         */
        function closeModal() {
            vm.showModal = false;
            vm.isEditMode = false;
            vm.currentNota = null;
            vm.error = null;
        }

        /**
         * Build active filters for API request
         */
        function buildActiveFilters() {
            var filters = {};

            if (vm.searchFilters.nombre) {
                filters.nombre = vm.searchFilters.nombre;
            }

            if (vm.searchFilters.idProfesor) {
                filters.idProfesor = parseInt(vm.searchFilters.idProfesor, 10);
            }

            if (vm.searchFilters.idEstudiante) {
                filters.idEstudiante = parseInt(vm.searchFilters.idEstudiante, 10);
            }

            if (vm.searchFilters.minValor !== '') {
                filters.minValor = parseFloat(vm.searchFilters.minValor);
            }

            if (vm.searchFilters.maxValor !== '') {
                filters.maxValor = parseFloat(vm.searchFilters.maxValor);
            }

            return notasService.buildFilterQuery(filters);
        }

        /**
         * Apply filters and reload data
         */
        function applyFilters() {
            vm.currentPage = 1;
            loadNotas();
        }

        /**
         * Clear all filters
         */
        function clearFilters() {
            vm.searchFilters = {
                nombre: '',
                idProfesor: '',
                idEstudiante: '',
                minValor: '',
                maxValor: ''
            };
            applyFilters();
        }

        /**
         * Navigate to specific page
         */
        function goToPage(page) {
            if (page >= 1 && page <= vm.totalPages) {
                vm.currentPage = page;
                loadNotas();
            }
        }

        /**
         * Change page size
         */
        function changePageSize() {
            vm.currentPage = 1;
            loadNotas();
        }

        /**
         * Toggle sort order direction
         */
        function toggleSortOrder() {
            vm.orderDirection = vm.orderDirection === 'asc' ? 'desc' : 'asc';
            loadNotas();
        }

        function setSort(field) {
            if (vm.orderBy === field) {
                vm.orderDirection = vm.orderDirection === 'asc' ? 'desc' : 'asc';
            } else {
                vm.orderBy = field;
                vm.orderDirection = 'asc';
            }
            loadNotas();
        }
    }

})();
