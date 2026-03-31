(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var page = document.getElementById('branchesIndexPage');
    if (!page) {
        return;
    }

    function setStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    window.AppCrud.createListPage({
        pageId: 'branchesIndexPage',
        filterFormId: 'branchesFilterForm',
        tableWrapperId: 'branchesTableWrapper',
        recordCountId: 'branchesRecordCount',
        paginationContainerId: 'branchesPaginationContainer',
        paginationSummaryId: 'branchesPaginationSummary',
        paginationId: 'branchesPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Branches',
        queryDefaults: {
            search: '',
            includeInactive: 'true'
        },
        recordCountText: function (data) {
            var total = data.totalCount || ((data.items || []).length);
            return total + ' total records';
        },
        updateStats: function (data, items) {
            var activeCount = 0;
            var headOfficeCount = 0;

            (items || []).forEach(function (branch) {
                if (branch.isActive) {
                    activeCount += 1;
                }
                if (branch.isHeadOffice) {
                    headOfficeCount += 1;
                }
            });

            setStat('total', data.totalCount || items.length);
            setStat('active', activeCount);
            setStat('headOffice', headOfficeCount);
        },
        renderEmptyState: function () {
            return [
                '<div class="text-center py-5">',
                '<i class="fas fa-code-branch fa-3x text-muted mb-3 opacity-50"></i>',
                '<h5 class="text-muted">No branches found</h5>',
                '<p class="text-muted small mb-0">Create your first branch to assign it to sites.</p>',
                '<a href="', page.getAttribute('data-create-url'), '" class="btn btn-primary mt-3">',
                '<i class="fas fa-plus me-2"></i>Add Branch',
                '</a>',
                '</div>'
            ].join('');
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            var editBase = page.getAttribute('data-edit-base');
            var sortedItems = (context.items || []).slice().sort(function (left, right) {
                return String(left.branchName || '').localeCompare(String(right.branchName || ''));
            });

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle mb-0">',
                '<thead class="table-light">',
                '<tr>',
                '<th>Branch</th>',
                '<th>Location</th>',
                '<th>Contact</th>',
                '<th>Compliance</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                sortedItems.map(function (branch) {
                    var badges = [];
                    if (branch.isHeadOffice) {
                        badges.push('<span class="badge bg-info me-1">Head Office</span>');
                    }
                    badges.push('<span class="badge bg-' + (branch.isActive ? 'success' : 'secondary') + '">' + (branch.isActive ? 'Active' : 'Inactive') + '</span>');

                    return [
                        '<tr>',
                        '<td><div class="fw-semibold">', context.escapeHtml(branch.branchName), '</div><div class="small text-muted">', context.escapeHtml(branch.branchCode), '</div></td>',
                        '<td><div>', context.escapeHtml((branch.city || '-') + ', ' + (branch.state || '-')), '</div><div class="small text-muted">', context.escapeHtml(branch.pinCode || '-'), '</div></td>',
                        '<td><div>', context.escapeHtml(branch.contactPerson || '-'), '</div><div class="small text-muted">', context.escapeHtml(branch.contactPhone || '-'), '</div></td>',
                        '<td><div class="small">GST: ', context.escapeHtml(branch.gstNumber || '-'), '</div><div class="small text-muted">License: ', context.escapeHtml(branch.labourLicenseNumber || '-'), '</div></td>',
                        '<td>', badges.join(''), '</td>',
                        '<td class="text-end"><div class="action-btns">',
                        '<a href="', context.replaceToken(detailsBase, '__id__', branch.id), '" class="btn btn-sm btn-outline-primary" title="View"><i class="fas fa-eye"></i></a>',
                        '<a href="', context.replaceToken(editBase, '__id__', branch.id), '" class="btn btn-sm btn-outline-secondary" title="Edit"><i class="fas fa-edit"></i></a>',
                        '<button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="', branch.id, '"><i class="fas fa-trash"></i></button>',
                        '</div></td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        getDeleteMessage: function (branch) {
            return 'Are you sure you want to delete "' + ((branch && branch.branchName) || 'this branch') + '"?';
        },
        deleteSuccessMessage: 'Branch deleted successfully.',
        loadErrorMessage: 'Unable to load branches right now.'
    });
})(window, document);
