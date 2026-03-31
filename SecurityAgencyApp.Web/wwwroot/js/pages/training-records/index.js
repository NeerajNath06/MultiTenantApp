(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var page = document.getElementById('trainingRecordsIndexPage');
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
        pageId: 'trainingRecordsIndexPage',
        tableWrapperId: 'trainingRecordsTableWrapper',
        recordCountId: 'trainingRecordsRecordCount',
        paginationContainerId: 'trainingRecordsPaginationContainer',
        paginationSummaryId: 'trainingRecordsPaginationSummary',
        paginationId: 'trainingRecordsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'TrainingRecords',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var completed = 0;
            var expired = 0;

            (items || []).forEach(function (item) {
                if (item.status === 'Completed') {
                    completed += 1;
                }
                if (item.status === 'Expired') {
                    expired += 1;
                }
            });

            setStat('total', data.totalCount || items.length);
            setStat('completed', completed);
            setStat('expired', expired);
        },
        renderEmptyState: function () {
            return [
                '<div class="text-center py-5">',
                '<i class="fas fa-graduation-cap fa-3x text-muted mb-3"></i>',
                '<h5 class="text-muted">No training records found</h5>',
                '<a href="', page.getAttribute('data-create-url'), '" class="btn btn-primary mt-3">',
                '<i class="fas fa-graduation-cap me-2"></i>Add Training Record',
                '</a>',
                '</div>'
            ].join('');
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');

            function statusBadge(status) {
                var className = status === 'Completed'
                    ? 'success'
                    : status === 'Expired'
                        ? 'danger'
                        : 'warning';
                return '<span class="badge bg-' + className + '">' + context.escapeHtml(status || '-') + '</span>';
            }

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>Guard</th>',
                '<th>Training Type</th>',
                '<th>Training Name</th>',
                '<th>Training Date</th>',
                '<th>Expiry Date</th>',
                '<th>Score</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (training) {
                    return [
                        '<tr>',
                        '<td><strong>', context.escapeHtml(training.guardCode), '</strong><br><small class="text-muted">', context.escapeHtml(training.guardName), '</small></td>',
                        '<td>', context.escapeHtml(training.trainingType), '</td>',
                        '<td>', context.escapeHtml(training.trainingName), '</td>',
                        '<td>', context.formatDate(training.trainingDate), '</td>',
                        '<td>', training.expiryDate ? context.formatDate(training.expiryDate) : '-', '</td>',
                        '<td>', training.score == null ? '-' : context.escapeHtml(Number(training.score).toFixed(2)), '</td>',
                        '<td>', statusBadge(training.status), '</td>',
                        '<td class="text-end">',
                        '<a href="', context.replaceToken(detailsBase, '__id__', training.id), '" class="btn btn-sm btn-outline-primary" title="View"><i class="fas fa-eye"></i></a> ',
                        '<button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="', training.id, '"><i class="fas fa-trash"></i></button>',
                        '</td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this training record?';
        },
        deleteSuccessMessage: 'Training record deleted successfully.',
        loadErrorMessage: 'Unable to load training records right now.'
    });
})(window, document);
