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
        pageId: 'trainingRecordFormPage',
        formId: 'trainingRecordForm',
        errorSummaryId: 'trainingRecordFormErrors',
        submitButtonId: 'trainingRecordFormSubmitButton',
        createSuccessMessage: 'Training record created successfully.',
        saveErrorMessage: 'Unable to save training record.',
        loadErrorMessage: 'Unable to load training record form data.',
        initialize: function () {
            return window.AppApi.get('SecurityGuards', {
                includeInactive: 'false',
                pageSize: '1000'
            }).then(function (data) {
                populateGuards(document.getElementById('trainingRecordGuardId'), data.items || []);
            });
        },
        transformPayload: function (payload) {
            if (!payload.expiryDate) {
                payload.expiryDate = null;
            }
            if (!payload.trainingProvider) {
                payload.trainingProvider = null;
            }
            if (!payload.certificateNumber) {
                payload.certificateNumber = null;
            }
            if (payload.score == null || payload.score === '') {
                payload.score = null;
            }
            if (!payload.remarks) {
                payload.remarks = null;
            }
            return payload;
        }
    });
})(window, document);
