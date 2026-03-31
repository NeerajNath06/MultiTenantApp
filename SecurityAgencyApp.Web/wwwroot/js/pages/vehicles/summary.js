(function (window, document) {
    'use strict';

    if (!window.AppApi || !window.AppCrud) {
        return;
    }

    var page = document.getElementById('vehicleSummaryPage');
    var form = document.getElementById('vehicleSummaryFilterForm');
    var tableWrapper = document.getElementById('vehicleSummaryTableWrapper');
    if (!page || !form || !tableWrapper) {
        return;
    }

    function populateSites(items) {
        var select = document.getElementById('vehicleSummarySiteId');
        if (!select) {
            return;
        }

        var url = new URL(window.location.href);
        var currentValue = url.searchParams.get('siteId') || '';
        select.innerHTML = '<option value="">All Sites</option>' + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(item.siteName || '') + '</option>';
        }).join('');
        select.value = currentValue;
    }

    function currentQuery() {
        var url = new URL(window.location.href);
        return {
            siteId: url.searchParams.get('siteId') || '',
            dateFrom: url.searchParams.get('dateFrom') || '',
            dateTo: url.searchParams.get('dateTo') || ''
        };
    }

    function load() {
        var query = currentQuery();
        Object.keys(query).forEach(function (key) {
            var field = form.elements.namedItem(key);
            if (field) {
                field.value = query[key];
            }
        });

        return window.AppApi.get(page.getAttribute('data-summary-endpoint'), query).then(function (data) {
            var items = data.sites || [];
            if (!items.length) {
                tableWrapper.innerHTML = '<div class="text-center py-5"><i class="fas fa-chart-bar fa-3x text-muted mb-3"></i><h5 class="text-muted">No summary data for the selected filters</h5><a href="' +
                    page.getAttribute('data-index-url') +
                    '" class="btn btn-primary mt-3"><i class="fas fa-car me-2"></i>Vehicle Log</a></div>';
                return;
            }

            tableWrapper.innerHTML = [
                '<div class="table-responsive"><table class="table table-hover align-middle"><thead class="table-light"><tr>',
                '<th>Site</th><th class="text-end">Total Entries</th><th class="text-end">Inside</th><th class="text-end">Exited</th><th class="text-end">Actions</th>',
                '</tr></thead><tbody>',
                items.map(function (item) {
                    var queryString = new URLSearchParams({
                        siteId: item.siteId,
                        dateFrom: currentQuery().dateFrom,
                        dateTo: currentQuery().dateTo
                    }).toString();

                    return '<tr>' +
                        '<td><strong>' + window.AppCrud.escapeHtml(item.siteName) + '</strong>' + (item.siteAddress ? '<br><small class="text-muted">' + window.AppCrud.escapeHtml(item.siteAddress) + '</small>' : '') + '</td>' +
                        '<td class="text-end">' + window.AppCrud.escapeHtml(item.totalEntries || 0) + '</td>' +
                        '<td class="text-end"><span class="badge bg-info">' + window.AppCrud.escapeHtml(item.insideCount || 0) + '</span></td>' +
                        '<td class="text-end"><span class="badge bg-secondary">' + window.AppCrud.escapeHtml(item.exitedCount || 0) + '</span></td>' +
                        '<td class="text-end"><a href="' + page.getAttribute('data-index-url') + '?' + queryString + '" class="btn btn-sm btn-outline-primary">View Logs</a></td>' +
                        '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        }).catch(function () {
            tableWrapper.innerHTML = '<div class="text-center py-5 text-danger">Unable to load summary right now.</div>';
        });
    }

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        var params = new URLSearchParams();
        Array.from(new FormData(form).entries()).forEach(function (entry) {
            if (entry[1]) {
                params.set(entry[0], entry[1]);
            }
        });
        window.location.href = window.location.pathname + (params.toString() ? '?' + params.toString() : '');
    });

    window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }).then(function (data) {
        populateSites(data.items || []);
    });

    load();
})(window, document);
