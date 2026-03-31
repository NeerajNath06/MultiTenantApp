(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createFormPage({
        pageId: 'departmentFormPage',
        formId: 'departmentForm',
        errorSummaryId: 'departmentFormErrors',
        submitButtonId: 'departmentFormSubmitButton',
        createSuccessMessage: 'Department created successfully.',
        updateSuccessMessage: 'Department updated successfully.',
        saveErrorMessage: 'Unable to save department.',
        loadErrorMessage: 'Unable to load department data.'
    });
})(window, document);
