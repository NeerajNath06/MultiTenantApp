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
        pageId: 'vehicleFormPage',
        formId: 'vehicleForm',
        errorSummaryId: 'vehicleFormErrors',
        submitButtonId: 'vehicleFormSubmitButton',
        createSuccessMessage: 'Vehicle entry registered successfully.',
        saveErrorMessage: 'Unable to register vehicle entry.',
        loadErrorMessage: 'Unable to load vehicle form data.',
        initialize: function () {
            return Promise.all([
                window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('SecurityGuards', { includeInactive: 'false', pageSize: '1000' })
            ]).then(function (results) {
                populateSelect(document.getElementById('vehicleSiteId'), results[0].items || [], '-- Select Site --', function (item) { return item.siteName || ''; });
                populateSelect(document.getElementById('vehicleGuardId'), results[1].items || [], '-- Select Guard --', function (item) { return item.guardCode || ''; });
            });
        },
        transformPayload: function (payload) {
            ['driverPhone', 'parkingSlot'].forEach(function (key) {
                if (!payload[key]) {
                    payload[key] = null;
                }
            });
            return payload;
        }
    });
})(window, document);
