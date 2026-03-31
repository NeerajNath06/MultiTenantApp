(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var page = document.getElementById('shiftsIndexPage');
    var stats = document.getElementById('shiftsStats');
    var showingSummary = document.getElementById('shiftsShowingSummary');
    var pageSizeSelect = document.getElementById('shiftsPageSize');

    function setStat(name, value) {
        if (!stats) {
            return;
        }

        var element = stats.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function sortLink(context, column, label) {
        var nextDirection = context.query.sortBy === column && context.query.sortDirection === 'asc' ? 'desc' : 'asc';
        var query = Object.assign({}, context.query, {
            pageNumber: '1',
            sortBy: column,
            sortDirection: nextDirection
        });

        var icon = '<i class="fas fa-sort text-muted"></i>';
        if (context.query.sortBy === column) {
            icon = '<i class="fas fa-sort-' + (context.query.sortDirection === 'asc' ? 'up' : 'down') + '"></i>';
        }

        return '<a href="' + context.indexUrl + '?' + context.buildQueryString(query) + '" class="text-decoration-none text-dark">' + label + ' ' + icon + '</a>';
    }

    window.AppCrud.createListPage({
        pageId: 'shiftsIndexPage',
        filterFormId: 'shiftsFilterForm',
        tableWrapperId: 'shiftsTableWrapper',
        recordCountId: 'shiftsRecordCount',
        paginationContainerId: 'shiftsPaginationContainer',
        paginationSummaryId: 'shiftsPaginationSummary',
        paginationId: 'shiftsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Shifts',
        queryDefaults: {
            pageNumber: '1',
            pageSize: pageSizeSelect ? pageSizeSelect.value : '10',
            search: '',
            sortBy: '',
            sortDirection: 'asc',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var activeCount = 0;
            var totalBreak = 0;

            (items || []).forEach(function (item) {
                if (item.isActive) {
                    activeCount += 1;
                }
                totalBreak += item.breakDuration || 0;
            });

            setStat('total', data.totalCount || 0);
            setStat('active', activeCount);
            setStat('break', totalBreak);

            if (showingSummary) {
                if (!items.length) {
                    showingSummary.textContent = 'No shifts to display';
                } else {
                    var start = ((data.pageNumber - 1) * data.pageSize) + 1;
                    var end = Math.min(data.pageNumber * data.pageSize, data.totalCount);
                    showingSummary.textContent = 'Showing ' + start + ' to ' + end + ' of ' + data.totalCount + ' shifts';
                }
            }
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-clock fa-3x text-muted mb-3"></i><h5 class="text-muted">No shifts found</h5><a href="/Shifts/Create" class="btn btn-primary mt-3"><i class="fas fa-clock me-2"></i>Add New Shift</a></div>';
        },
        renderTable: function (context) {
            var editBase = context.page.getAttribute('data-edit-base');

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>', sortLink(context, 'name', 'Shift Name'), '</th>',
                '<th>', sortLink(context, 'start', 'Start Time'), '</th>',
                '<th>', sortLink(context, 'end', 'End Time'), '</th>',
                '<th>Break Duration</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (shift) {
                    return [
                        '<tr>',
                        '<td><div class="fw-semibold">', context.escapeHtml(shift.shiftName), '</div></td>',
                        '<td>', context.escapeHtml(shift.startTime), '</td>',
                        '<td>', context.escapeHtml(shift.endTime), '</td>',
                        '<td>', context.escapeHtml(shift.breakDuration), ' minutes</td>',
                        '<td>', shift.isActive ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Active</span>' : '<span class="badge bg-danger"><i class="fas fa-times-circle me-1"></i>Inactive</span>', '</td>',
                        '<td class="text-end"><div class="btn-group" role="group"><a href="', context.replaceToken(editBase, '__id__', shift.id), '" class="btn btn-sm btn-outline-warning" title="Edit"><i class="fas fa-edit"></i></a><button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="', shift.id, '"><i class="fas fa-trash"></i></button></div></td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        getDeleteMessage: function (item) {
            return 'Are you sure you want to delete shift "' + ((item && item.shiftName) || 'this shift') + '"?';
        },
        deleteSuccessMessage: 'Shift deleted successfully.',
        loadErrorMessage: 'Unable to load shifts right now.'
    });

    if (pageSizeSelect) {
        pageSizeSelect.addEventListener('change', function () {
            var url = new URL(window.location.href);
            url.searchParams.set('pageSize', pageSizeSelect.value);
            url.searchParams.set('pageNumber', '1');
            window.location.href = url.toString();
        });
    }
})(window, document);
