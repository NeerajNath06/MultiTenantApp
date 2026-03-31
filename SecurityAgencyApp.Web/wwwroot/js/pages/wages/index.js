(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('wagesIndexPage');
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
        document.getElementById('wagesSearch').value = params.get('search') || '';
        document.getElementById('wagesStatus').value = params.get('status') || '';
    }

    bindFilters();

    window.AppCrud.createListPage({
        pageId: 'wagesIndexPage',
        filterFormId: 'wagesFilterForm',
        tableWrapperId: 'wagesTableWrapper',
        recordCountId: 'wagesRecordCount',
        paginationContainerId: 'wagesPaginationContainer',
        paginationSummaryId: 'wagesPaginationSummary',
        paginationId: 'wagesPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Wages',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var paidCount = 0;
            var visibleNet = 0;
            (items || []).forEach(function (item) {
                if (String(item.status || '').toLowerCase() === 'paid') {
                    paidCount += 1;
                }
                visibleNet += Number(item.netAmount || 0);
            });

            setStat('total', data.totalCount || items.length);
            setStat('paid', paidCount);
            var visibleNetAmount = page.querySelector('[data-stat="visibleNetAmount"]');
            if (visibleNetAmount) {
                visibleNetAmount.textContent = '\u20b9' + visibleNet.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            }
        },
        renderEmptyState: function () {
            return [
                '<div class="text-center py-5">',
                '<i class="fas fa-money-bill-wave fa-3x text-muted mb-3"></i>',
                '<h5 class="text-muted">No wage sheets found</h5>',
                '<a href="' + page.getAttribute('data-create-url') + '" class="btn btn-primary mt-3"><i class="fas fa-money-bill-wave me-2"></i>Create New Wage Sheet</a>',
                '</div>'
            ].join('');
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>Wage Sheet Number</th>',
                '<th>Period</th>',
                '<th>Payment Date</th>',
                '<th class="text-end">Total Wages</th>',
                '<th class="text-end">Net Amount</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (wage) {
                    var normalized = String(wage.status || '').toLowerCase();
                    var className = normalized === 'paid' ? 'success' : normalized === 'approved' ? 'info' : 'secondary';
                    return [
                        '<tr>',
                        '<td><strong>' + context.escapeHtml(wage.wageSheetNumber || '-') + '</strong></td>',
                        '<td>' + context.escapeHtml(new Date(wage.wagePeriodStart).toLocaleDateString() + ' - ' + new Date(wage.wagePeriodEnd).toLocaleDateString()) + '</td>',
                        '<td>' + context.escapeHtml(wage.paymentDate ? new Date(wage.paymentDate).toLocaleDateString() : '-') + '</td>',
                        '<td class="text-end">\u20b9' + Number(wage.totalWages || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '</td>',
                        '<td class="text-end"><strong>\u20b9' + Number(wage.netAmount || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '</strong></td>',
                        '<td><span class="badge bg-' + className + '">' + context.escapeHtml(wage.status || '-') + '</span></td>',
                        '<td class="text-end"><a href="' + context.replaceToken(detailsBase, '__id__', wage.id) + '" class="btn btn-sm btn-outline-primary" title="View"><i class="fas fa-eye"></i></a></td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        loadErrorMessage: 'Unable to load wage sheets right now.'
    });
})(window, document);
