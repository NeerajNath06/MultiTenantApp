(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('visitorsIndexPage');
    if (!page) {
        return;
    }

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
        window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
        window.AppApi.get('SecurityGuards', { includeInactive: 'false', pageSize: '1000' })
    ]).then(function (results) {
        populateSelect(document.getElementById('visitorsFilterSiteId'), results[0].items || [], 'All Sites', function (item) {
            return item.siteName || '';
        });
        populateSelect(document.getElementById('visitorsFilterGuardId'), results[1].items || [], 'All Guards', function (item) {
            return item.guardCode || '';
        });
    });

    var listPage = window.AppCrud.createListPage({
        pageId: 'visitorsIndexPage',
        filterFormId: 'visitorsFilterForm',
        tableWrapperId: 'visitorsTableWrapper',
        recordCountId: 'visitorsRecordCount',
        paginationContainerId: 'visitorsPaginationContainer',
        paginationSummaryId: 'visitorsPaginationSummary',
        paginationId: 'visitorsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Visitors',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            siteId: '',
            guardId: '',
            dateFrom: '',
            dateTo: '',
            insideOnly: '',
            search: ''
        },
        updateStats: function (data, items) {
            var inside = 0;
            var company = 0;

            (items || []).forEach(function (item) {
                if (!item.exitTime) {
                    inside += 1;
                }
                if (item.companyName) {
                    company += 1;
                }
            });

            setStat('total', data.totalCount || items.length);
            setStat('inside', inside);
            setStat('company', company);
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-users fa-3x text-muted mb-3"></i><h5 class="text-muted">No visitors found</h5><a href="' +
                page.getAttribute('data-create-url') +
                '" class="btn btn-primary mt-3"><i class="fas fa-user-plus me-2"></i>Register Visitor</a></div>';
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            var editBase = page.getAttribute('data-edit-base');

            return [
                '<div class="table-responsive"><table class="table table-hover align-middle"><thead class="table-light"><tr>',
                '<th>Visitor</th><th>Type</th><th>Purpose</th><th>Site</th><th>Guard</th><th>Entry</th><th>Exit</th><th>Badge</th><th class="text-end">Actions</th>',
                '</tr></thead><tbody>',
                context.items.map(function (item) {
                    var buttons = [
                        '<a href="' + context.replaceToken(detailsBase, '__id__', item.id) + '" class="btn btn-outline-primary" title="View"><i class="fas fa-eye"></i></a>',
                        '<a href="' + context.replaceToken(editBase, '__id__', item.id) + '" class="btn btn-outline-secondary" title="Edit"><i class="fas fa-edit"></i></a>'
                    ];

                    if (!item.exitTime) {
                        buttons.push('<button type="button" class="btn btn-outline-success" title="Mark Exit" data-visitor-exit-id="' + item.id + '">Exit</button>');
                    }

                    buttons.push('<button type="button" class="btn btn-outline-danger" title="Delete" data-crud-delete-id="' + item.id + '"><i class="fas fa-trash"></i></button>');

                    return '<tr>' +
                        '<td><strong>' + context.escapeHtml(item.visitorName) + '</strong>' + (item.companyName ? '<br><small class="text-muted">' + context.escapeHtml(item.companyName) + '</small>' : '') + '</td>' +
                        '<td>' + context.escapeHtml(item.visitorType) + '</td>' +
                        '<td>' + context.escapeHtml(item.purpose) + '</td>' +
                        '<td>' + context.escapeHtml(item.siteName) + '</td>' +
                        '<td>' + context.escapeHtml(item.guardName) + '</td>' +
                        '<td>' + context.formatDate(item.entryTime) + '</td>' +
                        '<td>' + (item.exitTime ? context.formatDate(item.exitTime) : '-') + '</td>' +
                        '<td>' + context.escapeHtml(item.badgeNumber || '-') + '</td>' +
                        '<td class="text-end"><div class="btn-group btn-group-sm">' + buttons.join('') + '</div></td>' +
                        '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this visitor record?';
        },
        deleteSuccessMessage: 'Visitor deleted successfully.',
        loadErrorMessage: 'Unable to load visitors right now.'
    });

    page.addEventListener('click', function (event) {
        var exitButton = event.target.closest('[data-visitor-exit-id]');
        if (!exitButton) {
            return;
        }

        var id = exitButton.getAttribute('data-visitor-exit-id');
        var confirmPromise = window.AppUi && typeof window.AppUi.confirm === 'function'
            ? window.AppUi.confirm({ message: 'Record exit for this visitor?', confirmText: 'Record Exit' })
            : Promise.resolve(window.confirm('Record exit for this visitor?'));

        confirmPromise.then(function (confirmed) {
            if (!confirmed) {
                return;
            }

            return window.AppApi.patch('Visitors/' + id + '/exit', { exitTime: null }).then(function () {
                if (window.AppUi) {
                    window.AppUi.showToast({
                        type: 'success',
                        title: 'Exit Recorded',
                        message: 'Visitor exit recorded.'
                    });
                }

                if (listPage && typeof listPage.reload === 'function') {
                    listPage.reload();
                }
            });
        });
    });
})(window, document);
