// Security Agency Management System - Custom JavaScript

$(document).ready(function () {
    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();

    // Initialize popovers
    $('[data-bs-toggle="popover"]').popover();
});

// Common utility functions
function showSuccessMessage(message) {
    showAlert(message, 'success', 'Success');
}

function showErrorMessage(message) {
    showAlert(message, 'danger', 'Action failed');
}

function showAlert(message, type, title) {
    if (typeof window.showCrudFeedbackModal !== 'function') return;

    window.showCrudFeedbackModal({
        message: message,
        type: type,
        title: title
    });
}
