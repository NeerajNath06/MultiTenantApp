(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    function currency(value) {
        return '\u20b9' + Number(value || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    window.AppCrud.createDetailsPage({
        pageId: 'wageDetailsPage',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');
            var tableWrapper = document.getElementById('wageDetailsTableWrapper');

            helpers.setText('wageSheetNumber', data.wageSheetNumber);
            helpers.setText('wagePeriod', new Date(data.wagePeriodStart).toLocaleDateString() + ' - ' + new Date(data.wagePeriodEnd).toLocaleDateString());
            helpers.setText('paymentDate', data.paymentDate ? new Date(data.paymentDate).toLocaleDateString() : '-');
            helpers.setText('totalWages', currency(data.totalWages));
            helpers.setText('totalAllowances', currency(data.totalAllowances));
            helpers.setText('totalDeductions', currency(data.totalDeductions));
            helpers.setText('netAmount', currency(data.netAmount));
            helpers.setText('notes', data.notes);

            if (statusBadge) {
                var normalized = String(data.status || '').toLowerCase();
                var className = normalized === 'paid' ? 'success' : normalized === 'approved' ? 'info' : 'secondary';
                statusBadge.textContent = data.status || '-';
                statusBadge.className = 'badge bg-' + className;
            }

            if (!tableWrapper) {
                return;
            }

            if (!data.details || !data.details.length) {
                tableWrapper.innerHTML = '<div class="text-muted">No wage details available.</div>';
                return;
            }

            tableWrapper.innerHTML = [
                '<div class="table-responsive">',
                '<table class="table table-sm table-hover">',
                '<thead class="table-light">',
                '<tr>',
                '<th>Guard</th>',
                '<th class="text-end">Days</th>',
                '<th class="text-end">Basic Rate</th>',
                '<th class="text-end">Basic</th>',
                '<th class="text-end">Overtime</th>',
                '<th class="text-end">Allowances</th>',
                '<th class="text-end">Deductions</th>',
                '<th class="text-end">Net</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                data.details.map(function (item) {
                    return [
                        '<tr>',
                        '<td><strong>' + window.AppCrud.escapeHtml(item.guardName || '-') + '</strong>' + (item.designation ? '<br><small class="text-muted">' + window.AppCrud.escapeHtml(item.designation) + '</small>' : '') + '</td>',
                        '<td class="text-end">' + window.AppCrud.escapeHtml(item.daysWorked) + '</td>',
                        '<td class="text-end">' + currency(item.basicRate) + '</td>',
                        '<td class="text-end">' + currency(item.basicAmount) + '</td>',
                        '<td class="text-end">' + currency(item.overtimeAmount) + '</td>',
                        '<td class="text-end">' + currency(item.allowances) + '</td>',
                        '<td class="text-end">' + currency(item.deductions) + '</td>',
                        '<td class="text-end"><strong>' + currency(item.netAmount) + '</strong></td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        }
    });
})(window, document);
