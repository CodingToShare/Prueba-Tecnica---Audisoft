(function() {
    'use strict';

    /**
     * Profesores Service
     * Handles all API interactions for teacher management
     * Supports advanced filtering, sorting, and pagination
     */
    angular
        .module('audiSoftSchoolApp')
        .factory('profesoresService', profesoresService);

    profesoresService.$inject = ['$q', '$log', 'apiService', 'configService'];
    function profesoresService($q, $log, apiService, configService) {
        
        var paginationConfig = configService.getPaginationConfig();
        
        var service = {
            // List operations
            getProfesores: getProfesores,
            getProfesorById: getProfesorById,
            
            // Create/Update operations
            createProfesor: createProfesor,
            updateProfesor: updateProfesor,
            
            // Delete operations
            deleteProfesor: deleteProfesor,
            
            // Search/Filter operations
            searchProfesores: searchProfesores,
            
            // Utility methods
            buildFilterQuery: buildFilterQuery
        };

        return service;

        ////////////////

        /**
         * Get paginated list of profesores with optional filtering and sorting
         * @param {Object} queryParams - Query parameters object
         * @param {number} queryParams.page - Page number (1-indexed)
         * @param {number} queryParams.pageSize - Items per page
         * @param {string} queryParams.orderBy - Field to sort by
         * @param {string} queryParams.orderDirection - 'asc' or 'desc'
         * @param {Object} queryParams.filters - Filter object { field: value, ... }
         * @returns {Promise} Resolves with { data: [], totalCount, page, pageSize }
         */
        function getProfesores(queryParams) {
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

            $log.debug('ProfesoresService: Fetching profesores with params', apiParams);

            apiService.get('profesores', apiParams)
                .then(function(response) {
                    $log.debug('ProfesoresService: Full response:', response);
                    
                    // Extract totalCount from X-Total-Count header or from response
                    var totalCount = 0;
                    if (response.headers && typeof response.headers === 'object') {
                        // Try both lowercase and capitalized versions
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
                    
                    $log.debug('ProfesoresService: responseData:', responseData);
                    $log.debug('ProfesoresService: responseData type:', typeof responseData);
                    $log.debug('ProfesoresService: responseData.items:', responseData ? responseData.items : 'N/A');
                    
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
                        $log.warn('ProfesoresService: Unexpected response format:', responseData);
                    }
                    
                    var result = {
                        data: items || [],
                        totalCount: totalCount,
                        page: page,
                        pageSize: pageSize
                    };

                    $log.info('ProfesoresService: Fetched', result.data.length, 'de', result.totalCount, 'profesores');
                    deferred.resolve(result);
                })
                .catch(function(error) {
                    $log.error('ProfesoresService: Failed to fetch profesores', error);
                    deferred.reject(error);
                });

            return deferred.promise;
        }

        /**
         * Get single profesor by ID
         * @param {number} id - Teacher ID
         * @returns {Promise} Resolves with profesor object
         */
        function getProfesorById(id) {
            if (!id) {
                return $q.reject({ message: 'ID is required' });
            }

            $log.debug('ProfesoresService: Fetching profesor', id);

            return apiService.get('profesores/' + id)
                .then(function(response) {
                    $log.info('ProfesoresService: Fetched profesor', id);
                    return response.data;
                })
                .catch(function(error) {
                    $log.error('ProfesoresService: Failed to fetch profesor', id, error);
                    return $q.reject(error);
                });
        }

        /**
         * Create new profesor
         * @param {Object} profesorData - Teacher data
         * @returns {Promise} Resolves with created profesor
         */
        function createProfesor(profesorData) {
            if (!profesorData || !profesorData.nombre) {
                return $q.reject({ message: 'Teacher name is required' });
            }

            $log.debug('ProfesoresService: Creating profesor', profesorData);

            return apiService.post('profesores', profesorData)
                .then(function(response) {
                    $log.info('ProfesoresService: Created profesor', response.data.id);
                    return response.data;
                })
                .catch(function(error) {
                    $log.error('ProfesoresService: Failed to create profesor', error);
                    return $q.reject(error);
                });
        }

        /**
         * Update existing profesor
         * @param {number} id - Teacher ID
         * @param {Object} profesorData - Updated teacher data
         * @returns {Promise} Resolves when update complete
         */
        function updateProfesor(id, profesorData) {
            if (!id) {
                return $q.reject({ message: 'ID is required' });
            }

            if (!profesorData || !profesorData.nombre) {
                return $q.reject({ message: 'Teacher name is required' });
            }

            $log.debug('ProfesoresService: Updating profesor', id, profesorData);

            return apiService.put('profesores/' + id, profesorData)
                .then(function(response) {
                    $log.info('ProfesoresService: Updated profesor', id);
                    return response.data;
                })
                .catch(function(error) {
                    $log.error('ProfesoresService: Failed to update profesor', id, error);
                    return $q.reject(error);
                });
        }

        /**
         * Delete profesor (soft delete)
         * @param {number} id - Teacher ID
         * @returns {Promise} Resolves when delete complete
         */
        function deleteProfesor(id) {
            if (!id) {
                return $q.reject({ message: 'ID is required' });
            }

            $log.debug('ProfesoresService: Deleting profesor', id);

            return apiService.delete('profesores/' + id)
                .then(function(response) {
                    $log.info('ProfesoresService: Deleted profesor', id);
                    return response.data;
                })
                .catch(function(error) {
                    $log.error('ProfesoresService: Failed to delete profesor', id, error);
                    return $q.reject(error);
                });
        }

        /**
         * Search profesores with advanced filtering
         * Uses backend's advanced filter syntax: field:value; (AND), field=value| (OR)
         * @param {Object} filters - Filter object { nombre: 'María', ... }
         * @param {number} pageSize - Items per page
         * @param {number} page - Page number
         * @returns {Promise} Resolves with paginated results
         */
        function searchProfesores(filters, pageSize, page) {
            pageSize = pageSize || paginationConfig.defaultPageSize;
            page = page || 1;

            var queryParams = {
                filters: filters,
                pageSize: pageSize,
                page: page
            };

            return getProfesores(queryParams);
        }

        /**
         * Build filter query string for backend advanced filtering
         * Backend supports:
         * - "Nombre:María" (contains)
         * - "Id=5" (equals) 
         * - "Valor>50;Nombre:Maria" (AND with ;)
         * - "Nombre:María|Nombre:Carlos" (OR with |)
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
