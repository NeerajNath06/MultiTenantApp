(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var page = document.getElementById('designationsIndexPage');
    var stats = document.getElementById('designationsStats');
    var showingSummary = document.getElementById('designationsShowingSummary');
    var pageSizeSelect = document.getElementById('designationsPageSize');

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
        pageId: 'designationsIndexPage',
        filterFormId: 'designationsFilterForm',
        tableWrapperId: 'designationsTableWrapper',
        recordCountId: 'designationsRecordCount',
        paginationContainerId: 'designationsPaginationContainer',
        paginationSummaryId: 'designationsPaginationSummary',
        paginationId: 'designationsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Designations',
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
            var linkedDepartments = 0;

            (items || []).forEach(function (item) {
                if (item.isActive) {
                    activeCount += 1;
                }
                if (item.departmentName) {
                    linkedDepartments += 1;
                }
            });

            setStat('total', data.totalCount || 0);
            setStat('active', activeCount);
            setStat('departments', linkedDepartments);

            if (showingSummary) {
                if (!items.length) {
                    showingSummary.textContent = 'No designations to display';
                } else {
                    var start = ((data.pageNumber - 1) * data.pageSize) + 1;
                    var end = Math.min(data.pageNumber * data.pageSize, data.totalCount);
                    showingSummary.textContent = 'Showing ' + start + ' to ' + end + ' of ' + data.totalCount + ' designations';
                }
            }
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-briefcase fa-3x text-muted mb-3"></i><h5 class="text-muted">No designations found</h5><p class="text-muted">Get started by creating your first designation.</p><a href="/Designations/Create" class="btn btn-primary mt-3"><i class="fas fa-briefcase me-2"></i>Add New Designation</a></div>';
        },
        renderTable: function (context) {
            var editBase = context.page.getAttribute('data-edit-base');

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>', sortLink(context, 'code', 'Code'), '</th>',
                '<th>', sortLink(context, 'name', 'Name'), '</th>',
                '<th>Department</th>',
                '<th>Description</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (designation) {
                    return [
                        '<tr>',
                        '<td><span class="badge bg-secondary">', context.escapeHtml(designation.code), '</span></td>',
                        '<td><div class="fw-semibold">', context.escapeHtml(designation.name), '</div></td>',
                        '<td>', designation.departmentName ? '<span class="badge bg-info">' + context.escapeHtml(designation.departmentName) + '</span>' : '<span class="text-muted">-</span>', '</td>',
                        '<td>', context.escapeHtml(designation.description || '-'), '</td>',
                        '<td>', designation.isActive ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Active</span>' : '<span class="badge bg-danger"><i class="fas fa-times-circle me-1"></i>Inactive</span>', '</td>',
                        '<td class="text-end"><div class="btn-group" role="group"><a href="', context.replaceToken(editBase, '__id__', designation.id), '" class="btn btn-sm btn-outline-warning" title="Edit"><i class="fas fa-edit"></i></a><button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="', designation.id, '"><i class="fas fa-trash"></i></button></div></td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        getDeleteMessage: function (item) {
            return 'Are you sure you want to delete designation "' + ((item && item.name) || 'this designation') + '"?';
        },
        deleteSuccessMessage: 'Designation deleted successfully.',
        loadErrorMessage: 'Unable to load designations right now.'
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
