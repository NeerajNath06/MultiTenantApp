(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'sitePostDetailsPage',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');

            helpers.setText('siteName', data.siteName);
            helpers.setText('branchName', data.branchName);
            helpers.setText('postCode', data.postCode);
            helpers.setText('postName', data.postName);
            helpers.setText('shiftName', data.shiftName);
            helpers.setText('sanctionedStrength', data.sanctionedStrength);
            helpers.setText('weeklyOffPattern', data.weeklyOffPattern);
            helpers.setText('genderRequirement', data.genderRequirement);
            helpers.setText('skillRequirement', data.skillRequirement);
            helpers.setText('requiresWeapon', data.requiresWeapon ? 'Yes' : 'No');
            helpers.setText('relieverRequired', data.relieverRequired ? 'Yes' : 'No');
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));
            helpers.setText('modifiedDate', data.modifiedDate ? helpers.formatDate(data.modifiedDate) : '-');

            if (statusBadge) {
                statusBadge.textContent = data.isActive ? 'Active' : 'Inactive';
                statusBadge.className = 'badge ' + (data.isActive ? 'bg-success' : 'bg-secondary');
            }
        }
    });
})(window, document);
