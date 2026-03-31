(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    function setText(page, field, value) {
        var element = page.querySelector('[data-field="' + field + '"]');
        if (element) {
            element.textContent = value == null || value === '' ? '-' : String(value);
        }
    }

    window.AppCrud.createDetailsPage({
        pageId: 'securityGuardDetailsPage',
        deleteButtonId: 'securityGuardDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'SecurityGuards/' + page.getAttribute('data-guard-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this guard? This cannot be undone.';
        },
        deleteSuccessMessage: 'Security guard deleted successfully.',
        render: function (page, data) {
            var fullName = ((data.firstName || '') + ' ' + (data.lastName || '')).trim() || '-';
            var statusBadge = page.querySelector('[data-field="statusBadge"]');

            setText(page, 'guardCode', data.guardCode);
            setText(page, 'fullName', fullName);
            setText(page, 'email', data.email);
            setText(page, 'phoneNumber', data.phoneNumber);
            setText(page, 'gender', data.gender);
            setText(page, 'dateOfBirth', data.dateOfBirth ? new Date(data.dateOfBirth).toLocaleDateString() : '-');
            setText(page, 'joiningDate', data.joiningDate ? new Date(data.joiningDate).toLocaleDateString() : '-');
            setText(page, 'address', data.address);
            setText(page, 'cityStatePin', [data.city || '-', data.state || '-', data.pinCode || '-'].join(' '));
            setText(page, 'aadharNumber', data.aadharNumber);
            setText(page, 'panNumber', data.pANNumber || data.panNumber);
            setText(page, 'emergencyContact', [data.emergencyContactName || '-', data.emergencyContactPhone || '-'].join(' - '));
            setText(page, 'supervisorName', data.supervisorName);
            setText(page, 'createdDate', data.createdDate ? new Date(data.createdDate).toLocaleDateString() : '-');

            if (statusBadge) {
                statusBadge.textContent = data.isActive ? 'Active' : 'Inactive';
                statusBadge.className = 'badge ' + (data.isActive ? 'bg-success' : 'bg-danger');
            }
        }
    });
})(window, document);
