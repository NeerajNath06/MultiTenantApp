(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    function money(value) {
        return '₹' + Number(value || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    window.AppCrud.createDetailsPage({
        pageId: 'billDetailsPage',
        deleteButtonId: 'billDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'Bills/' + page.getAttribute('data-bill-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this bill?';
        },
        deleteSuccessMessage: 'Bill deleted successfully.',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');
            var itemsWrapper = document.getElementById('billItemsWrapper');

            helpers.setText('billNumber', data.billNumber);
            helpers.setText('billDate', data.billDate ? new Date(data.billDate).toLocaleDateString() : '-');
            helpers.setText('clientName', data.clientName);
            helpers.setText('siteName', data.siteName || '-');
            helpers.setText('dueDate', data.dueDate ? new Date(data.dueDate).toLocaleDateString() : '-');
            helpers.setText('description', data.description);
            helpers.setText('subTotal', money(data.subTotal));
            helpers.setText('discountAmount', money(data.discountAmount));
            helpers.setText('taxAmount', money(data.taxAmount));
            helpers.setText('totalAmount', money(data.totalAmount));
            helpers.setText('paymentTerms', data.paymentTerms);
            helpers.setText('notes', data.notes);
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));

            if (statusBadge) {
                var status = String(data.status || '').toLowerCase();
                var statusClass = status === 'paid' ? 'success' : status === 'overdue' ? 'danger' : status === 'sent' ? 'info' : 'secondary';
                statusBadge.textContent = data.status || '-';
                statusBadge.className = 'badge bg-' + statusClass;
            }

            if (!data.items || !data.items.length) {
                itemsWrapper.innerHTML = '<div class="text-muted">No line items found.</div>';
                return;
            }

            itemsWrapper.innerHTML = [
                '<div class="table-responsive"><table class="table table-sm"><thead><tr><th>Item Name</th><th>Description</th><th class="text-end">Quantity</th><th class="text-end">Unit Price</th><th class="text-end">Tax Rate</th><th class="text-end">Discount</th><th class="text-end">Total</th></tr></thead><tbody>',
                data.items.map(function (item) {
                    return '<tr>'
                        + '<td>' + window.AppCrud.escapeHtml(item.itemName) + '</td>'
                        + '<td>' + window.AppCrud.escapeHtml(item.description || '-') + '</td>'
                        + '<td class="text-end">' + window.AppCrud.escapeHtml(item.quantity) + '</td>'
                        + '<td class="text-end">' + money(item.unitPrice) + '</td>'
                        + '<td class="text-end">' + Number(item.taxRate || 0).toFixed(2) + '%</td>'
                        + '<td class="text-end">' + money(item.discountAmount) + '</td>'
                        + '<td class="text-end"><strong>' + money(item.totalAmount) + '</strong></td>'
                        + '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        }
    });
})(window, document);
