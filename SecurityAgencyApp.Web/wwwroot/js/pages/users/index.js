(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('usersIndexPage');
    if (!page) {
        return;
    }

    var summaryTop = document.getElementById('usersPaginationSummaryTop');
    var pageSizeSelect = document.getElementById('usersPageSize');

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
        document.getElementById('usersSearch').value = params.get('search') || '';
        if (pageSizeSelect) {
            pageSizeSelect.value = params.get('pageSize') || '10';
        }
        var pageSizeInput = page.querySelector('input[name="pageSize"]');
        if (pageSizeInput) {
            pageSizeInput.value = params.get('pageSize') || '10';
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
        pageId: 'usersIndexPage',
        filterFormId: 'usersFilterForm',
        tableWrapperId: 'usersTableWrapper',
        recordCountId: 'usersRecordCount',
        paginationContainerId: 'usersPaginationContainer',
        paginationSummaryId: 'usersPaginationSummary',
        paginationId: 'usersPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Users',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10'
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

            setStat('total', data.totalCount || items.length);
            setStat('active', activeCount);
            setStat('departments', linkedDepartments);

            if (summaryTop) {
                if (items.length) {
                    var start = ((data.pageNumber || 1) - 1) * (data.pageSize || items.length) + 1;
                    var end = Math.min((data.pageNumber || 1) * (data.pageSize || items.length), data.totalCount || items.length);
                    summaryTop.textContent = 'Showing ' + start + ' to ' + end + ' of ' + (data.totalCount || items.length) + ' users';
                } else {
                    summaryTop.textContent = '';
                }
            }
        },
        renderEmptyState: function () {
            var search = currentParams().get('search') || '';
            return [
                '<div class="text-center py-5">',
                '<i class="fas fa-users fa-3x text-muted mb-3"></i>',
                '<h5 class="text-muted">No users found</h5>',
                search ? '<p class="text-muted">No results found for "' + window.AppCrud.escapeHtml(search) + '"</p>' : '<p class="text-muted">Get started by creating your first user.</p>',
                '<a href="' + page.getAttribute('data-create-url') + '" class="btn btn-primary mt-3">',
                '<i class="fas fa-user-plus me-2"></i>Add New User',
                '</a>',
                '</div>'
            ].join('');
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            var editBase = page.getAttribute('data-edit-base');

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>Name</th>',
                '<th>Email</th>',
                '<th>Phone</th>',
                '<th>Department</th>',
                '<th>Status</th>',
                '<th class="text-end" style="min-width: 200px;">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (user) {
                    var initial = (user.firstName || 'U').charAt(0).toUpperCase();
                    var statusBadge = user.isActive
                        ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Active</span>'
                        : '<span class="badge bg-danger"><i class="fas fa-times-circle me-1"></i>Inactive</span>';

                    return [
                        '<tr>',
                        '<td>',
                        '<div class="d-flex align-items-center">',
                        '<div class="bg-primary bg-opacity-10 rounded-circle d-flex align-items-center justify-content-center me-2" style="width: 40px; height: 40px;">',
                        '<span class="text-primary fw-semibold">' + context.escapeHtml(initial) + '</span>',
                        '</div>',
                        '<div>',
                        '<div class="fw-semibold">' + context.escapeHtml(((user.firstName || '') + ' ' + (user.lastName || '')).trim() || '-') + '</div>',
                        '<small class="text-muted">' + context.escapeHtml(user.userName || '-') + '</small>',
                        '</div>',
                        '</div>',
                        '</td>',
                        '<td>' + context.escapeHtml(user.email || '-') + '</td>',
                        '<td>' + context.escapeHtml(user.phoneNumber || '-') + '</td>',
                        '<td>' + (user.departmentName ? '<span class="badge bg-info">' + context.escapeHtml(user.departmentName) + '</span>' : '<span class="text-muted">-</span>') + '</td>',
                        '<td>' + statusBadge + '</td>',
                        '<td class="text-end"><div class="d-flex flex-wrap gap-1 justify-content-end">',
                        '<a href="' + context.replaceToken(detailsBase, '__id__', user.id) + '" class="btn btn-sm btn-outline-info" title="View Details"><i class="fas fa-eye me-1"></i>View</a>',
                        '<a href="' + context.replaceToken(editBase, '__id__', user.id) + '" class="btn btn-sm btn-outline-warning" title="Edit"><i class="fas fa-edit me-1"></i>Edit</a>',
                        '<button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="' + user.id + '"><i class="fas fa-trash me-1"></i>Delete</button>',
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
            return 'Are you sure you want to delete this user? This cannot be undone.';
        },
        deleteSuccessMessage: 'User deleted successfully.',
        loadErrorMessage: 'Unable to load users right now.'
    });
})(window, document);
