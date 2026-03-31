(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('sitesIndexPage');
    if (!page) {
        return;
    }

    function reportEndpoint(reportType) {
        switch ((reportType || '').toLowerCase()) {
            case 'bill':
                return 'Report/generate-bill';
            case 'attendance':
                return 'Report/generate-attendance';
            case 'wages':
                return 'Report/generate-wages';
            case 'full':
                return 'Report/generate-full-report';
            default:
                return null;
        }
    }

    function setStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function bindFilters() {
        var params = new URLSearchParams(window.location.search);
        document.getElementById('sitesSearch').value = params.get('search') || '';
        document.getElementById('sitesSortBy').value = params.get('sortBy') || '';
        document.getElementById('sitesSortDirection').value = params.get('sortDirection') || 'asc';
        document.getElementById('sitesPageSize').value = params.get('pageSize') || '10';
    }

    function downloadReport(siteId, year, month, format, reportType, button) {
        var endpoint = reportEndpoint(reportType);
        var msgEl = document.getElementById('reportMessage');
        var trackingEl = document.getElementById('reportTracking');
        var trackBillIdEl = document.getElementById('trackBillId');
        var trackWageIdEl = document.getElementById('trackWageId');
        if (!endpoint) {
            return Promise.reject(new Error('Invalid report type.'));
        }

        msgEl.classList.add('d-none');
        trackingEl.classList.add('d-none');
        button.disabled = true;

        var trackingPromise = reportType === 'full'
            ? window.AppApi.post('MonthlyDocuments/generate-all', { siteId: siteId, year: Number(year), month: Number(month) }).then(function (data) {
                trackBillIdEl.textContent = data.billId || '-';
                trackWageIdEl.textContent = data.wageId || '-';
                trackingEl.classList.remove('d-none');
            })
            : Promise.resolve();

        return trackingPromise.then(function () {
            return window.AppApi.client.get(endpoint, {
                params: {
                    siteId: siteId,
                    year: year,
                    month: month,
                    format: format
                },
                responseType: 'blob'
            });
        }).then(function (response) {
            var cd = response.headers['content-disposition'] || '';
            var match = cd.match(/filename\*?=(?:UTF-8'')?"?([^";\n]+)"?/i) || cd.match(/filename="?([^";\n]+)"?/i);
            var fileName = match && match[1] ? match[1].trim().replace(/^["']|["']$/g, '') : 'report';
            var href = window.URL.createObjectURL(response.data);
            var link = document.createElement('a');
            link.href = href;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(href);
            msgEl.classList.remove('d-none', 'alert-danger');
            msgEl.classList.add('alert-success');
            msgEl.textContent = 'Report downloaded successfully.';
        }).catch(function (error) {
            msgEl.classList.remove('d-none', 'alert-success');
            msgEl.classList.add('alert-danger');
            msgEl.textContent = error && error.message ? error.message : 'Download failed.';
        }).finally(function () {
            button.disabled = false;
        });
    }

    bindFilters();

    document.getElementById('sitesPageSize').addEventListener('change', function () {
        var url = new URL(window.location.href);
        url.searchParams.set('pageSize', this.value);
        url.searchParams.set('pageNumber', '1');
        window.location.href = url.toString();
    });

    window.AppCrud.createListPage({
        pageId: 'sitesIndexPage',
        filterFormId: 'sitesFilterForm',
        tableWrapperId: 'sitesTableWrapper',
        recordCountId: 'sitesRecordCount',
        paginationContainerId: 'sitesPaginationContainer',
        paginationSummaryId: 'sitesPaginationSummary',
        paginationId: 'sitesPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Sites',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            includeInactive: 'false',
            sortBy: '',
            sortDirection: 'asc'
        },
        buildQuery: function (query) {
            var result = Object.assign({}, query);
            var supervisorId = page.getAttribute('data-supervisor-id');
            if (supervisorId) {
                result.supervisorId = supervisorId;
            }
            return result;
        },
        updateStats: function (data, items) {
            setStat('total', data.totalCount || items.length);
            setStat('active', (items || []).filter(function (item) { return !!item.isActive; }).length);
            setStat('plans', (items || []).filter(function (item) { return !!item.hasActiveDeploymentPlan; }).length);
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-building fa-3x text-muted mb-3"></i><h5 class="text-muted">No sites found</h5><a href="' + page.getAttribute('data-create-url') + '" class="btn btn-primary mt-3"><i class="fas fa-building me-2"></i>Add New Site</a></div>';
        },
        renderTable: function (context) {
            var sortBy = context.query.sortBy || '';
            var sortDirection = context.query.sortDirection || 'asc';
            function sortLink(field, label) {
                var icon = '<i class="fas fa-sort text-muted ms-1"></i>';
                var direction = 'asc';
                if (sortBy === field) {
                    direction = sortDirection === 'asc' ? 'desc' : 'asc';
                    icon = '<i class="fas fa-sort-' + (sortDirection === 'asc' ? 'up' : 'down') + ' ms-1"></i>';
                }
                return '<button type="button" class="btn btn-link btn-sm p-0 text-decoration-none text-dark" data-sort-field="' + field + '" data-sort-direction="' + direction + '">' + label + icon + '</button>';
            }

            return [
                '<div class="table-responsive"><table class="table table-hover align-middle"><thead class="table-light"><tr>',
                '<th>', sortLink('code', 'Site Code'), '</th>',
                '<th>', sortLink('name', 'Site Name'), '</th>',
                '<th>', sortLink('client', 'Client'), '</th>',
                '<th>', sortLink('city', 'Location'), '</th>',
                '<th>Branch</th><th>Contact</th><th>Planning</th><th>Guards</th><th>Status</th><th class="text-end">Actions</th>',
                '</tr></thead><tbody>',
                context.items.map(function (site) {
                    return '<tr>'
                        + '<td><span class="badge bg-secondary">' + context.escapeHtml(site.siteCode) + '</span></td>'
                        + '<td><div class="fw-semibold">' + context.escapeHtml(site.siteName) + '</div></td>'
                        + '<td>' + context.escapeHtml(site.clientName) + '</td>'
                        + '<td><small>' + context.escapeHtml((site.city || '-') + ', ' + (site.state || '-')) + '</small><br /><small class="text-muted">' + context.escapeHtml(site.pinCode || '-') + '</small></td>'
                        + '<td><div>' + context.escapeHtml(site.branchName || '-') + '</div><small class="text-muted">' + (site.branchId ? 'Mapped' : 'Unassigned') + '</small></td>'
                        + '<td><small>' + context.escapeHtml(site.contactPerson || '-') + '</small><br /><small class="text-muted">' + context.escapeHtml(site.contactPhone || '-') + '</small>' + (site.emergencyContactName ? '<br /><small class="text-muted">Emergency: ' + context.escapeHtml(site.emergencyContactName) + '</small>' : '') + '</td>'
                        + '<td><span class="badge bg-secondary me-1">' + context.escapeHtml(site.postsCount) + ' post(s)</span>' + (site.hasActiveDeploymentPlan ? '<span class="badge bg-primary">Plan Active</span>' : '<span class="badge bg-light text-dark border">No Plan</span>') + (site.musterPoint ? '<div class="small text-muted mt-1">Muster: ' + context.escapeHtml(site.musterPoint) + '</div>' : '') + '</td>'
                        + '<td><span class="badge bg-info"><i class="fas fa-user-shield me-1"></i>' + context.escapeHtml(site.guardCount) + '</span></td>'
                        + '<td>' + (site.isActive ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Active</span>' : '<span class="badge bg-danger"><i class="fas fa-times-circle me-1"></i>Inactive</span>') + '</td>'
                        + '<td class="text-end"><div class="btn-group">'
                        + '<a href="' + context.replaceToken(page.getAttribute('data-details-base'), '__id__', site.id) + '" class="btn btn-sm btn-outline-primary" title="View"><i class="fas fa-eye"></i></a>'
                        + '<button type="button" class="btn btn-sm btn-outline-success" title="Generate Report" data-bs-toggle="modal" data-bs-target="#generateReportModal" data-site-id="' + site.id + '"><i class="fas fa-file-alt"></i></button>'
                        + '<a href="' + context.replaceToken(page.getAttribute('data-edit-base'), '__id__', site.id) + '" class="btn btn-sm btn-outline-warning" title="Edit"><i class="fas fa-edit"></i></a>'
                        + '<button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="' + site.id + '"><i class="fas fa-trash"></i></button>'
                        + '</div></td>'
                        + '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this site?';
        },
        deleteSuccessMessage: 'Site deleted successfully.',
        loadErrorMessage: 'Unable to load sites right now.'
    });

    page.addEventListener('click', function (event) {
        var sortButton = event.target.closest('[data-sort-field]');
        if (sortButton) {
            var url = new URL(window.location.href);
            url.searchParams.set('sortBy', sortButton.getAttribute('data-sort-field'));
            url.searchParams.set('sortDirection', sortButton.getAttribute('data-sort-direction'));
            url.searchParams.set('pageNumber', '1');
            window.location.href = url.toString();
        }
    });

    (function initReportModal() {
        var modalEl = document.getElementById('generateReportModal');
        if (!modalEl) {
            return;
        }

        var yearSelect = document.getElementById('reportYear');
        var curYear = new Date().getFullYear();
        var curMonth = new Date().getMonth() + 1;
        for (var year = curYear; year >= curYear - 5; year -= 1) {
            var option = document.createElement('option');
            option.value = year;
            option.textContent = year;
            if (year === curYear) {
                option.selected = true;
            }
            yearSelect.appendChild(option);
        }
        document.getElementById('reportMonth').value = String(curMonth);

        modalEl.addEventListener('show.bs.modal', function (e) {
            var trigger = e.relatedTarget;
            if (trigger && trigger.getAttribute('data-site-id')) {
                document.getElementById('reportSiteId').value = trigger.getAttribute('data-site-id');
            }
        });

        Array.from(document.querySelectorAll('.report-btn')).forEach(function (button) {
            button.addEventListener('click', function () {
                var siteId = document.getElementById('reportSiteId').value;
                if (!siteId) {
                    window.alert('Please select a site first.');
                    return;
                }
                downloadReport(
                    siteId,
                    document.getElementById('reportYear').value,
                    document.getElementById('reportMonth').value,
                    document.getElementById('reportFormat').value,
                    button.getAttribute('data-report-type'),
                    button
                );
            });
        });
    })();
})(window, document);
