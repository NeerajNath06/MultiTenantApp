(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createFormPage({
        pageId: 'announcementFormPage',
        formId: 'announcementForm',
        errorSummaryId: 'announcementFormErrors',
        submitButtonId: 'announcementFormSubmitButton',
        createSuccessMessage: 'Announcement created successfully.',
        updateSuccessMessage: 'Announcement updated successfully.',
        saveErrorMessage: 'Unable to save announcement.',
        loadErrorMessage: 'Unable to load announcement details.'
    });
})(window, document);
