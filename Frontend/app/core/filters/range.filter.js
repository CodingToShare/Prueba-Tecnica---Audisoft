(function() {
    'use strict';

    /**
     * Utility Filters for UI helpers
     */
    angular
        .module('audiSoftSchoolApp')
        .filter('range', rangeFilter);

    /**
     * Range filter for pagination
     * Usage: ng-repeat="page in [] | range:1:10" creates array [1,2,3...10]
     */
    function rangeFilter() {
        return function(input, start, end) {
            var result = [];
            start = parseInt(start, 10);
            end = parseInt(end, 10);
            
            for (var i = start; i < end; i++) {
                result.push(i);
            }
            
            return result;
        };
    }

})();
