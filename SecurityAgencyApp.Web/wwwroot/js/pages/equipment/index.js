(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var page = document.getElementById('equipmentIndexPage');
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
        pageId: 'equipmentIndexPage',
        tableWrapperId: 'equipmentTableWrapper',
        recordCountId: 'equipmentRecordCount',
        paginationContainerId: 'equipmentPaginationContainer',
        paginationSummaryId: 'equipmentPaginationSummary',
        paginationId: 'equipmentPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Equipment',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var assigned = 0;
            var maintenance = 0;

            (items || []).forEach(function (item) {
                if (item.assignedToGuardName || item.assignedToSiteName) {
                    assigned += 1;
                }
                if (item.status === 'Maintenance') {
                    maintenance += 1;
                }
            });

            setStat('total', data.totalCount || items.length);
            setStat('assigned', assigned);
            setStat('maintenance', maintenance);
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-tools fa-3x text-muted mb-3"></i><h5 class="text-muted">No equipment found</h5><a href="' +
                page.getAttribute('data-create-url') +
                '" class="btn btn-primary mt-3"><i class="fas fa-tools me-2"></i>Add Equipment</a></div>';
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            return [
                '<div class="table-responsive"><table class="table table-hover align-middle"><thead class="table-light"><tr>',
                '<th>Equipment Code</th><th>Equipment Name</th><th>Category</th><th>Manufacturer</th><th>Assigned To</th><th>Status</th><th class="text-end">Purchase Cost</th><th class="text-end">Actions</th>',
                '</tr></thead><tbody>',
                context.items.map(function (equipment) {
                    var assigned = [];
                    if (equipment.assignedToGuardName) {
                        assigned.push('<span class="badge bg-info">Guard: ' + context.escapeHtml(equipment.assignedToGuardName) + '</span>');
                    }
                    if (equipment.assignedToSiteName) {
                        assigned.push('<span class="badge bg-secondary">Site: ' + context.escapeHtml(equipment.assignedToSiteName) + '</span>');
                    }

                    var statusClass = equipment.status === 'Available'
                        ? 'success'
                        : equipment.status === 'Assigned'
                            ? 'info'
                            : equipment.status === 'Maintenance'
                                ? 'warning'
                                : 'danger';

                    return '<tr>' +
                        '<td><strong>' + context.escapeHtml(equipment.equipmentCode) + '</strong></td>' +
                        '<td>' + context.escapeHtml(equipment.equipmentName) + '</td>' +
                        '<td>' + context.escapeHtml(equipment.category) + '</td>' +
                        '<td>' + context.escapeHtml(equipment.manufacturer || '-') + '</td>' +
                        '<td>' + (assigned.length ? assigned.join(' ') : '<span class="text-muted">Not Assigned</span>') + '</td>' +
                        '<td><span class="badge bg-' + statusClass + '">' + context.escapeHtml(equipment.status || '-') + '</span></td>' +
                        '<td class="text-end">Rs ' + Number(equipment.purchaseCost || 0).toFixed(2) + '</td>' +
                        '<td class="text-end"><a href="' + context.replaceToken(detailsBase, '__id__', equipment.id) + '" class="btn btn-sm btn-outline-primary" title="View"><i class="fas fa-eye"></i></a> <button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="' + equipment.id + '"><i class="fas fa-trash"></i></button></td>' +
                        '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this equipment?';
        },
        deleteSuccessMessage: 'Equipment deleted successfully.',
        loadErrorMessage: 'Unable to load equipment right now.'
    });
})(window, document);
