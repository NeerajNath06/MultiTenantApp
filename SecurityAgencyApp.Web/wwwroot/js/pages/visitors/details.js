(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'visitorDetailsPage',
        deleteButtonId: 'visitorDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'Visitors/' + page.getAttribute('data-visitor-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this visitor record?';
        },
        deleteSuccessMessage: 'Visitor deleted successfully.',
        render: function (page, data, helpers) {
            helpers.setText('visitorName', data.visitorName);
            helpers.setText('visitorType', data.visitorType);
            helpers.setText('companyName', data.companyName);
            helpers.setText('phoneNumber', data.phoneNumber);
            helpers.setText('email', data.email);
            helpers.setText('purpose', data.purpose);
            helpers.setText('hostName', data.hostName);
            helpers.setText('hostDepartment', data.hostDepartment);
            helpers.setText('siteName', data.siteName);
            helpers.setText('guardName', data.guardName);
            helpers.setText('entryTime', helpers.formatDate(data.entryTime));
            helpers.setText('exitTime', data.exitTime ? helpers.formatDate(data.exitTime) : 'Inside');
            helpers.setText('badgeNumber', data.badgeNumber);
            helpers.setText('idProof', data.idProofType ? data.idProofType + (data.idProofNumber ? ' - ' + data.idProofNumber : '') : '-');
        }
    });
})(window, document);
