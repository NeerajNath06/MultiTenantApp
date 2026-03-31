(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    function setAll(page, field, value) {
        Array.from(page.querySelectorAll('[data-field="' + field + '"]')).forEach(function (element) {
            element.textContent = value == null || value === '' ? '-' : String(value);
        });
    }

    window.AppCrud.createDetailsPage({
        pageId: 'userDetailsPage',
        deleteButtonId: 'userDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'Users/' + page.getAttribute('data-user-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this user?';
        },
        deleteSuccessMessage: 'User deleted successfully.',
        render: function (page, data, helpers) {
            var fullName = ((data.firstName || '') + ' ' + (data.lastName || '')).trim() || '-';
            var rolesContainer = document.getElementById('userRolesBadges');
            var statusBadge = page.querySelector('[data-field="statusBadge"]');

            setAll(page, 'fullName', fullName);
            setAll(page, 'userName', data.userName || '-');
            helpers.setText('email', data.email);
            helpers.setText('phoneNumber', data.phoneNumber);
            helpers.setText('createdDate', data.createdDate ? new Date(data.createdDate).toLocaleDateString() : '-');
            helpers.setText('departmentName', data.departmentName);
            helpers.setText('designationName', data.designationName);
            helpers.setText('aadharNumber', data.aadharNumber);
            helpers.setText('panNumber', data.pANNumber || data.panNumber);
            helpers.setText('uan', data.uAN || data.uan);

            if (statusBadge) {
                statusBadge.textContent = data.isActive ? 'Active' : 'Inactive';
                statusBadge.className = 'badge ' + (data.isActive ? 'bg-success' : 'bg-danger');
            }

            if (rolesContainer) {
                var roles = data.roles || [];
                rolesContainer.innerHTML = roles.length
                    ? roles.map(function (role) {
                        return '<span class="badge bg-primary">' + window.AppCrud.escapeHtml(role) + '</span>';
                    }).join('')
                    : '<span class="text-muted">No roles assigned</span>';
            }
        }
    });
})(window, document);
