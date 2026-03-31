(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('billsIndexPage');
    if (!page) {
        return;
    }

    function setStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function bindFilters() {
        var params = new URLSearchParams(window.location.search);
        document.getElementById('billsSearch').value = params.get('search') || '';
        document.getElementById('billsStatus').value = params.get('status') || '';
    }

    bindFilters();

    window.AppCrud.createListPage({
        pageId: 'billsIndexPage',
        filterFormId: 'billsFilterForm',
        tableWrapperId: 'billsTableWrapper',
        recordCountId: 'billsRecordCount',
        paginationContainerId: 'billsPaginationContainer',
        paginationSummaryId: 'billsPaginationSummary',
        paginationId: 'billsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Bills',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var paid = 0;
            var overdue = 0;
            (items || []).forEach(function (item) {
                var status = String(item.status || '').toLowerCase();
                if (status === 'paid') {
                    paid += 1;
                }
                if (status === 'overdue') {
                    overdue += 1;
                }
            });
            setStat('total', data.totalCount || items.length);
            setStat('paid', paid);
            setStat('overdue', overdue);
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-file-invoice fa-3x text-muted mb-3"></i><h5 class="text-muted">No bills found</h5><a href="' + page.getAttribute('data-create-url') + '" class="btn btn-primary mt-3"><i class="fas fa-file-invoice me-2"></i>Create New Bill</a></div>';
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            var editBase = page.getAttribute('data-edit-base');
            return [
                '<div class="table-responsive"><table class="table table-hover align-middle"><thead class="table-light"><tr>',
                '<th>Bill Number</th><th>Bill Date</th><th>Client Name</th><th>Site</th><th class="text-end">Total Amount</th><th>Status</th><th>Due Date</th><th class="text-end">Actions</th>',
                '</tr></thead><tbody>',
                context.items.map(function (bill) {
                    var status = String(bill.status || '').toLowerCase();
                    var statusClass = status === 'paid' ? 'success' : status === 'overdue' ? 'danger' : status === 'sent' ? 'info' : 'secondary';
                    return '<tr>'
                        + '<td><strong>' + context.escapeHtml(bill.billNumber) + '</strong></td>'
                        + '<td>' + context.escapeHtml(new Date(bill.billDate).toLocaleDateString()) + '</td>'
                        + '<td>' + context.escapeHtml(bill.clientName) + '</td>'
                        + '<td>' + context.escapeHtml(bill.siteName || '-') + '</td>'
                        + '<td class="text-end"><strong>₹' + Number(bill.totalAmount || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '</strong></td>'
                        + '<td><span class="badge bg-' + statusClass + '">' + context.escapeHtml(bill.status || '-') + '</span></td>'
                        + '<td>' + context.escapeHtml(bill.dueDate ? new Date(bill.dueDate).toLocaleDateString() : '-') + '</td>'
                        + '<td class="text-end"><div class="btn-group btn-group-sm">'
                        + '<a href="' + context.replaceToken(detailsBase, '__id__', bill.id) + '" class="btn btn-outline-primary" title="View"><i class="fas fa-eye"></i></a>'
                        + '<a href="' + context.replaceToken(editBase, '__id__', bill.id) + '" class="btn btn-outline-secondary" title="Edit"><i class="fas fa-edit"></i></a>'
                        + '<button type="button" class="btn btn-outline-danger" title="Delete" data-crud-delete-id="' + bill.id + '"><i class="fas fa-trash"></i></button>'
                        + '</div></td>'
                        + '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this bill?';
        },
        deleteSuccessMessage: 'Bill deleted successfully.',
        loadErrorMessage: 'Unable to load bills right now.'
    });
})(window, document);
