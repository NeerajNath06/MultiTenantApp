(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('sitePostsIndexPage');
    if (!page) {
        return;
    }

    function setStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function populateSelect(select, items, labelSelector) {
        if (!select) {
            return;
        }

        var currentValue = select.value;
        var defaultText = select.id === 'sitePostsFilterSiteId' ? 'All sites' : 'All branches';
        select.innerHTML = '<option value="">' + defaultText + '</option>' + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(labelSelector(item)) + '</option>';
        }).join('');

        if (currentValue) {
            select.value = currentValue;
        } else {
            var url = new URL(window.location.href);
            select.value = url.searchParams.get(select.name) || '';
        }
    }

    Promise.all([
        window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
        window.AppApi.get('Branches', { includeInactive: 'false' })
    ]).then(function (results) {
        populateSelect(document.getElementById('sitePostsFilterSiteId'), results[0].items || [], function (item) { return item.siteName || ''; });
        populateSelect(document.getElementById('sitePostsFilterBranchId'), results[1].items || [], function (item) { return item.branchName || ''; });
    });

    window.AppCrud.createListPage({
        pageId: 'sitePostsIndexPage',
        filterFormId: 'sitePostsFilterForm',
        tableWrapperId: 'sitePostsTableWrapper',
        recordCountId: 'sitePostsRecordCount',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'SitePosts',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            search: '',
            siteId: '',
            branchId: '',
            includeInactive: 'true'
        },
        updateStats: function (data, items) {
            var active = 0;
            var strength = 0;

            (items || []).forEach(function (post) {
                if (post.isActive) {
                    active += 1;
                }
                strength += Number(post.sanctionedStrength || 0);
            });

            setStat('total', data.totalCount || items.length);
            setStat('active', active);
            setStat('strength', strength);
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-users-cog fa-3x text-muted mb-3 opacity-50"></i><h5 class="text-muted">No site posts found</h5><p class="text-muted small mb-0">Create site posts to manage sanctioned positions and staffing rules.</p><a href="' +
                page.getAttribute('data-create-url') +
                '" class="btn btn-primary mt-3"><i class="fas fa-plus me-2"></i>Add Site Post</a></div>';
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            var editBase = page.getAttribute('data-edit-base');
            return [
                '<div class="table-responsive"><table class="table table-hover align-middle mb-0"><thead class="table-light"><tr>',
                '<th>Post</th><th>Site / Branch</th><th>Shift</th><th>Strength</th><th>Requirements</th><th>Status</th><th class="text-end">Actions</th>',
                '</tr></thead><tbody>',
                context.items.map(function (post) {
                    return '<tr>' +
                        '<td><div class="fw-semibold">' + context.escapeHtml(post.postName) + '</div><div class="small text-muted">' + context.escapeHtml(post.postCode) + '</div></td>' +
                        '<td><div>' + context.escapeHtml(post.siteName) + '</div><div class="small text-muted">' + context.escapeHtml(post.branchName || 'No branch') + '</div></td>' +
                        '<td>' + context.escapeHtml(post.shiftName || '-') + '</td>' +
                        '<td><span class="badge bg-info">' + context.escapeHtml(post.sanctionedStrength || 0) + '</span></td>' +
                        '<td><div class="small">' + context.escapeHtml(post.genderRequirement || 'No gender rule') + '</div><div class="small text-muted">' + context.escapeHtml(post.skillRequirement || 'No skill rule') + '</div><div class="small text-muted">Weapon: ' + (post.requiresWeapon ? 'Yes' : 'No') + ' | Reliever: ' + (post.relieverRequired ? 'Yes' : 'No') + '</div></td>' +
                        '<td><span class="badge bg-' + (post.isActive ? 'success' : 'secondary') + '">' + (post.isActive ? 'Active' : 'Inactive') + '</span></td>' +
                        '<td class="text-end"><div class="action-btns"><a href="' + context.replaceToken(detailsBase, '__id__', post.id) + '" class="btn btn-sm btn-outline-primary" title="View"><i class="fas fa-eye"></i></a><a href="' + context.replaceToken(editBase, '__id__', post.id) + '" class="btn btn-sm btn-outline-secondary" title="Edit"><i class="fas fa-edit"></i></a><button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="' + post.id + '"><i class="fas fa-trash"></i></button></div></td>' +
                        '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this site post?';
        },
        deleteSuccessMessage: 'Site post deleted successfully.',
        loadErrorMessage: 'Unable to load site posts right now.'
    });
})(window, document);
