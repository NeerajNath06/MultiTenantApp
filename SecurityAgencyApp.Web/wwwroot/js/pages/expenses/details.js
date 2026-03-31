(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'expenseDetailsPage',
        deleteButtonId: 'expenseDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'Expenses/' + page.getAttribute('data-expense-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this expense?';
        },
        deleteSuccessMessage: 'Expense deleted successfully.',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');

            helpers.setText('expenseNumber', data.expenseNumber);
            helpers.setText('expenseDate', helpers.formatDate(data.expenseDate));
            helpers.setText('category', data.category);
            helpers.setText('description', data.description);
            helpers.setText('amount', 'Rs ' + Number(data.amount || 0).toFixed(2));
            helpers.setText('paymentMethod', data.paymentMethod);
            helpers.setText('vendorName', data.vendorName);
            helpers.setText('receiptNumber', data.receiptNumber);
            helpers.setText('siteName', data.siteName);
            helpers.setText('guardName', data.guardName);
            helpers.setText('notes', data.notes);
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));

            if (statusBadge) {
                statusBadge.textContent = data.status || '-';
                statusBadge.className = 'badge ' + (
                    data.status === 'Approved'
                        ? 'bg-success'
                        : data.status === 'Paid'
                            ? 'bg-info'
                            : data.status === 'Rejected'
                                ? 'bg-danger'
                                : 'bg-warning'
                );
            }
        }
    });
})(window, document);
