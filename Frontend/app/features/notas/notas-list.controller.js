(function(){
  'use strict';
  angular
    .module('audiSoftSchoolApp')
    .controller('NotasListController', NotasListController);

  NotasListController.$inject = ['$scope'];
  function NotasListController($scope){
    var vm = this;
    vm.title = 'Listado de Notas';
  }
})();
