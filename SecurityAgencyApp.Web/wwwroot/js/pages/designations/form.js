(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    function populateDepartments(select, items) {
        if (!select) {
            return;
        }

        var currentValue = select.value;
        select.innerHTML = '<option value="">Select Department (Optional)</option>' + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(item.name) + '</option>';
        }).join('');

        if (currentValue) {
            select.value = currentValue;
        }
    }

    window.AppCrud.createFormPage({
        pageId: 'designationFormPage',
        formId: 'designationForm',
        errorSummaryId: 'designationFormErrors',
        submitButtonId: 'designationFormSubmitButton',
        createSuccessMessage: 'Designation created successfully.',
        updateSuccessMessage: 'Designation updated successfully.',
        saveErrorMessage: 'Unable to save designation.',
        loadErrorMessage: 'Unable to load designation data.',
        initialize: function () {
            return window.AppApi.get('Departments', {
                includeInactive: 'false',
                pageSize: '1000'
            }).then(function (data) {
                populateDepartments(document.getElementById('designationDepartmentId'), data.items || []);
            });
        }
    });
})(window, document);
