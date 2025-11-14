(function () {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .factory('loadingService', loadingService);

    loadingService.$inject = ['$document', '$log'];
    function loadingService($document, $log) {
        var active = 0;
        var overlay = null;

        function ensureOverlay() {
            if (!overlay) {
                overlay = $document[0].getElementById('loading-overlay');
                if (!overlay) {
                    $log.warn('loadingService: #loading-overlay not found');
                }
            }
            return overlay;
        }

        function update() {
            var el = ensureOverlay();
            if (!el) return;
            if (active > 0) {
                el.classList.remove('d-none');
            } else {
                el.classList.add('d-none');
            }
        }

        function show() {
            active++;
            update();
        }

        function hide() {
            active = Math.max(0, active - 1);
            update();
        }

        function reset() {
            active = 0;
            update();
        }

        return {
            show: show,
            hide: hide,
            reset: reset,
            isActive: function() { return active > 0; }
        };
    }
})();
