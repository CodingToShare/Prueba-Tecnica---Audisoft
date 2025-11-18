/**
 * NotFoundController
 * Handles 404 Not Found page
 */
angular.module('audiSoftSchoolApp').controller('NotFoundController', NotFoundController);

function NotFoundController($scope, $location) {
    var vm = this;
    
    // Expose current URL path to the view
    vm.requestedUrl = $location.path();
}
