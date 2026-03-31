(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var page = document.getElementById('announcementsIndexPage');
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
        pageId: 'announcementsIndexPage',
        filterFormId: 'announcementsFilterForm',
        tableWrapperId: 'announcementsTableWrapper',
        recordCountId: 'announcementsRecordCount',
        paginationContainerId: 'announcementsPaginationContainer',
        paginationSummaryId: 'announcementsPaginationSummary',
        paginationId: 'announcementsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Announcements',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            search: '',
            category: '',
            isPinned: '',
            includeInactive: 'false',
            sortDirection: 'desc'
        },
        updateStats: function (data, items) {
            var pinned = 0;
            var active = 0;

            (items || []).forEach(function (item) {
                if (item.isPinned) {
                    pinned += 1;
                }
                if (item.isActive) {
                    active += 1;
                }
            });

            setStat('total', data.totalCount || items.length);
            setStat('pinned', pinned);
            setStat('active', active);
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-bullhorn fa-3x text-muted mb-3"></i><h5 class="text-muted">No announcements</h5><a href="' +
                page.getAttribute('data-create-url') +
                '" class="btn btn-primary mt-3"><i class="fas fa-plus me-2"></i>New Announcement</a></div>';
        },
        renderTable: function (context) {
            var editBase = page.getAttribute('data-edit-base');
            return [
                '<div class="table-responsive"><table class="table table-hover align-middle"><thead class="table-light"><tr>',
                '<th>Title</th><th>Category</th><th>Posted</th><th>Pinned</th><th>Active</th><th class="text-end">Actions</th>',
                '</tr></thead><tbody>',
                context.items.map(function (item) {
                    var postedText = context.formatDate(item.postedAt);
                    if (item.postedByName) {
                        postedText += ' by ' + context.escapeHtml(item.postedByName);
                    }

                    return '<tr>' +
                        '<td><strong>' + context.escapeHtml(item.title) + '</strong></td>' +
                        '<td>' + context.escapeHtml(item.category) + '</td>' +
                        '<td>' + postedText + '</td>' +
                        '<td>' + (item.isPinned ? 'Yes' : 'No') + '</td>' +
                        '<td>' + (item.isActive ? 'Yes' : 'No') + '</td>' +
                        '<td class="text-end">' +
                        '<a href="' + context.replaceToken(editBase, '__id__', item.id) + '" class="btn btn-sm btn-outline-primary">Edit</a> ' +
                        '<button type="button" class="btn btn-sm btn-outline-danger" data-crud-delete-id="' + item.id + '">Delete</button>' +
                        '</td>' +
                        '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Delete this announcement?';
        },
        deleteSuccessMessage: 'Announcement deleted.',
        loadErrorMessage: 'Unable to load announcements right now.'
    });
})(window, document);
