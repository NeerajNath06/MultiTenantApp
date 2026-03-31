(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('guardAssignmentsIndexPage');
    if (!page) {
        return;
    }

    var pageSizeSelect = document.getElementById('guardAssignmentsPageSize');
    var showingSummary = document.getElementById('guardAssignmentsShowingSummary');

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

        var url = new URL(window.location.href);
        var currentValue = url.searchParams.get(select.name) || '';
        select.innerHTML = '<option value="">' + placeholder + '</option>' + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(labelSelector(item)) + '</option>';
        }).join('');
        select.value = currentValue;
    }

    Promise.all([
        window.AppApi.get('SecurityGuards', { includeInactive: 'false', pageSize: '1000' }),
        window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' })
    ]).then(function (results) {
        populateSelect(document.getElementById('guardAssignmentsGuardId'), results[0].items || [], 'All Guards', function (item) { return item.guardCode || ''; });
        populateSelect(document.getElementById('guardAssignmentsSiteId'), results[1].items || [], 'All Sites', function (item) { return item.siteName || ''; });
    });

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
        pageId: 'guardAssignmentsIndexPage',
        filterFormId: 'guardAssignmentsFilterForm',
        tableWrapperId: 'guardAssignmentsTableWrapper',
        recordCountId: 'guardAssignmentsRecordCount',
        paginationContainerId: 'guardAssignmentsPaginationContainer',
        paginationSummaryId: 'guardAssignmentsPaginationSummary',
        paginationId: 'guardAssignmentsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'GuardAssignments',
        buildQuery: function (query) {
            var next = Object.assign({}, query, { includeInactive: 'false' });
            var supervisorId = page.getAttribute('data-supervisor-id');
            if (supervisorId) {
                next.supervisorId = supervisorId;
            }
            return next;
        },
        queryDefaults: {
            pageNumber: '1',
            pageSize: pageSizeSelect ? pageSizeSelect.value : '10',
            search: '',
            sortBy: '',
            sortDirection: 'desc',
            guardId: '',
            siteId: ''
        },
        updateStats: function (data, items) {
            var active = 0;
            var completed = 0;

            (items || []).forEach(function (item) {
                if (item.status === 'Active') {
                    active += 1;
                }
                if (item.status === 'Completed') {
                    completed += 1;
                }
            });

            setStat('total', data.totalCount || items.length);
            setStat('active', active);
            setStat('completed', completed);

            if (showingSummary) {
                if (!items.length) {
                    showingSummary.textContent = 'No assignments to display';
                } else {
                    var start = ((data.pageNumber - 1) * data.pageSize) + 1;
                    var end = Math.min(data.pageNumber * data.pageSize, data.totalCount);
                    showingSummary.textContent = 'Showing ' + start + ' to ' + end + ' of ' + data.totalCount + ' assignments';
                }
            }
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-user-check fa-3x text-muted mb-3"></i><h5 class="text-muted">No assignments found</h5><a href="' +
                page.getAttribute('data-create-url') +
                '" class="btn btn-primary mt-3"><i class="fas fa-user-check me-2"></i>Assign Guard</a></div>';
        },
        renderTable: function (context) {
            return [
                '<div class="table-responsive"><table class="table table-hover align-middle"><thead class="table-light"><tr>',
                '<th>Guard</th><th>Site</th><th>Shift</th><th>', sortLink(context, 'date', 'Start Date'), '</th><th>End Date</th><th>', sortLink(context, 'status', 'Status'), '</th>',
                '</tr></thead><tbody>',
                context.items.map(function (item) {
                    var statusHtml = item.status === 'Active'
                        ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Active</span>'
                        : item.status === 'Completed'
                            ? '<span class="badge bg-info"><i class="fas fa-check me-1"></i>Completed</span>'
                            : '<span class="badge bg-danger"><i class="fas fa-times me-1"></i>Cancelled</span>';

                    return '<tr>' +
                        '<td><div class="fw-semibold">' + context.escapeHtml(item.guardName) + '</div><small class="text-muted">' + context.escapeHtml(item.guardCode) + '</small></td>' +
                        '<td>' + context.escapeHtml(item.siteName) + '</td>' +
                        '<td>' + context.escapeHtml(item.shiftName) + ((item.shiftStartTime || item.shiftEndTime) ? '<small class="text-muted d-block">' + context.escapeHtml(item.shiftStartTime || '—') + ' - ' + context.escapeHtml(item.shiftEndTime || '—') + '</small>' : '') + '</td>' +
                        '<td>' + context.formatDate(item.assignmentStartDate) + '</td>' +
                        '<td>' + (item.assignmentEndDate ? context.formatDate(item.assignmentEndDate) : 'Ongoing') + '</td>' +
                        '<td>' + statusHtml + '</td>' +
                        '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        },
        loadErrorMessage: 'Unable to load assignments right now.'
    });

    if (pageSizeSelect) {
        var url = new URL(window.location.href);
        pageSizeSelect.value = url.searchParams.get('pageSize') || '10';
        pageSizeSelect.addEventListener('change', function () {
            var nextUrl = new URL(window.location.href);
            nextUrl.searchParams.set('pageSize', pageSizeSelect.value);
            nextUrl.searchParams.set('pageNumber', '1');
            window.location.href = nextUrl.toString();
        });
    }
})(window, document);
