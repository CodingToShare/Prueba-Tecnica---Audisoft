(function () {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .factory('toastService', toastService);

    toastService.$inject = ['$document', '$window', '$timeout'];
    function toastService($document, $window, $timeout) {
        var container;

        function ensureContainer() {
            if (container && $document[0].body.contains(container)) return container;
            container = $document[0].querySelector('.toast-container');
            if (!container) {
                container = $document[0].createElement('div');
                container.className = 'toast-container position-fixed top-0 end-0 p-3';
                container.style.zIndex = 11000;
                $document[0].body.appendChild(container);
            }
            return container;
        }

        function buildToast(message, title, variant, delay) {
            var el = $document[0].createElement('div');
            el.className = 'toast align-items-center text-bg-' + (variant || 'primary') + ' border-0';
            el.setAttribute('role', 'alert');
            el.setAttribute('aria-live', 'assertive');
            el.setAttribute('aria-atomic', 'true');

            var headerHtml = title ? (
                '<div class="toast-header">'
                + '<strong class="me-auto">' + title + '</strong>'
                + '<button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>'
                + '</div>'
            ) : '';

            var bodyHtml = '<div class="d-flex">'
                + '<div class="toast-body">' + message + '</div>'
                + '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>'
                + '</div>';

            el.innerHTML = headerHtml + bodyHtml;

            ensureContainer().appendChild(el);

            var Toast = ($window.bootstrap && $window.bootstrap.Toast) ? $window.bootstrap.Toast : null;
            var instance = Toast ? new Toast(el, { delay: delay || 3500, autohide: true }) : null;

            // Fallback: remove toast if Bootstrap isn't available
            if (!instance) {
                $timeout(function(){ el.remove(); }, delay || 3500);
            } else {
                el.addEventListener('hidden.bs.toast', function () { el.remove(); });
                instance.show();
            }
        }

        function success(message, title) { buildToast(message, title || 'Éxito', 'success', 2500); }
        function info(message, title) { buildToast(message, title || 'Información', 'info', 3000); }
        function warning(message, title) { buildToast(message, title || 'Atención', 'warning', 4000); }
        function error(message, title) { buildToast(message, title || 'Error', 'danger', 5000); }

        return { success: success, info: info, warning: warning, error: error };
    }
})();
