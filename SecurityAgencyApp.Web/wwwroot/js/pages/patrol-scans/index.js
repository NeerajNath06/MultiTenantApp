(function (window, document) {
    'use strict';

    if (!window.AppApi || !window.AppCrud) {
        return;
    }

    var page = document.getElementById('patrolScansIndexPage');
    if (!page) {
        return;
    }

    var filterForm = document.getElementById('patrolScansFilterForm');
    var tableWrapper = document.getElementById('patrolScansTableWrapper');
    var recordCount = document.getElementById('patrolScansRecordCount');
    var paginationContainer = document.getElementById('patrolScansPaginationContainer');
    var paginationSummary = document.getElementById('patrolScansPaginationSummary');
    var pagination = document.getElementById('patrolScansPagination');
    var pageSize = '20';

    function escapeHtml(value) {
        return window.AppCrud.escapeHtml(value);
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

        var currentValue = select.value;
        select.innerHTML = '<option value="">' + placeholder + '</option>' + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + escapeHtml(labelSelector(item)) + '</option>';
        }).join('');

        if (currentValue) {
            select.value = currentValue;
        }
    }

    function getQuery() {
        var params = new URLSearchParams(window.location.search);
        return {
            guardId: params.get('guardId') || '',
            siteId: params.get('siteId') || '',
            dateFrom: params.get('dateFrom') || '',
            dateTo: params.get('dateTo') || '',
            pageNumber: params.get('pageNumber') || '1',
            pageSize: params.get('pageSize') || pageSize
        };
    }

    function buildQueryString(query) {
        var params = new URLSearchParams();
        Object.keys(query || {}).forEach(function (key) {
            if (query[key]) {
                params.set(key, query[key]);
            }
        });
        return params.toString();
    }

    function navigate(query) {
        var qs = buildQueryString(query);
        window.location.href = page.getAttribute('data-index-url') + (qs ? '?' + qs : '');
    }

    function bindFilters() {
        var query = getQuery();
        document.getElementById('patrolScansGuardId').value = query.guardId;
        document.getElementById('patrolScansSiteId').value = query.siteId;
        document.getElementById('patrolScansDateFrom').value = query.dateFrom ? query.dateFrom.slice(0, 10) : '';
        document.getElementById('patrolScansDateTo').value = query.dateTo ? query.dateTo.slice(0, 10) : '';
    }

    function renderEmptyState() {
        tableWrapper.innerHTML = [
            '<div class="text-center py-5">',
            '<i class="fas fa-qrcode fa-3x text-muted mb-3"></i>',
            '<h5 class="text-muted">No patrol scans found</h5>',
            '<p class="text-muted">Select a guard and click View to see scan history.</p>',
            '</div>'
        ].join('');
        recordCount.textContent = '0 total scans';
        setStat('total', 0);
        setStat('visible', 0);
        setStat('sites', 0);
        paginationContainer.classList.add('d-none');
        paginationSummary.textContent = '';
        pagination.innerHTML = '';
    }

    function renderTable(data) {
        var items = data.items || [];
        var siteCount = {};

        items.forEach(function (item) {
            if (item.siteName) {
                siteCount[item.siteName] = true;
            }
        });

        recordCount.textContent = (data.totalCount || items.length) + ' total scans';
        setStat('total', data.totalCount || items.length);
        setStat('visible', items.length);
        setStat('sites', Object.keys(siteCount).length);

        if (!items.length) {
            renderEmptyState();
            return;
        }

        tableWrapper.innerHTML = [
            '<div class="table-responsive">',
            '<table class="table table-hover align-middle">',
            '<thead class="table-light">',
            '<tr>',
            '<th>Scanned At</th>',
            '<th>Location</th>',
            '<th>Checkpoint</th>',
            '<th>Site</th>',
            '<th>Status</th>',
            '</tr>',
            '</thead>',
            '<tbody>',
            items.map(function (scan) {
                return [
                    '<tr>',
                    '<td>' + escapeHtml(new Date(scan.scannedAt).toLocaleString()) + '</td>',
                    '<td>' + escapeHtml(scan.locationName || '-') + '</td>',
                    '<td>' + escapeHtml(scan.checkpointCode || '-') + '</td>',
                    '<td>' + escapeHtml(scan.siteName || '-') + '</td>',
                    '<td><span class="badge bg-success">' + escapeHtml(scan.status || '-') + '</span></td>',
                    '</tr>'
                ].join('');
            }).join(''),
            '</tbody>',
            '</table>',
            '</div>'
        ].join('');

        var totalPages = data.totalPages || 0;
        var query = getQuery();
        if (totalPages <= 1) {
            paginationContainer.classList.add('d-none');
            paginationSummary.textContent = '';
            pagination.innerHTML = '';
            return;
        }

        paginationContainer.classList.remove('d-none');
        paginationSummary.textContent = 'Page ' + (data.pageNumber || 1) + ' of ' + totalPages + ' · ' + (data.totalCount || 0) + ' total';
        var html = [];

        function pageLink(label, targetPage, isActive, iconDirection) {
            var nextQuery = Object.assign({}, query, { pageNumber: String(targetPage) });
            return [
                '<li class="page-item' + (isActive ? ' active' : '') + '">',
                '<a class="page-link" href="' + page.getAttribute('data-index-url') + '?' + buildQueryString(nextQuery) + '">',
                iconDirection ? '<i class="fas fa-chevron-' + iconDirection + '"></i>' : label,
                '</a>',
                '</li>'
            ].join('');
        }

        if ((data.pageNumber || 1) > 1) {
            html.push(pageLink('', (data.pageNumber || 1) - 1, false, 'left'));
        }

        for (var i = Math.max(1, (data.pageNumber || 1) - 2); i <= Math.min(totalPages, (data.pageNumber || 1) + 2); i += 1) {
            html.push(pageLink(String(i), i, i === (data.pageNumber || 1), ''));
        }

        if ((data.pageNumber || 1) < totalPages) {
            html.push(pageLink('', (data.pageNumber || 1) + 1, false, 'right'));
        }

        pagination.innerHTML = html.join('');
    }

    function loadScans() {
        var query = getQuery();
        if (!query.guardId) {
            renderEmptyState();
            return;
        }

        window.AppApi.get('PatrolScans', query)
            .then(renderTable)
            .catch(function () {
                tableWrapper.innerHTML = '<div class="text-center py-5 text-danger">Unable to load patrol scans right now.</div>';
            });
    }

    filterForm.addEventListener('submit', function (event) {
        event.preventDefault();
        navigate({
            guardId: document.getElementById('patrolScansGuardId').value,
            siteId: document.getElementById('patrolScansSiteId').value,
            dateFrom: document.getElementById('patrolScansDateFrom').value,
            dateTo: document.getElementById('patrolScansDateTo').value,
            pageNumber: '1',
            pageSize: pageSize
        });
    });

    Promise.all([
        window.AppApi.get('SecurityGuards', { includeInactive: 'false', pageSize: '1000' }),
        window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' })
    ]).then(function (results) {
        populateSelect(document.getElementById('patrolScansGuardId'), results[0].items || [], '-- Select Guard --', function (item) {
            return item.guardCode || item.guardName || '';
        });
        populateSelect(document.getElementById('patrolScansSiteId'), results[1].items || [], 'All Sites', function (item) {
            return item.siteName || '';
        });
        bindFilters();
        loadScans();
    }).catch(function () {
        tableWrapper.innerHTML = '<div class="text-center py-5 text-danger">Unable to load patrol scan filters right now.</div>';
    });
})(window, document);
