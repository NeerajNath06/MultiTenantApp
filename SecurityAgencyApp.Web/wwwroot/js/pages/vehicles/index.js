(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('vehiclesIndexPage');
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
        populateSelect(document.getElementById('vehiclesFilterSiteId'), results[0].items || [], 'All Sites', function (item) { return item.siteName || ''; });
        populateSelect(document.getElementById('vehiclesFilterGuardId'), results[1].items || [], 'All Guards', function (item) { return item.guardCode || ''; });
    });

    var listPage = window.AppCrud.createListPage({
        pageId: 'vehiclesIndexPage',
        filterFormId: 'vehiclesFilterForm',
        tableWrapperId: 'vehiclesTableWrapper',
        recordCountId: 'vehiclesRecordCount',
        paginationContainerId: 'vehiclesPaginationContainer',
        paginationSummaryId: 'vehiclesPaginationSummary',
        paginationId: 'vehiclesPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Vehicles',
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
            var exited = 0;

            (items || []).forEach(function (item) {
                if (item.exitTime) {
                    exited += 1;
                } else {
                    inside += 1;
                }
            });

            setStat('total', data.totalCount || items.length);
            setStat('inside', inside);
            setStat('exited', exited);
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-car fa-3x text-muted mb-3"></i><h5 class="text-muted">No vehicle entries found</h5><a href="' +
                page.getAttribute('data-create-url') +
                '" class="btn btn-primary mt-3"><i class="fas fa-car me-2"></i>Register Entry</a></div>';
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            return [
                '<div class="table-responsive"><table class="table table-hover align-middle"><thead class="table-light"><tr>',
                '<th>Vehicle</th><th>Driver</th><th>Purpose</th><th>Site</th><th>Guard</th><th>Entry</th><th>Exit</th><th class="text-end">Actions</th>',
                '</tr></thead><tbody>',
                context.items.map(function (item) {
                    var buttons = [
                        '<a href="' + context.replaceToken(detailsBase, '__id__', item.id) + '" class="btn btn-outline-primary" title="View">View</a>'
                    ];
                    if (!item.exitTime) {
                        buttons.push('<button type="button" class="btn btn-outline-success" data-vehicle-exit-id="' + item.id + '">Exit</button>');
                    }
                    buttons.push('<button type="button" class="btn btn-outline-danger" data-crud-delete-id="' + item.id + '"><i class="fas fa-trash"></i></button>');

                    return '<tr>' +
                        '<td><strong>' + context.escapeHtml(item.vehicleNumber) + '</strong><br><small class="text-muted">' + context.escapeHtml(item.vehicleType) + '</small></td>' +
                        '<td>' + context.escapeHtml(item.driverName) + (item.driverPhone ? '<br><small class="text-muted">' + context.escapeHtml(item.driverPhone) + '</small>' : '') + '</td>' +
                        '<td>' + context.escapeHtml(item.purpose) + '</td>' +
                        '<td>' + context.escapeHtml(item.siteName) + '</td>' +
                        '<td>' + context.escapeHtml(item.guardName) + '</td>' +
                        '<td>' + context.formatDate(item.entryTime) + '</td>' +
                        '<td>' + (item.exitTime ? context.formatDate(item.exitTime) : '-') + '</td>' +
                        '<td class="text-end"><div class="btn-group btn-group-sm">' + buttons.join('') + '</div></td>' +
                        '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this vehicle log entry?';
        },
        deleteSuccessMessage: 'Vehicle log deleted successfully.',
        loadErrorMessage: 'Unable to load vehicle entries right now.'
    });

    page.addEventListener('click', function (event) {
        var exitButton = event.target.closest('[data-vehicle-exit-id]');
        if (!exitButton) {
            return;
        }

        var id = exitButton.getAttribute('data-vehicle-exit-id');
        var confirmPromise = window.AppUi && typeof window.AppUi.confirm === 'function'
            ? window.AppUi.confirm({ message: 'Record exit for this vehicle?', confirmText: 'Record Exit' })
            : Promise.resolve(window.confirm('Record exit for this vehicle?'));

        confirmPromise.then(function (confirmed) {
            if (!confirmed) {
                return;
            }

            return window.AppApi.patch('Vehicles/' + id + '/exit', { exitTime: null }).then(function () {
                if (window.AppUi) {
                    window.AppUi.showToast({
                        type: 'success',
                        title: 'Exit Recorded',
                        message: 'Vehicle exit recorded.'
                    });
                }

                if (listPage && typeof listPage.reload === 'function') {
                    listPage.reload();
                }
            });
        });
    });
})(window, document);
