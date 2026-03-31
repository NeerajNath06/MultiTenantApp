(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createFormPage({
        pageId: 'clientFormPage',
        formId: 'clientForm',
        errorSummaryId: 'clientFormErrors',
        submitButtonId: 'clientFormSubmitButton',
        createSuccessMessage: 'Client created successfully.',
        updateSuccessMessage: 'Client updated successfully.',
        saveErrorMessage: 'Unable to save client.',
        loadErrorMessage: 'Unable to load client data.'
    });
})(window, document);
