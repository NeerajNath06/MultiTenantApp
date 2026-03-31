(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var page = document.getElementById('expensesIndexPage');
    if (!page) {
        return;
    }

    function setStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function setCurrencyStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = 'Rs ' + Number(value || 0).toFixed(2);
        }
    }

    function syncFiltersFromQuery() {
        var url = new URL(window.location.href);
        var filterForm = document.getElementById('expensesFilterForm');
        if (!filterForm) {
            return;
        }

        ['search', 'category', 'status'].forEach(function (name) {
            var field = filterForm.elements.namedItem(name);
            if (field) {
                field.value = url.searchParams.get(name) || '';
            }
        });
    }

    syncFiltersFromQuery();

    window.AppCrud.createListPage({
        pageId: 'expensesIndexPage',
        filterFormId: 'expensesFilterForm',
        tableWrapperId: 'expensesTableWrapper',
        recordCountId: 'expensesRecordCount',
        paginationContainerId: 'expensesPaginationContainer',
        paginationSummaryId: 'expensesPaginationSummary',
        paginationId: 'expensesPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Expenses',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            search: '',
            category: '',
            status: '',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var approvedCount = 0;
            var visibleAmount = 0;

            (items || []).forEach(function (expense) {
                if (expense.status === 'Approved') {
                    approvedCount += 1;
                }
                visibleAmount += Number(expense.amount || 0);
            });

            setStat('total', data.totalCount || items.length);
            setStat('approved', approvedCount);
            setCurrencyStat('amount', visibleAmount);
        },
        renderEmptyState: function () {
            return [
                '<div class="text-center py-5">',
                '<i class="fas fa-receipt fa-3x text-muted mb-3"></i>',
                '<h5 class="text-muted">No expenses found</h5>',
                '<a href="', page.getAttribute('data-create-url'), '" class="btn btn-primary mt-3">',
                '<i class="fas fa-receipt me-2"></i>Add Expense',
                '</a>',
                '</div>'
            ].join('');
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');

            function statusBadge(status) {
                var className = status === 'Approved'
                    ? 'success'
                    : status === 'Paid'
                        ? 'info'
                        : status === 'Rejected'
                            ? 'danger'
                            : 'warning';
                return '<span class="badge bg-' + className + '">' + context.escapeHtml(status || '-') + '</span>';
            }

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>Expense Number</th>',
                '<th>Date</th>',
                '<th>Category</th>',
                '<th>Description</th>',
                '<th>Site/Guard</th>',
                '<th class="text-end">Amount</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (expense) {
                    var tags = [];
                    if (expense.siteName) {
                        tags.push('<span class="badge bg-info">' + context.escapeHtml(expense.siteName) + '</span>');
                    }
                    if (expense.guardName) {
                        tags.push('<span class="badge bg-secondary">' + context.escapeHtml(expense.guardName) + '</span>');
                    }

                    return [
                        '<tr>',
                        '<td><strong>', context.escapeHtml(expense.expenseNumber), '</strong></td>',
                        '<td>', context.formatDate(expense.expenseDate), '</td>',
                        '<td>', context.escapeHtml(expense.category), '</td>',
                        '<td>', context.escapeHtml(expense.description), '</td>',
                        '<td>', tags.length ? tags.join(' ') : '<span class="text-muted">-</span>', '</td>',
                        '<td class="text-end"><strong>Rs ', Number(expense.amount || 0).toFixed(2), '</strong></td>',
                        '<td>', statusBadge(expense.status), '</td>',
                        '<td class="text-end">',
                        '<a href="', context.replaceToken(detailsBase, '__id__', expense.id), '" class="btn btn-sm btn-outline-primary" title="View"><i class="fas fa-eye"></i></a> ',
                        '<button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="', expense.id, '"><i class="fas fa-trash"></i></button>',
                        '</td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this expense?';
        },
        deleteSuccessMessage: 'Expense deleted successfully.',
        loadErrorMessage: 'Unable to load expenses right now.'
    });
})(window, document);
