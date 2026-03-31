(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var page = document.getElementById('departmentsIndexPage');
    var stats = document.getElementById('departmentsStats');
    var showingSummary = document.getElementById('departmentsShowingSummary');
    var pageSizeSelect = document.getElementById('departmentsPageSize');

    function setStat(name, value) {
        if (!stats) {
            return;
        }

        var element = stats.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function sortLink(context, column) {
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

        return '<a href="' + context.indexUrl + '?' + context.buildQueryString(query) + '" class="text-decoration-none text-dark">' +
            column.charAt(0).toUpperCase() + column.slice(1) + ' ' + icon + '</a>';
    }

    window.AppCrud.createListPage({
        pageId: 'departmentsIndexPage',
        filterFormId: 'departmentsFilterForm',
        tableWrapperId: 'departmentsTableWrapper',
        recordCountId: 'departmentsRecordCount',
        paginationContainerId: 'departmentsPaginationContainer',
        paginationSummaryId: 'departmentsPaginationSummary',
        paginationId: 'departmentsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Departments',
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
            var totalUsers = 0;

            (items || []).forEach(function (item) {
                if (item.isActive) {
                    activeCount += 1;
                }

                totalUsers += item.userCount || 0;
            });

            setStat('total', data.totalCount || 0);
            setStat('active', activeCount);
            setStat('users', totalUsers);

            if (showingSummary) {
                if (!items.length) {
                    showingSummary.textContent = 'No departments to display';
                } else {
                    var start = ((data.pageNumber - 1) * data.pageSize) + 1;
                    var end = Math.min(data.pageNumber * data.pageSize, data.totalCount);
                    showingSummary.textContent = 'Showing ' + start + ' to ' + end + ' of ' + data.totalCount + ' departments';
                }
            }
        },
        renderEmptyState: function () {
            return [
                '<div class="text-center py-5">',
                '<i class="fas fa-building fa-3x text-muted mb-3"></i>',
                '<h5 class="text-muted">No departments found</h5>',
                '<p class="text-muted">Get started by creating your first department.</p>',
                '<a href="/Departments/Create" class="btn btn-primary mt-3"><i class="fas fa-building me-2"></i>Add New Department</a>',
                '</div>'
            ].join('');
        },
        renderTable: function (context) {
            var editBase = context.page.getAttribute('data-edit-base');

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>', sortLink(context, 'code'), '</th>',
                '<th>', sortLink(context, 'name'), '</th>',
                '<th>Description</th>',
                '<th>Users</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (dept) {
                    return [
                        '<tr>',
                        '<td><span class="badge bg-secondary">', context.escapeHtml(dept.code), '</span></td>',
                        '<td><div class="fw-semibold">', context.escapeHtml(dept.name), '</div></td>',
                        '<td>', context.escapeHtml(dept.description || '-'), '</td>',
                        '<td><span class="badge bg-info"><i class="fas fa-users me-1"></i>', context.escapeHtml(dept.userCount || 0), '</span></td>',
                        '<td>',
                        dept.isActive
                            ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Active</span>'
                            : '<span class="badge bg-danger"><i class="fas fa-times-circle me-1"></i>Inactive</span>',
                        '</td>',
                        '<td class="text-end">',
                        '<div class="btn-group" role="group">',
                        '<a href="', context.replaceToken(editBase, '__id__', dept.id), '" class="btn btn-sm btn-outline-warning" title="Edit"><i class="fas fa-edit"></i></a>',
                        '<button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="', dept.id, '"><i class="fas fa-trash"></i></button>',
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
            return 'Are you sure you want to delete department "' + ((item && item.name) || 'this department') + '"?';
        },
        deleteSuccessMessage: 'Department deleted successfully.',
        loadErrorMessage: 'Unable to load departments right now.'
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
