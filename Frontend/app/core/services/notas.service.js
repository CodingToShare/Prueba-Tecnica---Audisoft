(function() {
    'use strict';

    /**
     * Notas Service
     * Handles all API interactions for grade/note management
     * Supports advanced filtering, sorting, and pagination
     * Role-based access: Admin (full), Profesor (own notes), Estudiante (own grades)
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('notasService', notasService);

    notasService.$inject = ['$q', '$log', 'apiService', 'configService'];
    function notasService($q, $log, apiService, configService) {
        
        var paginationConfig = configService.getPaginationConfig();
        
        var service = {
            // List operations
            getNotas: getNotas,
            getNotaById: getNotaById,
            getNotasByProfesor: getNotasByProfesor,
            getNotasByEstudiante: getNotasByEstudiante,
            
            // Create/Update/Delete operations
            createNota: createNota,
            updateNota: updateNota,
            deleteNota: deleteNota,
            
            // Search and filter
            searchNotas: searchNotas,
            buildFilterQuery: buildFilterQuery
        };

        return service;

        ////////////////

        /**
         * Get paginated list of notas with filtering and sorting
         * Automatically filters by role (Admin sees all, Profesor/Estudiante see only theirs)
         * @param {Object} queryParams - Query parameters (page, pageSize, filters, sorting)
         * @returns {Promise} Promise containing paginated notas
         */
        function getNotas(queryParams) {
            $log.debug('NotasService: Fetching notas', queryParams);
            
            var params = buildQueryParams(queryParams);
            var url = 'Notas';
            var page = queryParams ? queryParams.page || 1 : 1;
            var pageSize = queryParams ? queryParams.pageSize || paginationConfig.defaultPageSize : paginationConfig.defaultPageSize;

            return apiService.get(url, params)
                .then(function(response) {
                    $log.debug('NotasService: Full response:', response);
                    
                    // Extract totalCount from X-Total-Count header or from response
                    var totalCount = 0;
                    if (response.headers && typeof response.headers === 'object') {
                        totalCount = response.headers['x-total-count'] || response.headers['X-Total-Count'];
                        if (totalCount) {
                            totalCount = parseInt(totalCount, 10);
                        } else {
                            totalCount = 0;
                        }
                    }
                    
                    // The response.data contains the API response directly
                    var responseData = response.data;
                    var items = [];
                    
                    $log.debug('NotasService: responseData:', responseData);
                    $log.debug('NotasService: responseData type:', typeof responseData);
                    $log.debug('NotasService: responseData.items:', responseData ? responseData.items : 'N/A');
                    
                    if (responseData && responseData.items) {
                        // Format 1: Direct PagedResult structure with items array
                        items = responseData.items;
                        if (responseData.totalCount && !totalCount) {
                            totalCount = responseData.totalCount;
                        }
                    } else if (Array.isArray(responseData)) {
                        // Format 2: Direct array
                        items = responseData;
                    } else {
                        $log.warn('NotasService: Unexpected response format:', responseData);
                    }
                    
                    var result = {
                        data: items || [],
                        totalCount: totalCount,
                        page: page,
                        pageSize: pageSize,
                        totalPages: totalCount > 0 ? Math.ceil(totalCount / pageSize) : 0,
                        hasPreviousPage: page > 1,
                        hasNextPage: (page * pageSize) < totalCount,
                        items: items || []
                    };
                    
                    $log.info('NotasService: Fetched ' + items.length + ' de ' + totalCount + ' notas');
                    
                    return result;
                })
                .catch(function(error) {
                    $log.error('NotasService: Error fetching notas', error);
                    return $q.reject(error);
                });
        }

        /**
         * Get a single nota by ID
         * @param {number} id - Nota ID
         * @returns {Promise} Promise containing nota data
         */
        function getNotaById(id) {
            $log.debug('NotasService: Fetching nota by ID', id);
            
            var url = 'Notas/' + id;

            return apiService.get(url)
                .then(function(response) {
                    var data = response.data || response;
                    $log.info('NotasService: Fetched nota with ID ' + id);
                    return data;
                })
                .catch(function(error) {
                    $log.error('NotasService: Error fetching nota', error);
                    return $q.reject(error);
                });
        }

        /**
         * Get all notas for a specific profesor
         * @param {number} idProfesor - Profesor ID
         * @param {Object} queryParams - Query parameters
         * @returns {Promise} Promise containing profesor's notas
         */
        function getNotasByProfesor(idProfesor, queryParams) {
            $log.debug('NotasService: Fetching notas for profesor', idProfesor);
            
            var params = buildQueryParams(queryParams);
            var url = 'Notas/profesor/' + idProfesor;
            var page = queryParams ? queryParams.page || 1 : 1;
            var pageSize = queryParams ? queryParams.pageSize || paginationConfig.defaultPageSize : paginationConfig.defaultPageSize;

            return apiService.get(url, params)
                .then(function(response) {
                    $log.debug('NotasService: Full response:', response);
                    
                    // Extract totalCount from X-Total-Count header or from response
                    var totalCount = 0;
                    if (response.headers && typeof response.headers === 'object') {
                        totalCount = response.headers['x-total-count'] || response.headers['X-Total-Count'];
                        if (totalCount) {
                            totalCount = parseInt(totalCount, 10);
                        } else {
                            totalCount = 0;
                        }
                    }
                    
                    // The response.data contains the API response directly
                    var responseData = response.data;
                    var items = [];
                    
                    if (responseData && responseData.items) {
                        // Format 1: Direct PagedResult structure with items array
                        items = responseData.items;
                        if (responseData.totalCount && !totalCount) {
                            totalCount = responseData.totalCount;
                        }
                    } else if (Array.isArray(responseData)) {
                        // Format 2: Direct array
                        items = responseData;
                    } else {
                        $log.warn('NotasService: Unexpected response format:', responseData);
                    }
                    
                    var result = {
                        data: items || [],
                        totalCount: totalCount,
                        page: page,
                        pageSize: pageSize,
                        totalPages: totalCount > 0 ? Math.ceil(totalCount / pageSize) : 0,
                        hasPreviousPage: page > 1,
                        hasNextPage: (page * pageSize) < totalCount,
                        items: items || []
                    };
                    
                    $log.info('NotasService: Fetched ' + items.length + ' de ' + totalCount + 
                        ' notas for profesor ' + idProfesor);
                    
                    return result;
                })
                .catch(function(error) {
                    $log.error('NotasService: Error fetching notas for profesor', error);
                    return $q.reject(error);
                });
        }

        /**
         * Get all notas for a specific estudiante
         * @param {number} idEstudiante - Estudiante ID
         * @param {Object} queryParams - Query parameters
         * @returns {Promise} Promise containing estudiante's notas
         */
        function getNotasByEstudiante(idEstudiante, queryParams) {
            $log.debug('NotasService: Fetching notas for estudiante', idEstudiante);
            
            var params = buildQueryParams(queryParams);
            var url = 'Notas/estudiante/' + idEstudiante;
            var page = queryParams ? queryParams.page || 1 : 1;
            var pageSize = queryParams ? queryParams.pageSize || paginationConfig.defaultPageSize : paginationConfig.defaultPageSize;

            return apiService.get(url, params)
                .then(function(response) {
                    $log.debug('NotasService: Full response:', response);
                    
                    // Extract totalCount from X-Total-Count header or from response
                    var totalCount = 0;
                    if (response.headers && typeof response.headers === 'object') {
                        totalCount = response.headers['x-total-count'] || response.headers['X-Total-Count'];
                        if (totalCount) {
                            totalCount = parseInt(totalCount, 10);
                        } else {
                            totalCount = 0;
                        }
                    }
                    
                    // The response.data contains the API response directly
                    var responseData = response.data;
                    var items = [];
                    
                    if (responseData && responseData.items) {
                        // Format 1: Direct PagedResult structure with items array
                        items = responseData.items;
                        if (responseData.totalCount && !totalCount) {
                            totalCount = responseData.totalCount;
                        }
                    } else if (Array.isArray(responseData)) {
                        // Format 2: Direct array
                        items = responseData;
                    } else {
                        $log.warn('NotasService: Unexpected response format:', responseData);
                    }
                    
                    var result = {
                        data: items || [],
                        totalCount: totalCount,
                        page: page,
                        pageSize: pageSize,
                        totalPages: totalCount > 0 ? Math.ceil(totalCount / pageSize) : 0,
                        hasPreviousPage: page > 1,
                        hasNextPage: (page * pageSize) < totalCount,
                        items: items || []
                    };
                    
                    $log.info('NotasService: Fetched ' + items.length + ' de ' + totalCount + 
                        ' notas for estudiante ' + idEstudiante);
                    
                    return result;
                })
                .catch(function(error) {
                    $log.error('NotasService: Error fetching notas for estudiante', error);
                    return $q.reject(error);
                });
        }

        /**
         * Create a new nota
         * @param {Object} data - Nota data (nombre, valor, idProfesor, idEstudiante)
         * @returns {Promise} Promise containing created nota
         */
        function createNota(data) {
            $log.debug('NotasService: Creating nota', data);
            
            // Validate required fields
            if (!data) {
                var error = {
                    message: 'Datos de nota incompletos',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - data is null/undefined');
                return $q.reject(error);
            }

            // Validate nombre
            if (!data.nombre || !data.nombre.toString().trim()) {
                var errorNombre = {
                    message: 'El nombre de la nota es obligatorio',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - nombre is missing or empty');
                return $q.reject(errorNombre);
            }

            // Validate valor
            var valor = parseFloat(data.valor);
            if (isNaN(valor) || valor < 0 || valor > 100) {
                var errorValor = {
                    message: 'El valor debe estar entre 0 y 100',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - valor is invalid:', data.valor);
                return $q.reject(errorValor);
            }

            // Validate idProfesor
            var idProfesor = parseInt(data.idProfesor, 10);
            if (isNaN(idProfesor) || idProfesor <= 0) {
                var errorProfesor = {
                    message: 'Debes seleccionar un profesor válido',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - idProfesor is invalid:', data.idProfesor);
                return $q.reject(errorProfesor);
            }

            // Validate idEstudiante
            var idEstudiante = parseInt(data.idEstudiante, 10);
            if (isNaN(idEstudiante) || idEstudiante <= 0) {
                var errorEstudiante = {
                    message: 'Debes seleccionar un estudiante válido',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - idEstudiante is invalid:', data.idEstudiante);
                return $q.reject(errorEstudiante);
            }

            var payload = {
                nombre: data.nombre.toString().trim(),
                valor: valor,
                idProfesor: idProfesor,
                idEstudiante: idEstudiante
            };

            $log.debug('NotasService: Creating nota with payload', payload);

            var url = 'Notas';

            return apiService.post(url, payload)
                .then(function(response) {
                    var nota = response.data || response;
                    $log.info('NotasService: Nota created successfully with ID ' + nota.id);
                    return nota;
                })
                .catch(function(error) {
                    var message = error && error.data && error.data.message 
                        ? error.data.message 
                        : 'Error creando nota';
                    $log.error('NotasService: Error creating nota', error);
                    return $q.reject({
                        message: message,
                        status: error.status || 500,
                        data: error.data
                    });
                });
        }

        /**
         * Update an existing nota
         * @param {number} id - Nota ID
         * @param {Object} data - Updated nota data
         * @returns {Promise} Promise containing updated nota
         */
        function updateNota(id, data) {
            $log.debug('NotasService: Updating nota', id, data);
            
            // Validate required fields
            if (!data) {
                var error = {
                    message: 'Datos de nota incompletos',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - data is null/undefined');
                return $q.reject(error);
            }

            // Validate nombre
            if (!data.nombre || !data.nombre.toString().trim()) {
                var errorNombre = {
                    message: 'El nombre de la nota es obligatorio',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - nombre is missing or empty');
                return $q.reject(errorNombre);
            }

            // Validate valor
            var valor = parseFloat(data.valor);
            if (isNaN(valor) || valor < 0 || valor > 100) {
                var errorValor = {
                    message: 'El valor debe estar entre 0 y 100',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - valor is invalid:', data.valor);
                return $q.reject(errorValor);
            }

            // Validate idProfesor
            var idProfesor = parseInt(data.idProfesor, 10);
            if (isNaN(idProfesor) || idProfesor <= 0) {
                var errorProfesor = {
                    message: 'Debes seleccionar un profesor válido',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - idProfesor is invalid:', data.idProfesor);
                return $q.reject(errorProfesor);
            }

            // Validate idEstudiante
            var idEstudiante = parseInt(data.idEstudiante, 10);
            if (isNaN(idEstudiante) || idEstudiante <= 0) {
                var errorEstudiante = {
                    message: 'Debes seleccionar un estudiante válido',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data - idEstudiante is invalid:', data.idEstudiante);
                return $q.reject(errorEstudiante);
            }

            var payload = {
                nombre: data.nombre.toString().trim(),
                valor: valor,
                idProfesor: idProfesor,
                idEstudiante: idEstudiante
            };

            $log.debug('NotasService: Updating nota with payload', payload);

            var url = 'Notas/' + id;

            return apiService.put(url, payload)
                .then(function(response) {
                    var nota = response.data || response;
                    $log.info('NotasService: Nota ' + id + ' updated successfully');
                    return nota;
                })
                .catch(function(error) {
                    var message = error && error.data && error.data.message 
                        ? error.data.message 
                        : 'Error actualizando nota';
                    $log.error('NotasService: Error updating nota', error);
                    return $q.reject({
                        message: message,
                        status: error.status || 500,
                        data: error.data
                    });
                });
        }

        /**
         * Delete a nota
         * @param {number} id - Nota ID
         * @returns {Promise} Promise indicating deletion success
         */
        function deleteNota(id) {
            $log.debug('NotasService: Deleting nota', id);
            
            var url = 'Notas/' + id;

            return apiService.delete(url)
                .then(function(response) {
                    $log.info('NotasService: Nota ' + id + ' deleted successfully');
                    return { success: true };
                })
                .catch(function(error) {
                    var message = error && error.data && error.data.message 
                        ? error.data.message 
                        : 'Error eliminando nota';
                    $log.error('NotasService: Error deleting nota', error);
                    return $q.reject({
                        message: message,
                        status: error.status || 500,
                        data: error.data
                    });
                });
        }

        /**
         * Search notas with filters, sorting, and pagination
         * @param {Object} filters - Filter criteria
         * @param {number} pageSize - Items per page
         * @param {number} page - Page number
         * @returns {Promise} Promise containing filtered notas
         */
        function searchNotas(filters, pageSize, page) {
            $log.debug('NotasService: Searching notas', filters);
            
            var queryParams = {
                page: page || 1,
                pageSize: pageSize || paginationConfig.defaultPageSize,
                filter: buildFilterQuery(filters)
            };

            return getNotas(queryParams);
        }

        /**
         * Build filter query string from filter object
         * @param {Object} filters - Filter criteria
         * @returns {string} Filter query string
         */
        function buildFilterQuery(filters) {
            var filterParts = [];

            if (filters.nombre) {
                filterParts.push('Nombre=' + encodeURIComponent(filters.nombre));
            }

            if (filters.idProfesor) {
                filterParts.push('IdProfesor=' + filters.idProfesor);
            }

            if (filters.idEstudiante) {
                filterParts.push('IdEstudiante=' + filters.idEstudiante);
            }

            if (filters.minValor !== undefined && filters.minValor !== '') {
                filterParts.push('MinValor=' + parseFloat(filters.minValor));
            }

            if (filters.maxValor !== undefined && filters.maxValor !== '') {
                filterParts.push('MaxValor=' + parseFloat(filters.maxValor));
            }

            var query = filterParts.join(';');
            $log.debug('NotasService: Built filter query', query);
            
            return query;
        }

        /**
         * Build query parameters for API requests
         * @param {Object} queryParams - Query parameters
         * @returns {Object} Formatted query parameters
         */
        function buildQueryParams(queryParams) {
            if (!queryParams) {
                queryParams = {};
            }

            var params = {
                page: queryParams.page || 1,
                pageSize: queryParams.pageSize || paginationConfig.defaultPageSize,
                maxPageSize: paginationConfig.maxPageSize || 100,
                sortField: queryParams.orderBy || 'nombre',
                sortDesc: queryParams.orderDirection === 'desc' ? true : false
            };

            // Add filter if provided
            if (queryParams.filter) {
                params.filter = queryParams.filter;
            }

            $log.debug('NotasService: Built query params', params);
            
            return params;
        }

    }

})();
