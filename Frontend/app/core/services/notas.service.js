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
            var url = '/api/v1/Notas';

            return apiService.get(url, params)
                .then(function(response) {
                    var result = apiService.processSuccessResponse(response);
                    
                    $log.info('NotasService: Fetched ' + (result.totalCount || result.length || 0) + 
                        ' notas from page ' + (queryParams.page || 1));
                    
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
            
            var url = '/api/v1/Notas/' + id;

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
            var url = '/api/v1/Notas/profesor/' + idProfesor;

            return apiService.get(url, params)
                .then(function(response) {
                    var result = apiService.processSuccessResponse(response);
                    
                    $log.info('NotasService: Fetched ' + (result.totalCount || result.length || 0) + 
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
            var url = '/api/v1/Notas/estudiante/' + idEstudiante;

            return apiService.get(url, params)
                .then(function(response) {
                    var result = apiService.processSuccessResponse(response);
                    
                    $log.info('NotasService: Fetched ' + (result.totalCount || result.length || 0) + 
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
            
            if (!data || !data.nombre || data.valor === undefined || !data.idProfesor || !data.idEstudiante) {
                var error = {
                    message: 'Datos de nota incompletos',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data', data);
                return $q.reject(error);
            }

            var payload = {
                nombre: data.nombre.trim(),
                valor: parseFloat(data.valor),
                idProfesor: parseInt(data.idProfesor, 10),
                idEstudiante: parseInt(data.idEstudiante, 10)
            };

            var url = '/api/v1/Notas';

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
            
            if (!data || !data.nombre || data.valor === undefined || !data.idProfesor || !data.idEstudiante) {
                var error = {
                    message: 'Datos de nota incompletos',
                    status: 400
                };
                $log.error('NotasService: Invalid nota data', data);
                return $q.reject(error);
            }

            var payload = {
                nombre: data.nombre.trim(),
                valor: parseFloat(data.valor),
                idProfesor: parseInt(data.idProfesor, 10),
                idEstudiante: parseInt(data.idEstudiante, 10)
            };

            var url = '/api/v1/Notas/' + id;

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
            
            var url = '/api/v1/Notas/' + id;

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
