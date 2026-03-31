(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('securityGuardsIndexPage');
    if (!page) {
        return;
    }

    var pageSizeSelect = document.getElementById('securityGuardsPageSize');
    var summaryTop = document.getElementById('securityGuardsPaginationSummaryTop');

    function setStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function currentParams() {
        return new URLSearchParams(window.location.search);
    }

    function bindFilters() {
        var params = currentParams();
        document.getElementById('securityGuardsSearch').value = params.get('search') || '';
        document.getElementById('securityGuardsSortBy').value = params.get('sortBy') || '';
        document.getElementById('securityGuardsSortDirection').value = params.get('sortDirection') || 'asc';
        page.querySelector('input[name="pageSize"]').value = params.get('pageSize') || '10';
        if (pageSizeSelect) {
            pageSizeSelect.value = params.get('pageSize') || '10';
        }
    }

    bindFilters();

    if (pageSizeSelect) {
        pageSizeSelect.addEventListener('change', function () {
            var url = new URL(window.location.href);
            url.searchParams.set('pageSize', pageSizeSelect.value);
            url.searchParams.set('pageNumber', '1');
            window.location.href = url.toString();
        });
    }

    window.AppCrud.createListPage({
        pageId: 'securityGuardsIndexPage',
        filterFormId: 'securityGuardsFilterForm',
        tableWrapperId: 'securityGuardsTableWrapper',
        recordCountId: 'securityGuardsRecordCount',
        paginationContainerId: 'securityGuardsPaginationContainer',
        paginationSummaryId: 'securityGuardsPaginationSummary',
        paginationId: 'securityGuardsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'SecurityGuards',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            includeInactive: 'false',
            sortBy: '',
            sortDirection: 'asc'
        },
        buildQuery: function (query) {
            var nextQuery = Object.assign({}, query);
            var supervisorId = page.getAttribute('data-supervisor-id');
            if (supervisorId) {
                nextQuery.supervisorId = supervisorId;
            }
            return nextQuery;
        },
        updateStats: function (data, items) {
            var activeCount = 0;
            var supervisorCount = 0;
            (items || []).forEach(function (item) {
                if (item.isActive) {
                    activeCount += 1;
                }
                if (item.supervisorName) {
                    supervisorCount += 1;
                }
            });

            setStat('total', data.totalCount || items.length);
            setStat('active', activeCount);
            setStat('supervisors', supervisorCount);

            if (summaryTop) {
                if (items.length) {
                    var start = ((data.pageNumber || 1) - 1) * (data.pageSize || items.length) + 1;
                    var end = Math.min((data.pageNumber || 1) * (data.pageSize || items.length), data.totalCount || items.length);
                    summaryTop.textContent = 'Showing ' + start + ' to ' + end + ' of ' + (data.totalCount || items.length) + ' guards';
                } else {
                    summaryTop.textContent = '';
                }
            }
        },
        renderEmptyState: function () {
            var search = currentParams().get('search') || '';
            return [
                '<div class="text-center py-5">',
                '<i class="fas fa-user-shield fa-3x text-muted mb-3"></i>',
                '<h5 class="text-muted">No guards found</h5>',
                search ? '<p class="text-muted">No results found for "' + window.AppCrud.escapeHtml(search) + '"</p>' : '',
                '<a href="' + page.getAttribute('data-create-url') + '" class="btn btn-primary mt-3"><i class="fas fa-user-shield me-2"></i>Add New Guard</a>',
                '</div>'
            ].join('');
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            var editBase = page.getAttribute('data-edit-base');
            var sortBy = context.query.sortBy || '';
            var sortDirection = context.query.sortDirection || 'asc';

            function sortLink(label, field) {
                var nextDirection = sortBy === field && sortDirection === 'asc' ? 'desc' : 'asc';
                var query = Object.assign({}, context.query, { pageNumber: '1', sortBy: field, sortDirection: nextDirection });
                var icon = sortBy === field
                    ? '<i class="fas fa-sort-' + (sortDirection === 'asc' ? 'up' : 'down') + '"></i>'
                    : '<i class="fas fa-sort text-muted"></i>';

                return '<a href="' + context.indexUrl + '?' + context.buildQueryString(query) + '" class="text-decoration-none text-dark">' + label + ' ' + icon + '</a>';
            }

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>' + sortLink('Guard Code', 'code') + '</th>',
                '<th>' + sortLink('Name', 'name') + '</th>',
                '<th>' + sortLink('Email', 'email') + '</th>',
                '<th>Phone</th>',
                '<th>Supervisor</th>',
                '<th>Gender</th>',
                '<th>Status</th>',
                '<th class="text-end" style="min-width: 200px;">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (guard) {
                    var fullName = ((guard.firstName || '') + ' ' + (guard.lastName || '')).trim();
                    var statusBadge = guard.isActive
                        ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Active</span>'
                        : '<span class="badge bg-danger"><i class="fas fa-times-circle me-1"></i>Inactive</span>';

                    return [
                        '<tr>',
                        '<td><span class="badge bg-secondary">' + context.escapeHtml(guard.guardCode) + '</span></td>',
                        '<td><div class="fw-semibold">' + context.escapeHtml(fullName || '-') + '</div></td>',
                        '<td>' + context.escapeHtml(guard.email || '-') + '</td>',
                        '<td>' + context.escapeHtml(guard.phoneNumber || '-') + '</td>',
                        '<td>' + context.escapeHtml(guard.supervisorName || '-') + '</td>',
                        '<td>' + context.escapeHtml(guard.gender || '-') + '</td>',
                        '<td>' + statusBadge + '</td>',
                        '<td class="text-end"><div class="d-flex flex-wrap gap-1 justify-content-end">',
                        '<a href="' + context.replaceToken(detailsBase, '__id__', guard.id) + '" class="btn btn-sm btn-outline-info" title="View Details"><i class="fas fa-eye me-1"></i>View</a>',
                        '<a href="' + context.replaceToken(editBase, '__id__', guard.id) + '" class="btn btn-sm btn-outline-warning" title="Edit"><i class="fas fa-edit me-1"></i>Edit</a>',
                        '<button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="' + guard.id + '"><i class="fas fa-trash me-1"></i>Delete</button>',
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
            return 'Are you sure you want to delete this guard? This cannot be undone.';
        },
        deleteSuccessMessage: 'Security guard deleted successfully.',
        loadErrorMessage: 'Unable to load guards right now.'
    });
})(window, document);
