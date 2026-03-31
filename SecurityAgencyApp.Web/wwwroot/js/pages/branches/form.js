(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createFormPage({
        pageId: 'branchFormPage',
        formId: 'branchForm',
        errorSummaryId: 'branchFormErrors',
        submitButtonId: 'branchFormSubmitButton',
        createSuccessMessage: 'Branch created successfully.',
        updateSuccessMessage: 'Branch updated successfully.',
        saveErrorMessage: 'Unable to save branch details.',
        loadErrorMessage: 'Unable to load branch details.'
    });
})(window, document);
