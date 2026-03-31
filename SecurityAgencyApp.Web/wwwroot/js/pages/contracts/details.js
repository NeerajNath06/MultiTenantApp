(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    function money(value) {
        return '₹' + Number(value || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    window.AppCrud.createDetailsPage({
        pageId: 'contractDetailsPage',
        deleteButtonId: 'contractDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'Contracts/' + page.getAttribute('data-contract-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete this contract?';
        },
        deleteSuccessMessage: 'Contract deleted successfully.',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');
            var autoRenewalBadge = page.querySelector('[data-field="autoRenewalBadge"]');
            var sitesWrapper = document.getElementById('contractSitesWrapper');

            helpers.setText('contractNumber', data.contractNumber);
            helpers.setText('clientName', data.clientName);
            helpers.setText('title', data.title);
            helpers.setText('startDate', data.startDate ? new Date(data.startDate).toLocaleDateString() : '-');
            helpers.setText('endDate', data.endDate ? new Date(data.endDate).toLocaleDateString() : '-');
            helpers.setText('contractValue', money(data.contractValue));
            helpers.setText('monthlyAmount', money(data.monthlyAmount));
            helpers.setText('billingCycle', data.billingCycle);
            helpers.setText('numberOfGuards', data.numberOfGuards);
            helpers.setText('description', data.description);
            helpers.setText('paymentTerms', data.paymentTerms);
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));
            helpers.setText('renewalDate', data.renewalDate ? new Date(data.renewalDate).toLocaleDateString() : '-');

            if (statusBadge) {
                var status = String(data.status || '').toLowerCase();
                var statusClass = status === 'active' ? 'success' : status === 'expired' ? 'danger' : 'secondary';
                statusBadge.textContent = data.status || '-';
                statusBadge.className = 'badge bg-' + statusClass;
            }

            if (autoRenewalBadge) {
                autoRenewalBadge.textContent = data.autoRenewal ? 'Yes' : 'No';
                autoRenewalBadge.className = 'badge ' + (data.autoRenewal ? 'bg-success' : 'bg-secondary');
            }

            if (!sitesWrapper) {
                return;
            }

            if (!data.sites || !data.sites.length) {
                sitesWrapper.innerHTML = '<div class="text-muted">No contract sites configured.</div>';
                return;
            }

            sitesWrapper.innerHTML = [
                '<div class="table-responsive"><table class="table table-sm"><thead><tr><th>Site Name</th><th>Required Guards</th><th>Shift Details</th></tr></thead><tbody>',
                data.sites.map(function (site) {
                    return '<tr><td>' + window.AppCrud.escapeHtml(site.siteName || '-') + '</td><td>' + window.AppCrud.escapeHtml(site.requiredGuards) + '</td><td>' + window.AppCrud.escapeHtml(site.shiftDetails || '-') + '</td></tr>';
                }).join(''),
                '</tbody></table></div>'
            ].join('');
        }
    });
})(window, document);
