(function(){
  'use strict';
  angular
    .module('audiSoftSchoolApp')
    .controller('ProfesorFormController', ProfesorFormController);

  ProfesorFormController.$inject = ['$scope'];
  function ProfesorFormController($scope){
    var vm = this;
    vm.title = 'Nuevo Profesor';
  }
})();
