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

    window.AppCrud.createFormPage({
        pageId: 'equipmentFormPage',
        formId: 'equipmentForm',
        errorSummaryId: 'equipmentFormErrors',
        submitButtonId: 'equipmentFormSubmitButton',
        createSuccessMessage: 'Equipment created successfully.',
        saveErrorMessage: 'Unable to save equipment.',
        loadErrorMessage: 'Unable to load equipment form data.',
        initialize: function () {
            return Promise.all([
                window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('SecurityGuards', { includeInactive: 'false', pageSize: '1000' })
            ]).then(function (results) {
                populateSelect(document.getElementById('equipmentAssignedToSiteId'), results[0].items || [], 'Not Assigned', function (item) { return item.siteName || ''; });
                populateSelect(document.getElementById('equipmentAssignedToGuardId'), results[1].items || [], 'Not Assigned', function (item) {
                    return (item.guardCode || '') + ' - ' + ((item.firstName || '') + ' ' + (item.lastName || '')).trim();
                });
            });
        },
        transformPayload: function (payload) {
            [
                'manufacturer',
                'modelNumber',
                'serialNumber',
                'assignedToGuardId',
                'assignedToSiteId',
                'lastMaintenanceDate',
                'nextMaintenanceDate',
                'notes'
            ].forEach(function (key) {
                if (!payload[key]) {
                    payload[key] = null;
                }
            });
            return payload;
        }
    });
})(window, document);
