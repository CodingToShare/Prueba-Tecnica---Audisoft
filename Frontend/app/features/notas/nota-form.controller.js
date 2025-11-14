(function(){
  'use strict';
  angular
    .module('audiSoftSchoolApp')
    .controller('NotaFormController', NotaFormController);

  NotaFormController.$inject = ['$scope'];
  function NotaFormController($scope){
    var vm = this;
    vm.title = 'Nueva Nota';
  }
})();
