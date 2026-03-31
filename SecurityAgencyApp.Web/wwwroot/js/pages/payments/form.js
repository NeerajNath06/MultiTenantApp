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

    function formatDateInput(value) {
        return value ? String(value).slice(0, 10) : '';
    }

    window.AppCrud.createFormPage({
        pageId: 'paymentFormPage',
        formId: 'paymentForm',
        errorSummaryId: 'paymentFormErrors',
        submitButtonId: 'paymentFormSubmitButton',
        createSuccessMessage: 'Payment recorded successfully.',
        updateSuccessMessage: 'Payment updated successfully.',
        saveErrorMessage: 'Unable to save payment.',
        loadErrorMessage: 'Unable to load payment data.',
        initialize: function () {
            return Promise.all([
                window.AppApi.get('Bills', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('Clients', { includeInactive: 'false', pageSize: '1000' })
            ]).then(function (results) {
                populateSelect(document.getElementById('paymentBillId'), results[0].items || [], 'Select Bill', function (item) {
                    return (item.billNumber || '') + (item.totalAmount != null ? ' - \u20b9' + Number(item.totalAmount).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '');
                });
                populateSelect(document.getElementById('paymentClientId'), results[1].items || [], 'Select Client', function (item) {
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
                    element.value = formatDateInput(value);
                    return;
                }

                element.value = value == null ? '' : value;
            });
        },
        transformPayload: function (payload) {
            if (!payload.billId) {
                payload.billId = null;
            }
            if (!payload.clientId) {
                payload.clientId = null;
            }
            if (!payload.chequeNumber) {
                payload.chequeNumber = null;
            }
            if (!payload.bankName) {
                payload.bankName = null;
            }
            if (!payload.transactionReference) {
                payload.transactionReference = null;
            }
            if (!payload.status) {
                payload.status = null;
            }
            if (!payload.notes) {
                payload.notes = null;
            }
            if (!payload.receivedDate) {
                payload.receivedDate = null;
            }

            return payload;
        }
    });
})(window, document);
