(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createFormPage({
        pageId: 'menuFormPage',
        formId: 'menuForm',
        errorSummaryId: 'menuFormErrors',
        submitButtonId: 'menuFormSubmitButton',
        createSuccessMessage: 'Menu created successfully.',
        saveErrorMessage: 'Unable to save menu.'
    });
})(window, document);
