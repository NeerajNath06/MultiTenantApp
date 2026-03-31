(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    function populateGuards(select, items) {
        if (!select) {
            return;
        }

        var currentValue = select.value;
        select.innerHTML = '<option value="">Select Guard</option>' + (items || []).map(function (item) {
            var label = (item.guardCode || '') + ' - ' + ((item.firstName || '') + ' ' + (item.lastName || '')).trim();
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(label) + '</option>';
        }).join('');

        if (currentValue) {
            select.value = currentValue;
        }
    }

    window.AppCrud.createFormPage({
        pageId: 'leaveRequestFormPage',
        formId: 'leaveRequestForm',
        errorSummaryId: 'leaveRequestFormErrors',
        submitButtonId: 'leaveRequestFormSubmitButton',
        createSuccessMessage: 'Leave request created successfully.',
        saveErrorMessage: 'Unable to save leave request.',
        loadErrorMessage: 'Unable to load leave request form data.',
        initialize: function () {
            return window.AppApi.get('SecurityGuards', {
                includeInactive: 'false',
                pageSize: '1000'
            }).then(function (data) {
                populateGuards(document.getElementById('leaveRequestGuardId'), data.items || []);
            });
        },
        transformPayload: function (payload) {
            if (!payload.notes) {
                payload.notes = null;
            }
            return payload;
        }
    });
})(window, document);
