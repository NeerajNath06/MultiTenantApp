(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createFormPage({
        pageId: 'shiftFormPage',
        formId: 'shiftForm',
        errorSummaryId: 'shiftFormErrors',
        submitButtonId: 'shiftFormSubmitButton',
        createSuccessMessage: 'Shift created successfully.',
        updateSuccessMessage: 'Shift updated successfully.',
        saveErrorMessage: 'Unable to save shift.',
        loadErrorMessage: 'Unable to load shift data.'
    });
})(window, document);
