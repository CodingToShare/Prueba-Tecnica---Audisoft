(function(){
  'use strict';
  angular
    .module('audiSoftSchoolApp')
    .controller('ProfesoresListController', ProfesoresListController);

  ProfesoresListController.$inject = ['$scope'];
  function ProfesoresListController($scope){
    var vm = this;
    vm.title = 'Listado de Profesores';
  }
})();
