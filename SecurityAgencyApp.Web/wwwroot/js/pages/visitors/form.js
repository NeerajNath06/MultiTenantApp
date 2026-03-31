(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    function populateSelect(select, items, placeholder, valueSelector, textSelector) {
        if (!select) {
            return;
        }

        var currentValue = select.value;
        select.innerHTML = '<option value="">' + placeholder + '</option>' + (items || []).map(function (item) {
            return '<option value="' + valueSelector(item) + '">' + window.AppCrud.escapeHtml(textSelector(item)) + '</option>';
        }).join('');

        if (currentValue) {
            select.value = currentValue;
        }
    }

    window.AppCrud.createFormPage({
        pageId: 'visitorFormPage',
        formId: 'visitorForm',
        errorSummaryId: 'visitorFormErrors',
        submitButtonId: 'visitorFormSubmitButton',
        createSuccessMessage: 'Visitor registered successfully. Badge can be issued at the gate.',
        updateSuccessMessage: 'Visitor updated successfully.',
        saveErrorMessage: 'Unable to save visitor.',
        loadErrorMessage: 'Unable to load visitor data.',
        initialize: function () {
            return Promise.all([
                window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('SecurityGuards', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('Departments', { includeInactive: 'false', pageSize: '1000' })
            ]).then(function (results) {
                populateSelect(document.getElementById('visitorSiteId'), results[0].items || [], '-- Select Site --', function (item) { return item.id; }, function (item) { return item.siteName || ''; });
                populateSelect(document.getElementById('visitorGuardId'), results[1].items || [], '-- Select Guard --', function (item) { return item.id; }, function (item) { return item.guardCode || ''; });
                populateSelect(document.getElementById('visitorHostDepartment'), results[2].items || [], '-- Select Department --', function (item) { return item.name || ''; }, function (item) { return item.name || ''; });
            });
        },
        transformPayload: function (payload) {
            ['companyName', 'email', 'hostName', 'hostDepartment', 'idProofType', 'idProofNumber'].forEach(function (key) {
                if (!payload[key]) {
                    payload[key] = null;
                }
            });
            return payload;
        }
    });
})(window, document);
