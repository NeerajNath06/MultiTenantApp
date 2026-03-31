(function (window, document) {
    'use strict';

    if (!window.AppApi || !window.AppCrud) {
        return;
    }

    var page = document.getElementById('wageFormPage');
    var form = document.getElementById('wageForm');
    if (!page || !form) {
        return;
    }

    var guards = [];
    var sites = [];
    var shifts = [];
    var detailIndex = 0;
    var errorSummary = document.getElementById('wageFormErrors');
    var submitButton = document.getElementById('wageFormSubmitButton');

    function showError(message) {
        if (!errorSummary) {
            return;
        }
        errorSummary.textContent = message || 'Unable to save wage sheet.';
        errorSummary.classList.remove('d-none');
    }

    function clearError() {
        if (!errorSummary) {
            return;
        }
        errorSummary.textContent = '';
        errorSummary.classList.add('d-none');
    }

    function optionHtml(items, placeholder, labelSelector) {
        return ['<option value="">', placeholder, '</option>'].join('') + (items || []).map(function (item) {
            return '<option value="' + item.id + '">' + window.AppCrud.escapeHtml(labelSelector(item)) + '</option>';
        }).join('');
    }

    function addWageDetail() {
        var container = document.getElementById('wageDetailsContainer');
        var html = [
            '<div class="card mb-3 wage-detail" data-index="', detailIndex, '">',
            '<div class="card-body">',
            '<div class="d-flex justify-content-between align-items-center mb-3">',
            '<h6 class="mb-0">Guard ', (detailIndex + 1), '</h6>',
            '<button type="button" class="btn btn-sm btn-danger" data-remove-wage-detail="', detailIndex, '"><i class="fas fa-trash"></i></button>',
            '</div>',
            '<div class="row g-3">',
            '<div class="col-md-4"><label class="form-label">Guard <span class="text-danger">*</span></label><select class="form-select" data-field="guardId" required>', optionHtml(guards, 'Select Guard', function (g) { return (g.guardCode || '') + ' - ' + ((g.firstName || '') + ' ' + (g.lastName || '')).trim(); }), '</select></div>',
            '<div class="col-md-4"><label class="form-label">Site</label><select class="form-select" data-field="siteId">', optionHtml(sites, 'Select Site', function (s) { return s.siteName || ''; }), '</select></div>',
            '<div class="col-md-4"><label class="form-label">Shift</label><select class="form-select" data-field="shiftId">', optionHtml(shifts, 'Select Shift', function (s) { return s.shiftName || ''; }), '</select></div>',
            '</div>',
            '<div class="row g-3 mt-2">',
            '<div class="col-md-3"><label class="form-label">Days Worked</label><input type="number" class="form-control" data-field="daysWorked" min="0" value="0" /></div>',
            '<div class="col-md-3"><label class="form-label">Hours Worked <span class="text-danger">*</span></label><input type="number" class="form-control" data-field="hoursWorked" min="0" value="0" required /></div>',
            '<div class="col-md-3"><label class="form-label">Basic Rate <span class="text-danger">*</span></label><input type="number" class="form-control" data-field="basicRate" min="0" step="0.01" value="0" required /></div>',
            '<div class="col-md-3"><label class="form-label">Overtime Hours</label><input type="number" class="form-control" data-field="overtimeHours" min="0" step="0.01" value="0" /></div>',
            '</div>',
            '<div class="row g-3 mt-2">',
            '<div class="col-md-3"><label class="form-label">Overtime Rate</label><input type="number" class="form-control" data-field="overtimeRate" min="0" step="0.01" value="0" /></div>',
            '<div class="col-md-3"><label class="form-label">Allowances</label><input type="number" class="form-control" data-field="allowances" min="0" step="0.01" value="0" /></div>',
            '<div class="col-md-3"><label class="form-label">Deductions</label><input type="number" class="form-control" data-field="deductions" min="0" step="0.01" value="0" /></div>',
            '<div class="col-md-3"><label class="form-label">Remarks</label><input type="text" class="form-control" data-field="remarks" /></div>',
            '</div>',
            '</div>',
            '</div>'
        ].join('');
        container.insertAdjacentHTML('beforeend', html);
        detailIndex += 1;
        calculateTotals();
    }

    function collectDetails() {
        return Array.from(document.querySelectorAll('.wage-detail')).map(function (detail) {
            function value(field) {
                var element = detail.querySelector('[data-field="' + field + '"]');
                return element ? element.value : '';
            }

            return {
                guardId: value('guardId'),
                siteId: value('siteId') || null,
                shiftId: value('shiftId') || null,
                daysWorked: Number(value('daysWorked') || 0),
                hoursWorked: Number(value('hoursWorked') || 0),
                basicRate: Number(value('basicRate') || 0),
                overtimeHours: Number(value('overtimeHours') || 0),
                overtimeRate: Number(value('overtimeRate') || 0),
                allowances: Number(value('allowances') || 0),
                deductions: Number(value('deductions') || 0),
                remarks: value('remarks') || null
            };
        }).filter(function (item) {
            return item.guardId;
        });
    }

    function calculateTotals() {
        var details = collectDetails();
        var totalWages = 0;
        var detailAllowances = 0;
        var detailDeductions = 0;

        details.forEach(function (item) {
            totalWages += (item.hoursWorked * item.basicRate) + (item.overtimeHours * item.overtimeRate) + item.allowances;
            detailAllowances += item.allowances;
            detailDeductions += item.deductions;
        });

        var totalAllowances = Number(form.elements.TotalAllowances.value || 0) + detailAllowances;
        var totalDeductions = Number(form.elements.TotalDeductions.value || 0) + detailDeductions;
        var netAmount = totalWages + totalAllowances - totalDeductions;

        document.getElementById('totalWages').textContent = '\u20b9' + totalWages.toFixed(2);
        document.getElementById('totalAllowances').textContent = '\u20b9' + totalAllowances.toFixed(2);
        document.getElementById('totalDeductions').textContent = '\u20b9' + totalDeductions.toFixed(2);
        document.getElementById('netAmount').textContent = '\u20b9' + netAmount.toFixed(2);
    }

    document.getElementById('addWageDetailButton').addEventListener('click', addWageDetail);

    form.addEventListener('input', function (event) {
        if (event.target.closest('.wage-detail') || event.target.name === 'TotalAllowances' || event.target.name === 'TotalDeductions') {
            calculateTotals();
        }
    });

    form.addEventListener('click', function (event) {
        var removeButton = event.target.closest('[data-remove-wage-detail]');
        if (!removeButton) {
            return;
        }

        var detail = removeButton.closest('.wage-detail');
        if (detail) {
            detail.remove();
            calculateTotals();
        }
    });

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        clearError();
        submitButton.disabled = true;

        var payload = {
            wageSheetNumber: form.elements.WageSheetNumber.value,
            wagePeriodStart: form.elements.WagePeriodStart.value,
            wagePeriodEnd: form.elements.WagePeriodEnd.value,
            paymentDate: form.elements.PaymentDate.value,
            status: form.elements.Status.value,
            totalAllowances: Number(form.elements.TotalAllowances.value || 0),
            totalDeductions: Number(form.elements.TotalDeductions.value || 0),
            notes: form.elements.Notes.value || null,
            wageDetails: collectDetails()
        };

        if (!payload.wageDetails.length) {
            showError('Add at least one guard wage detail.');
            submitButton.disabled = false;
            return;
        }

        window.AppApi.post(page.getAttribute('data-submit-endpoint'), payload)
            .then(function () {
                if (window.AppUi) {
                    window.AppUi.showToast({
                        type: 'success',
                        title: 'Created',
                        message: 'Wage sheet created successfully.'
                    });
                }
                window.setTimeout(function () {
                    window.location.href = page.getAttribute('data-redirect-url');
                }, 300);
            })
            .catch(function (error) {
                showError(error && error.message ? error.message : 'Unable to save wage sheet.');
            })
            .finally(function () {
                submitButton.disabled = false;
            });
    });

    Promise.all([
        window.AppApi.get('SecurityGuards', { includeInactive: 'false', pageSize: '1000' }),
        window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
        window.AppApi.get('Shifts', { includeInactive: 'false', pageSize: '1000' })
    ]).then(function (results) {
        guards = results[0].items || [];
        sites = results[1].items || [];
        shifts = results[2].items || [];
        addWageDetail();
    }).catch(function (error) {
        showError(error && error.message ? error.message : 'Unable to load wage form data.');
    });
})(window, document);
