(function(){
  'use strict';
  angular
    .module('audiSoftSchoolApp')
    .controller('ProfesorDetailController', ProfesorDetailController);

  ProfesorDetailController.$inject = ['$scope', '$routeParams'];
  function ProfesorDetailController($scope, $routeParams){
    var vm = this;
    vm.title = 'Detalle de Profesor #' + $routeParams.id;
  }
})();
