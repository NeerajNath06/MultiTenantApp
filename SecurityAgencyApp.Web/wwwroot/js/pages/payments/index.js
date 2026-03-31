(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('paymentsIndexPage');
    if (!page) {
        return;
    }

    function setStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function populateSelect(select, items, placeholder, labelSelector) {
        if (!select) {
            return;
        }

        var currentValue = select.value;
        select.innerHTML = '<option value="">' + placeholder + '</option>' + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(labelSelector(item)) + '</option>';
        }).join('');

        if (currentValue) {
            select.value = currentValue;
        }
    }

    function bindQueryToFilters() {
        var params = new URLSearchParams(window.location.search);
        var pageSizeInput = page.querySelector('input[name="pageSize"]');

        document.getElementById('paymentsSearch').value = params.get('search') || '';
        document.getElementById('paymentsBillId').value = params.get('billId') || '';
        document.getElementById('paymentsClientId').value = params.get('clientId') || '';
        document.getElementById('paymentsStatus').value = params.get('status') || '';

        if (pageSizeInput) {
            pageSizeInput.value = params.get('pageSize') || '10';
        }
    }

    Promise.all([
        window.AppApi.get('Bills', { includeInactive: 'false', pageSize: '1000' }),
        window.AppApi.get('Clients', { includeInactive: 'false', pageSize: '1000' })
    ]).then(function (results) {
        populateSelect(document.getElementById('paymentsBillId'), results[0].items || [], 'All Bills', function (item) {
            return item.billNumber || '';
        });
        populateSelect(document.getElementById('paymentsClientId'), results[1].items || [], 'All Clients', function (item) {
            return item.companyName || '';
        });
        bindQueryToFilters();
    });

    window.AppCrud.createListPage({
        pageId: 'paymentsIndexPage',
        filterFormId: 'paymentsFilterForm',
        tableWrapperId: 'paymentsTableWrapper',
        recordCountId: 'paymentsRecordCount',
        paginationContainerId: 'paymentsPaginationContainer',
        paginationSummaryId: 'paymentsPaginationSummary',
        paginationId: 'paymentsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Payments',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var completedCount = 0;
            var totalAmount = 0;

            (items || []).forEach(function (item) {
                if (String(item.status || '').toLowerCase() === 'completed') {
                    completedCount += 1;
                }

                totalAmount += Number(item.amount || 0);
            });

            setStat('total', data.totalCount || items.length);
            setStat('completed', completedCount);
            var amountElement = page.querySelector('[data-stat="visibleAmount"]');
            if (amountElement) {
                amountElement.textContent = '\u20b9' + totalAmount.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            }
        },
        renderEmptyState: function () {
            return [
                '<div class="text-center py-5">',
                '<i class="fas fa-money-check fa-3x text-muted mb-3"></i>',
                '<h5 class="text-muted">No payments found</h5>',
                '<a href="', page.getAttribute('data-create-url'), '" class="btn btn-primary mt-3">',
                '<i class="fas fa-money-check me-2"></i>Record Payment',
                '</a>',
                '</div>'
            ].join('');
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            var editBase = page.getAttribute('data-edit-base');

            function getStatusBadge(status) {
                var normalized = String(status || '').toLowerCase();
                var className = normalized === 'completed'
                    ? 'success'
                    : normalized === 'pending'
                        ? 'warning'
                        : 'danger';

                return '<span class="badge bg-' + className + '">' + context.escapeHtml(status || '-') + '</span>';
            }

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>Payment Number</th>',
                '<th>Payment Date</th>',
                '<th>Client</th>',
                '<th>Bill Number</th>',
                '<th class="text-end">Amount</th>',
                '<th>Payment Method</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (payment) {
                    var paymentDate = payment.paymentDate
                        ? new Date(payment.paymentDate).toLocaleDateString()
                        : '-';

                    return [
                        '<tr>',
                        '<td><strong>' + context.escapeHtml(payment.paymentNumber) + '</strong></td>',
                        '<td>' + context.escapeHtml(paymentDate) + '</td>',
                        '<td>' + context.escapeHtml(payment.clientName || '-') + '</td>',
                        '<td>' + context.escapeHtml(payment.billNumber || '-') + '</td>',
                        '<td class="text-end"><strong>\u20b9' + Number(payment.amount || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '</strong></td>',
                        '<td>' + context.escapeHtml(payment.paymentMethod || '-') + '</td>',
                        '<td>' + getStatusBadge(payment.status) + '</td>',
                        '<td class="text-end"><div class="btn-group btn-group-sm">',
                        '<a href="' + context.replaceToken(detailsBase, '__id__', payment.id) + '" class="btn btn-outline-primary" title="View"><i class="fas fa-eye"></i></a>',
                        '<a href="' + context.replaceToken(editBase, '__id__', payment.id) + '" class="btn btn-outline-secondary" title="Edit"><i class="fas fa-edit"></i></a>',
                        '<button type="button" class="btn btn-outline-danger" title="Delete" data-crud-delete-id="' + payment.id + '"><i class="fas fa-trash"></i></button>',
                        '</div></td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this payment?';
        },
        deleteSuccessMessage: 'Payment deleted successfully.',
        loadErrorMessage: 'Unable to load payments right now.'
    });
})(window, document);
