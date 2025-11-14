(function () {
    'use strict';

    angular
        .module('audiSoftSchoolApp')
        .directive('asInput', asInputDirective);

    function asInputDirective() {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                label: '@',
                model: '=',
                type: '@?',
                required: '<?',
                min: '<?',
                max: '<?',
                step: '<?',
                minlength: '<?',
                maxlength: '<?',
                pattern: '@?',
                placeholder: '@?',
                disabled: '<?',
                help: '@?',
                id: '@?',
                name: '@?',
                options: '=?',
                valueField: '@?',
                labelField: '@?',
                errorRequired: '@?',
                errorPattern: '@?',
                errorMinlength: '@?',
                errorMaxlength: '@?',
                errorMin: '@?',
                errorMax: '@?'
            },
            require: ['?^form'],
            template:
                '<div class="mb-3">' +
                '  <label class="form-label" ng-attr-for="{{id}}">{{ label }} <span class="text-danger" ng-if="required">*</span></label>' +
                '  <div ng-switch="inputType">' +
                '    <input ng-switch-when="text" type="text" class="form-control" ng-class="{\'is-invalid\': isInvalid(), \'is-valid\': isValid()}" ng-attr-id="{{id}}" ng-attr-name="{{name}}" ng-model="model" ng-required="required" ng-disabled="disabled" ng-attr-placeholder="{{placeholder}}" ng-minlength="minlength" ng-maxlength="maxlength" ng-pattern="patternExpr" />' +
                '    <input ng-switch-when="number" type="number" class="form-control" ng-class="{\'is-invalid\': isInvalid(), \'is-valid\': isValid()}" ng-attr-id="{{id}}" ng-attr-name="{{name}}" ng-model="model" ng-required="required" ng-disabled="disabled" ng-attr-placeholder="{{placeholder}}" ng-attr-min="{{min}}" ng-attr-max="{{max}}" ng-attr-step="{{step || 1}}" />' +
                '    <select ng-switch-when="select" class="form-select" ng-class="{\'is-invalid\': isInvalid(), \'is-valid\': isValid()}" ng-attr-id="{{id}}" ng-attr-name="{{name}}" ng-model="model" ng-required="required" ng-disabled="disabled" ng-options="option[valueField || \'value\'] as option[labelField || \'label\'] for option in (options || [])">' +
                '      <option value="">-- Seleccionar --</option>' +
                '    </select>' +
                '  </div>' +
                '  <div class="invalid-feedback" ng-if="isInvalid()">' +
                '    <div ng-if="ctrl.$error.required">{{ errorRequired || \"Este campo es obligatorio\" }}</div>' +
                '    <div ng-if="ctrl.$error.minlength">{{ errorMinlength || (\"Debe tener al menos \" + (minlength || 0) + \" caracteres\") }}</div>' +
                '    <div ng-if="ctrl.$error.maxlength">{{ errorMaxlength || (\"Debe tener como máximo \" + (maxlength || 0) + \" caracteres\") }}</div>' +
                '    <div ng-if="ctrl.$error.pattern">{{ errorPattern || \"Formato inválido\" }}</div>' +
                '    <div ng-if="ctrl.$error.min">{{ errorMin || (\"El valor mínimo es \" + (min !== undefined ? min : \"\")) }}</div>' +
                '    <div ng-if="ctrl.$error.max">{{ errorMax || (\"El valor máximo es \" + (max !== undefined ? max : \"\")) }}</div>' +
                '  </div>' +
                '  <small class="form-text text-muted" ng-if="help">{{ help }}</small>' +
                '</div>',
            link: function (scope, element, attrs, ctrls) {
                scope.inputType = (scope.type || 'text').toLowerCase();
                var formCtrl = ctrls && ctrls[0] ? ctrls[0] : null;

                // Build pattern expression as RegExp if provided as string
                scope.patternExpr = undefined;
                if (scope.pattern) {
                    try {
                        if (scope.pattern.charAt(0) === '/' && scope.pattern.lastIndexOf('/') > 0) {
                            var last = scope.pattern.lastIndexOf('/');
                            var body = scope.pattern.slice(1, last);
                            var flags = scope.pattern.slice(last + 1);
                            scope.patternExpr = new RegExp(body, flags);
                        } else {
                            scope.patternExpr = new RegExp(scope.pattern);
                        }
                    } catch (e) {
                        // Fallback: let Angular treat it as plain string
                        scope.patternExpr = scope.pattern;
                    }
                }

                function getControl() {
                    if (!formCtrl || !scope.name) return null;
                    return formCtrl[scope.name] || null;
                }

                scope.ctrl = getControl();

                scope.isInvalid = function () {
                    var c = scope.ctrl || getControl();
                    if (!c) return false;
                    var touchedOrSubmitted = c.$touched || (formCtrl && formCtrl.$submitted);
                    return touchedOrSubmitted && c.$invalid;
                };

                scope.isValid = function () {
                    var c = scope.ctrl || getControl();
                    if (!c) return false;
                    if (!c.$touched && !(formCtrl && formCtrl.$submitted)) return false;
                    return c.$valid;
                };
            }
        };
    }
})();
