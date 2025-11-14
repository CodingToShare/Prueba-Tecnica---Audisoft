(function(){
  'use strict';
  angular
    .module('audiSoftSchoolApp')
    .controller('NotaDetailController', NotaDetailController);

  NotaDetailController.$inject = ['$scope', '$routeParams'];
  function NotaDetailController($scope, $routeParams){
    var vm = this;
    vm.title = 'Detalle de Nota #' + $routeParams.id;
  }
})();
