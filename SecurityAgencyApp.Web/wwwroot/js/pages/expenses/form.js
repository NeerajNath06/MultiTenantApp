(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    function populateSelect(select, items, placeholder, textSelector) {
        if (!select) {
            return;
        }

        var currentValue = select.value;
        select.innerHTML = '<option value="">' + placeholder + '</option>' + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(textSelector(item)) + '</option>';
        }).join('');

        if (currentValue) {
            select.value = currentValue;
        }
    }

    window.AppCrud.createFormPage({
        pageId: 'expenseFormPage',
        formId: 'expenseForm',
        errorSummaryId: 'expenseFormErrors',
        submitButtonId: 'expenseFormSubmitButton',
        createSuccessMessage: 'Expense created successfully.',
        saveErrorMessage: 'Unable to save expense.',
        loadErrorMessage: 'Unable to load expense form data.',
        initialize: function () {
            return Promise.all([
                window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('SecurityGuards', { includeInactive: 'false', pageSize: '1000' })
            ]).then(function (results) {
                populateSelect(document.getElementById('expenseSiteId'), results[0].items || [], 'Select Site', function (item) {
                    return item.siteName || '';
                });
                populateSelect(document.getElementById('expenseGuardId'), results[1].items || [], 'Select Guard', function (item) {
                    return (item.guardCode || '') + ' - ' + ((item.firstName || '') + ' ' + (item.lastName || '')).trim();
                });
            });
        },
        transformPayload: function (payload) {
            if (!payload.siteId) {
                payload.siteId = null;
            }
            if (!payload.guardId) {
                payload.guardId = null;
            }
            if (!payload.vendorName) {
                payload.vendorName = null;
            }
            if (!payload.receiptNumber) {
                payload.receiptNumber = null;
            }
            if (!payload.notes) {
                payload.notes = null;
            }
            return payload;
        }
    });
})(window, document);
