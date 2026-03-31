(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var stats = document.getElementById('clientsStats');

    function setStat(name, value, selectorPrefix) {
        if (!stats) {
            return;
        }

        var element = stats.querySelector('[' + (selectorPrefix || 'data-stat') + '="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    window.AppCrud.createListPage({
        pageId: 'clientsIndexPage',
        filterFormId: 'clientsFilterForm',
        tableWrapperId: 'clientsTableWrapper',
        recordCountId: 'clientsRecordCount',
        paginationContainerId: 'clientsPaginationContainer',
        paginationSummaryId: 'clientsPaginationSummary',
        paginationId: 'clientsPagination',
        indexUrl: document.getElementById('clientsIndexPage').getAttribute('data-index-url'),
        endpoint: 'Clients',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            search: '',
            status: '',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var activeCount = 0;
            var suspendedCount = 0;

            (items || []).forEach(function (item) {
                if ((item.status || '').toLowerCase() === 'active') {
                    activeCount += 1;
                } else if ((item.status || '').toLowerCase() === 'suspended') {
                    suspendedCount += 1;
                }
            });

            setStat('total', data.totalCount || 0);
            setStat('active', activeCount);
            setStat('suspended', suspendedCount);
        },
        renderEmptyState: function () {
            return [
            '<div class="text-center py-5">',
            '<i class="fas fa-building fa-3x text-muted mb-3 opacity-50"></i>',
            '<h5 class="text-muted">No clients found</h5>',
            '<p class="text-muted small mb-0">Try adjusting your search or add a new client.</p>',
            '<a href="/Clients/Create" class="btn btn-primary mt-3"><i class="fas fa-plus me-2"></i>Add New Client</a>',
            '</div>'
        ].join('');
        },
        renderTable: function (context) {
            var detailsBase = context.page.getAttribute('data-details-base');
            var editBase = context.page.getAttribute('data-edit-base');

            function getStatusBadgeClass(status) {
                if (status === 'Active') {
                    return 'success';
                }

                if (status === 'Suspended') {
                    return 'warning';
                }

                return 'secondary';
            }

            return [
            '<div class="table-responsive">',
            '<table class="table table-hover align-middle mb-0">',
            '<thead class="table-light">',
            '<tr>',
            '<th>Client Code</th>',
            '<th>Company Name</th>',
            '<th>Contact Person</th>',
            '<th>Commercial Owner</th>',
            '<th>Email</th>',
            '<th>Phone / Billing</th>',
            '<th>City</th>',
            '<th>Status</th>',
            '<th class="text-end">Actions</th>',
            '</tr>',
            '</thead>',
            '<tbody>',
            context.items.map(function (client) {
                return [
                    '<tr>',
                    '<td><strong>', context.escapeHtml(client.clientCode), '</strong></td>',
                    '<td>', context.escapeHtml(client.companyName), '</td>',
                    '<td>', context.escapeHtml(client.contactPerson || '–'), '</td>',
                    '<td><div>', context.escapeHtml(client.accountManagerName || '–'), '</div><small class="text-muted">', context.escapeHtml(client.billingContactName || '–'), '</small></td>',
                    '<td>', context.escapeHtml(client.email || '–'), '</td>',
                    '<td><div>', context.escapeHtml(client.phoneNumber || '–'), '</div><small class="text-muted">Billing: ', context.escapeHtml(client.billingContactName || '–'), '</small></td>',
                    '<td>', context.escapeHtml(client.city || '–'), '</td>',
                    '<td><span class="badge bg-', getStatusBadgeClass(client.status), '">', context.escapeHtml(client.status || 'Unknown'), '</span></td>',
                    '<td class="text-end">',
                    '<div class="action-btns justify-content-end">',
                    '<a href="', context.replaceToken(detailsBase, '__id__', client.id), '" class="btn btn-sm btn-outline-primary" title="View"><i class="fas fa-eye"></i></a>',
                    '<a href="', context.replaceToken(editBase, '__id__', client.id), '" class="btn btn-sm btn-outline-secondary" title="Edit"><i class="fas fa-edit"></i></a>',
                    '<button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="', client.id, '"><i class="fas fa-trash"></i></button>',
                    '</div>',
                    '</td>',
                    '</tr>'
                ].join('');
            }).join(''),
            '</tbody>',
            '</table>',
            '</div>'
        ].join('');
        },
        getDeleteMessage: function (item) {
            return 'Are you sure you want to delete "' + ((item && item.companyName) || 'this client') + '"?';
        },
        deleteSuccessMessage: 'Client deleted successfully.',
        loadErrorMessage: 'Unable to load clients right now.'
    });
})(window, document);
