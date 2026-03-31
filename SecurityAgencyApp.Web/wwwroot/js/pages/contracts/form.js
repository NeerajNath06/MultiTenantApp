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

    function formatDate(value) {
        return value ? String(value).slice(0, 10) : '';
    }

    window.AppCrud.createFormPage({
        pageId: 'contractFormPage',
        formId: 'contractForm',
        errorSummaryId: 'contractFormErrors',
        submitButtonId: 'contractFormSubmitButton',
        createSuccessMessage: 'Contract created successfully.',
        updateSuccessMessage: 'Contract updated successfully.',
        saveErrorMessage: 'Unable to save contract.',
        loadErrorMessage: 'Unable to load contract data.',
        initialize: function () {
            return window.AppApi.get('Clients', { includeInactive: 'false', pageSize: '1000' }).then(function (data) {
                populateSelect(document.getElementById('contractClientId'), data.items || [], 'Select Client', function (item) {
                    return item.companyName || '';
                });
            });
        },
        mapResponseToForm: function (form, data) {
            Array.from(form.elements).forEach(function (element) {
                if (!element.name) {
                    return;
                }

                var key = Object.keys(data || {}).find(function (name) {
                    return name.toLowerCase() === element.name.toLowerCase();
                });
                if (!key) {
                    return;
                }

                var value = data[key];
                if (element.type === 'date') {
                    element.value = formatDate(value);
                    return;
                }
                if (element.type === 'checkbox') {
                    element.checked = !!value;
                    return;
                }

                element.value = value == null ? '' : value;
            });
        },
        transformPayload: function (payload) {
            if (!payload.description) {
                payload.description = null;
            }
            if (!payload.paymentTerms) {
                payload.paymentTerms = null;
            }
            if (!payload.termsAndConditions) {
                payload.termsAndConditions = null;
            }
            if (!payload.numberOfGuards && payload.numberOfGuards !== 0) {
                payload.numberOfGuards = null;
            }
            if (!payload.renewalDate) {
                payload.renewalDate = null;
            }
            if (!payload.notes) {
                payload.notes = null;
            }
            if (!payload.sites) {
                payload.sites = [];
            }
            return payload;
        }
    });
})(window, document);
