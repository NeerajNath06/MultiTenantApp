(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('contractsIndexPage');
    if (!page) {
        return;
    }

    function populateSelect(select, items, placeholder, labelSelector) {
        if (!select) {
            return;
        }

        var currentValue = select.value;
        select.innerHTML = '<option value="">' + placeholder + '</option>' + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(labelSelector(item)) + '</option>';
        }).join('');
        if (currentValue) {
            select.value = currentValue;
        }
    }

    function bindFilters() {
        var params = new URLSearchParams(window.location.search);
        document.getElementById('contractsSearch').value = params.get('search') || '';
        document.getElementById('contractsClientId').value = params.get('clientId') || '';
        document.getElementById('contractsStatus').value = params.get('status') || '';
    }

    function setStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    window.AppApi.get('Clients', { includeInactive: 'false', pageSize: '1000' }).then(function (data) {
        populateSelect(document.getElementById('contractsClientId'), data.items || [], 'All Clients', function (item) {
            return item.companyName || '';
        });
        bindFilters();
    });

    window.AppCrud.createListPage({
        pageId: 'contractsIndexPage',
        filterFormId: 'contractsFilterForm',
        tableWrapperId: 'contractsTableWrapper',
        recordCountId: 'contractsRecordCount',
        paginationContainerId: 'contractsPaginationContainer',
        paginationSummaryId: 'contractsPaginationSummary',
        paginationId: 'contractsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Contracts',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var active = 0;
            var expired = 0;
            (items || []).forEach(function (item) {
                var status = String(item.status || '').toLowerCase();
                if (status === 'active') {
                    active += 1;
                }
                if (status === 'expired') {
                    expired += 1;
                }
            });
            setStat('total', data.totalCount || items.length);
            setStat('active', active);
            setStat('expired', expired);
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-file-contract fa-3x text-muted mb-3"></i><h5 class="text-muted">No contracts found</h5><a href="' + page.getAttribute('data-create-url') + '" class="btn btn-primary mt-3"><i class="fas fa-file-contract me-2"></i>Create New Contract</a></div>';
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');
            var editBase = page.getAttribute('data-edit-base');
            return [
                '<div class="table-responsive"><table class="table table-hover align-middle"><thead class="table-light"><tr>',
                '<th>Contract Number</th><th>Client</th><th>Title</th><th>Start Date</th><th>End Date</th><th class="text-end">Contract Value</th><th>Status</th><th class="text-end">Actions</th>',
                '</tr></thead><tbody>',
                context.items.map(function (contract) {
                    var status = String(contract.status || '').toLowerCase();
                    var statusClass = status === 'active' ? 'success' : status === 'expired' ? 'danger' : status === 'draft' ? 'secondary' : 'info';
                    return '<tr>'
                        + '<td><strong>' + context.escapeHtml(contract.contractNumber) + '</strong></td>'
                        + '<td>' + context.escapeHtml(contract.clientName) + '</td>'
                        + '<td>' + context.escapeHtml(contract.title) + '</td>'
                        + '<td>' + context.escapeHtml(new Date(contract.startDate).toLocaleDateString()) + '</td>'
                        + '<td>' + context.escapeHtml(new Date(contract.endDate).toLocaleDateString()) + '</td>'
                        + '<td class="text-end">₹' + Number(contract.contractValue || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '</td>'
                        + '<td><span class="badge bg-' + statusClass + '">' + context.escapeHtml(contract.status || '-') + '</span></td>'
                        + '<td class="text-end"><div class="btn-group btn-group-sm">'
                        + '<a href="' + context.replaceToken(detailsBase, '__id__', contract.id) + '" class="btn btn-outline-primary" title="View"><i class="fas fa-eye"></i></a>'
                        + '<a href="' + context.replaceToken(editBase, '__id__', contract.id) + '" class="btn btn-outline-secondary" title="Edit"><i class="fas fa-edit"></i></a>'
                        + '<button type="button" class="btn btn-outline-danger" title="Delete" data-crud-delete-id="' + contract.id + '"><i class="fas fa-trash"></i></button>'
                        + '</div></td>'
                        + '</tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this contract?';
        },
        deleteSuccessMessage: 'Contract deleted successfully.',
        loadErrorMessage: 'Unable to load contracts right now.'
    });
})(window, document);
