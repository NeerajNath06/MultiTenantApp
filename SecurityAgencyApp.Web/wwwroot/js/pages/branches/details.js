(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'branchDetailsPage',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');

            helpers.setText('branchCode', data.branchCode);
            helpers.setText('branchName', data.branchName);
            helpers.setText('numberPrefix', data.numberPrefix);
            helpers.setText('isHeadOffice', data.isHeadOffice ? 'Yes' : 'No');
            helpers.setText('address', data.address);
            helpers.setText('city', data.city);
            helpers.setText('state', data.state);
            helpers.setText('pinCode', data.pinCode);
            helpers.setText('contactPerson', data.contactPerson);
            helpers.setText('contactPhone', data.contactPhone);
            helpers.setText('contactEmail', data.contactEmail);
            helpers.setText('gstNumber', data.gstNumber);
            helpers.setText('labourLicenseNumber', data.labourLicenseNumber);
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));
            helpers.setText('modifiedDate', data.modifiedDate ? helpers.formatDate(data.modifiedDate) : '-');

            if (statusBadge) {
                statusBadge.textContent = data.isActive ? 'Active' : 'Inactive';
                statusBadge.className = 'badge ' + (data.isActive ? 'bg-success' : 'bg-secondary');
            }
        }
    });
})(window, document);
