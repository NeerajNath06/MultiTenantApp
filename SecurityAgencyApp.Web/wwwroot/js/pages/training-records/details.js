(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'trainingRecordDetailsPage',
        deleteButtonId: 'trainingRecordDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'TrainingRecords/' + page.getAttribute('data-training-record-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this training record?';
        },
        deleteSuccessMessage: 'Training record deleted successfully.',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');

            helpers.setText('guardName', data.guardName);
            helpers.setText('guardCode', data.guardCode);
            helpers.setText('trainingType', data.trainingType);
            helpers.setText('trainingName', data.trainingName);
            helpers.setText('trainingProvider', data.trainingProvider);
            helpers.setText('trainingDate', helpers.formatDate(data.trainingDate));
            helpers.setText('expiryDate', data.expiryDate ? helpers.formatDate(data.expiryDate) : '-');
            helpers.setText('certificateNumber', data.certificateNumber);
            helpers.setText('score', data.score == null ? '-' : Number(data.score).toFixed(2));
            helpers.setText('remarks', data.remarks);
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));

            if (statusBadge) {
                statusBadge.textContent = data.status || '-';
                statusBadge.className = 'badge ' + (
                    data.status === 'Completed'
                        ? 'bg-success'
                        : data.status === 'Expired'
                            ? 'bg-danger'
                            : 'bg-warning'
                );
            }
        }
    });
})(window, document);
