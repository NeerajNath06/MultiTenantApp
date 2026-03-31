(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    function populateSelect(select, items, placeholder, labelSelector) {
        if (!select) {
            return;
        }

        var currentValue = select.value;
        select.innerHTML = '<option value="">' + placeholder + '</option>' + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(labelSelector(item)) + '</option>';
        }).join('');

        if (currentValue) {
            select.value = currentValue;
        }
    }

    function loadSupervisors(siteId) {
        var select = document.getElementById('guardAssignmentSupervisorId');
        if (!select) {
            return Promise.resolve();
        }

        select.innerHTML = '<option value="">-- Select Supervisor --</option>';
        if (!siteId) {
            return Promise.resolve();
        }

        return window.AppApi.get('Sites/' + siteId + '/Supervisors').then(function (data) {
            populateSelect(select, data.items || [], '-- Select Supervisor --', function (item) {
                return item.displayName || item.name || '';
            });
        });
    }

    window.AppCrud.createFormPage({
        pageId: 'guardAssignmentFormPage',
        formId: 'guardAssignmentForm',
        errorSummaryId: 'guardAssignmentFormErrors',
        submitButtonId: 'guardAssignmentFormSubmitButton',
        createSuccessMessage: 'Guard assigned successfully.',
        saveErrorMessage: 'Unable to save assignment.',
        loadErrorMessage: 'Unable to load assignment form data.',
        initialize: function () {
            var siteSelect = document.getElementById('guardAssignmentSiteId');

            return Promise.all([
                window.AppApi.get('SecurityGuards', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('Shifts', { includeInactive: 'false', pageSize: '1000' })
            ]).then(function (results) {
                populateSelect(document.getElementById('guardAssignmentGuardId'), results[0].items || [], '-- Select Guard --', function (item) { return item.guardCode || ''; });
                populateSelect(document.getElementById('guardAssignmentSiteId'), results[1].items || [], '-- Select Site --', function (item) { return item.siteName || ''; });
                populateSelect(document.getElementById('guardAssignmentShiftId'), results[2].items || [], '-- Select Shift --', function (item) { return item.shiftName || ''; });

                if (siteSelect) {
                    siteSelect.addEventListener('change', function () {
                        loadSupervisors(siteSelect.value);
                    });
                }
            });
        },
        transformPayload: function (payload) {
            if (!payload.supervisorId) {
                payload.supervisorId = null;
            }
            if (!payload.assignmentEndDate) {
                payload.assignmentEndDate = null;
            }
            if (!payload.remarks) {
                payload.remarks = null;
            }
            return payload;
        }
    });
})(window, document);
