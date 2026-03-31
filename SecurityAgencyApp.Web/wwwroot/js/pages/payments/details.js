(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'paymentDetailsPage',
        deleteButtonId: 'paymentDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'Payments/' + page.getAttribute('data-payment-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this payment?';
        },
        deleteSuccessMessage: 'Payment deleted successfully.',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');

            helpers.setText('paymentNumber', data.paymentNumber);
            helpers.setText('paymentDate', data.paymentDate ? new Date(data.paymentDate).toLocaleDateString() : '-');
            helpers.setText('amount', '\u20b9' + Number(data.amount || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
            helpers.setText('paymentMethod', data.paymentMethod);
            helpers.setText('billNumber', data.billNumber);
            helpers.setText('clientName', data.clientName);
            helpers.setText('chequeNumber', data.chequeNumber);
            helpers.setText('bankName', data.bankName);
            helpers.setText('transactionReference', data.transactionReference);
            helpers.setText('receivedDate', data.receivedDate ? new Date(data.receivedDate).toLocaleDateString() : '-');
            helpers.setText('notes', data.notes);
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));
            helpers.setText('modifiedDate', data.modifiedDate ? helpers.formatDate(data.modifiedDate) : '-');

            if (statusBadge) {
                var normalized = String(data.status || '').toLowerCase();
                var className = normalized === 'completed'
                    ? 'success'
                    : normalized === 'failed' || normalized === 'cancelled'
                        ? 'danger'
                        : 'warning';
                statusBadge.textContent = data.status || '-';
                statusBadge.className = 'badge bg-' + className;
            }
        }
    });
})(window, document);
