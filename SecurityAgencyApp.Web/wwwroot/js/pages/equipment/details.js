(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'equipmentDetailsPage',
        deleteButtonId: 'equipmentDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'Equipment/' + page.getAttribute('data-equipment-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this equipment?';
        },
        deleteSuccessMessage: 'Equipment deleted successfully.',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');

            helpers.setText('equipmentCode', data.equipmentCode);
            helpers.setText('equipmentName', data.equipmentName);
            helpers.setText('category', data.category);
            helpers.setText('manufacturer', data.manufacturer);
            helpers.setText('modelNumber', data.modelNumber);
            helpers.setText('serialNumber', data.serialNumber);
            helpers.setText('purchaseDate', helpers.formatDate(data.purchaseDate));
            helpers.setText('purchaseCost', 'Rs ' + Number(data.purchaseCost || 0).toFixed(2));
            helpers.setText('assignedToGuardName', data.assignedToGuardName);
            helpers.setText('assignedToSiteName', data.assignedToSiteName);
            helpers.setText('nextMaintenanceDate', data.nextMaintenanceDate ? helpers.formatDate(data.nextMaintenanceDate) : '-');
            helpers.setText('notes', data.notes);
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));

            if (statusBadge) {
                statusBadge.textContent = data.status || '-';
                statusBadge.className = 'badge ' + (
                    data.status === 'Available'
                        ? 'bg-success'
                        : data.status === 'Assigned'
                            ? 'bg-info'
                            : data.status === 'Maintenance'
                                ? 'bg-warning'
                                : 'bg-danger'
                );
            }
        }
    });
})(window, document);
