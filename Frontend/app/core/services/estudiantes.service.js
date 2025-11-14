(function() {
    'use strict';

    /**
     * Estudiantes Service
     * Handles all API interactions for student management
     * Supports advanced filtering, sorting, and pagination
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('estudiantesService', estudiantesService);

    estudiantesService.$inject = ['$q', '$log', 'apiService', 'configService'];
    function estudiantesService($q, $log, apiService, configService) {
        
        var paginationConfig = configService.getPaginationConfig();
        
        var service = {
            // List operations
            getEstudiantes: getEstudiantes,
            getEstudianteById: getEstudianteById,
            
            // Create/Update operations
            createEstudiante: createEstudiante,
            updateEstudiante: updateEstudiante,
            
            // Delete operations
            deleteEstudiante: deleteEstudiante,
            
            // Search/Filter operations
            searchEstudiantes: searchEstudiantes,
            
            // Utility methods
            buildFilterQuery: buildFilterQuery
        };

        return service;

        ////////////////

        /**
         * Get paginated list of estudiantes with optional filtering and sorting
         * @param {Object} queryParams - Query parameters object
         * @param {number} queryParams.page - Page number (1-indexed)
         * @param {number} queryParams.pageSize - Items per page
         * @param {string} queryParams.orderBy - Field to sort by
         * @param {string} queryParams.orderDirection - 'asc' or 'desc'
         * @param {Object} queryParams.filters - Filter object { field: value, ... }
         * @returns {Promise} Resolves with { data: [], totalCount, page, pageSize }
         */
        function getEstudiantes(queryParams) {
            var deferred = $q.defer();

            // Default pagination
            queryParams = queryParams || {};
            var page = queryParams.page || 1;
            var pageSize = queryParams.pageSize || paginationConfig.defaultPageSize;

            // Build API parameters according to backend QueryParams structure
            var apiParams = {
                page: page,
                pageSize: pageSize,
                maxPageSize: 100
            };

            // Add filtering if present
            if (queryParams.filters && Object.keys(queryParams.filters).length > 0) {
                var filterQuery = buildFilterQuery(queryParams.filters);
                if (filterQuery) {
                    apiParams.filter = filterQuery;
                }
            }

            // Add sorting if present
            if (queryParams.orderBy) {
                apiParams.sortField = queryParams.orderBy;
                // Backend expects sortDesc: true for desc, false for asc
                apiParams.sortDesc = queryParams.orderDirection === 'desc' ? true : false;
            }

            $log.debug('EstudiantesService: Fetching estudiantes with params', apiParams);

            apiService.get('estudiantes', apiParams)
                .then(function(response) {
                    // The API returns PagedResult with items array
                    // Extract totalCount from X-Total-Count header or from response
                    // Note: In AngularJS $http, headers is a function that returns the header value
                    var headerValue = response.headers('X-Total-Count');
                    var totalCount = headerValue ? parseInt(headerValue) : (response.totalCount || 0);
                    
                    var result = {
                        data: response.data || [],
                        totalCount: totalCount,
                        page: page,
                        pageSize: pageSize
                    };

                    $log.info('EstudiantesService: Fetched', result.data.length, 'de', result.totalCount, 'estudiantes');
                    deferred.resolve(result);
                })
                .catch(function(error) {
                    $log.error('EstudiantesService: Failed to fetch estudiantes', error);
                    deferred.reject(error);
                });

            return deferred.promise;
        }

        /**
         * Get single estudiante by ID
         * @param {number} id - Student ID
         * @returns {Promise} Resolves with estudiante object
         */
        function getEstudianteById(id) {
            if (!id) {
                return $q.reject({ message: 'ID is required' });
            }

            $log.debug('EstudiantesService: Fetching estudiante', id);

            return apiService.get('estudiantes/' + id)
                .then(function(response) {
                    $log.info('EstudiantesService: Fetched estudiante', id);
                    return response.data;
                })
                .catch(function(error) {
                    $log.error('EstudiantesService: Failed to fetch estudiante', id, error);
                    return $q.reject(error);
                });
        }

        /**
         * Create new estudiante
         * @param {Object} estudianteData - Student data
         * @returns {Promise} Resolves with created estudiante
         */
        function createEstudiante(estudianteData) {
            if (!estudianteData || !estudianteData.nombre) {
                return $q.reject({ message: 'Student name is required' });
            }

            $log.debug('EstudiantesService: Creating estudiante', estudianteData);

            return apiService.post('estudiantes', estudianteData)
                .then(function(response) {
                    $log.info('EstudiantesService: Created estudiante', response.data.id);
                    return response.data;
                })
                .catch(function(error) {
                    $log.error('EstudiantesService: Failed to create estudiante', error);
                    return $q.reject(error);
                });
        }

        /**
         * Update existing estudiante
         * @param {number} id - Student ID
         * @param {Object} estudianteData - Updated student data
         * @returns {Promise} Resolves when update complete
         */
        function updateEstudiante(id, estudianteData) {
            if (!id) {
                return $q.reject({ message: 'ID is required' });
            }

            if (!estudianteData || !estudianteData.nombre) {
                return $q.reject({ message: 'Student name is required' });
            }

            $log.debug('EstudiantesService: Updating estudiante', id, estudianteData);

            return apiService.put('estudiantes/' + id, estudianteData)
                .then(function(response) {
                    $log.info('EstudiantesService: Updated estudiante', id);
                    return response.data;
                })
                .catch(function(error) {
                    $log.error('EstudiantesService: Failed to update estudiante', id, error);
                    return $q.reject(error);
                });
        }

        /**
         * Delete estudiante (soft delete)
         * @param {number} id - Student ID
         * @returns {Promise} Resolves when delete complete
         */
        function deleteEstudiante(id) {
            if (!id) {
                return $q.reject({ message: 'ID is required' });
            }

            $log.debug('EstudiantesService: Deleting estudiante', id);

            return apiService.delete('estudiantes/' + id)
                .then(function(response) {
                    $log.info('EstudiantesService: Deleted estudiante', id);
                    return response.data;
                })
                .catch(function(error) {
                    $log.error('EstudiantesService: Failed to delete estudiante', id, error);
                    return $q.reject(error);
                });
        }

        /**
         * Search estudiantes with advanced filtering
         * Uses backend's advanced filter syntax: field:value; (AND), field=value| (OR)
         * @param {Object} filters - Filter object { nombre: 'Juan', grado: '10°', ... }
         * @param {number} pageSize - Items per page
         * @param {number} page - Page number
         * @returns {Promise} Resolves with paginated results
         */
        function searchEstudiantes(filters, pageSize, page) {
            pageSize = pageSize || paginationConfig.defaultPageSize;
            page = page || 1;

            var queryParams = {
                filters: filters,
                pageSize: pageSize,
                page: page
            };

            return getEstudiantes(queryParams);
        }

        /**
         * Build filter query string for backend advanced filtering
         * Backend supports:
         * - "Nombre:Juan" (contains)
         * - "Id=5" (equals) 
         * - "Valor>50;Nombre:Maria" (AND with ;)
         * - "Nombre:Juan|Nombre:Maria" (OR with |)
         * @param {Object} filters - Filter object
         * @returns {string} Filter query string
         */
        function buildFilterQuery(filters) {
            if (!filters || typeof filters !== 'object') {
                return '';
            }

            var filterParts = [];

            for (var key in filters) {
                if (filters.hasOwnProperty(key) && filters[key] !== null && filters[key] !== '') {
                    var value = String(filters[key]).trim();

                    if (value) {
                        // Use 'contains' filter syntax for string fields: fieldName:value
                        // This matches the backend's advanced filter syntax
                        filterParts.push(key + ':' + value);
                    }
                }
            }

            // Join with ; for AND logic
            return filterParts.length > 0 ? filterParts.join(';') : '';
        }
    }

})();
