(function (window, document) {
    'use strict';

    if (!window.AppApi || !window.AppCrud) {
        return;
    }

    var page = document.getElementById('siteRatesPage');
    if (!page) {
        return;
    }

    var siteId = page.getAttribute('data-site-id');
    var form = document.getElementById('siteRatesForm');
    var submitButton = document.getElementById('siteRatesSubmitButton');
    var errorSummary = document.getElementById('siteRatesFormErrors');
    var currentSite = null;

    function showError(message) {
        errorSummary.textContent = message || 'Unable to save rate.';
        errorSummary.classList.remove('d-none');
    }

    function clearError() {
        errorSummary.textContent = '';
        errorSummary.classList.add('d-none');
    }

    function money(value) {
        return '₹ ' + Number(value || 0).toFixed(2);
    }

    function renderHistory(items) {
        var wrapper = document.getElementById('siteRatesHistoryWrapper');
        if (!items || !items.length) {
            wrapper.innerHTML = '<div class="text-muted">No rate plans found.</div>';
            return;
        }
        wrapper.innerHTML = '<table class="table table-hover align-middle"><thead class="table-light"><tr><th>Rate</th><th>Effective From</th><th>Effective To</th><th>Status</th></tr></thead><tbody>'
            + items.map(function (rate) {
                return '<tr><td><span class="badge bg-secondary">' + money(rate.rateAmount) + '</span></td><td>' + window.AppCrud.escapeHtml(new Date(rate.effectiveFrom).toLocaleDateString()) + '</td><td>' + window.AppCrud.escapeHtml(rate.effectiveTo ? new Date(rate.effectiveTo).toLocaleDateString() : '-') + '</td><td>' + (rate.isActive ? '<span class="badge bg-success">Active</span>' : '<span class="badge bg-light text-dark">Inactive</span>') + '</td></tr>';
            }).join('') + '</tbody></table>';
    }

    function loadPage() {
        return Promise.all([
            window.AppApi.get('Sites/' + siteId),
            window.AppApi.get('SiteRates/' + siteId + '/history', { includeInactive: 'true' })
        ]).then(function (results) {
            currentSite = results[0];
            page.querySelector('[data-field="siteName"]').textContent = currentSite.siteName || '-';
            page.querySelector('[data-field="clientName"]').textContent = currentSite.clientName || '-';
            renderHistory(results[1] || []);
        });
    }

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        clearError();

        if (!currentSite || !currentSite.clientId) {
            showError('This site must be linked to a client before saving rates.');
            return;
        }

        submitButton.disabled = true;
        window.AppApi.post('SiteRates', {
            siteId: siteId,
            clientId: currentSite.clientId,
            rateAmount: Number(form.elements.rateAmount.value || 0),
            effectiveFrom: form.elements.effectiveFrom.value,
            effectiveTo: form.elements.effectiveTo.value || null
        }).then(function () {
            if (window.AppUi) {
                window.AppUi.showToast({ type: 'success', title: 'Saved', message: 'Rate plan saved.' });
            }
            form.reset();
            form.elements.effectiveFrom.value = new Date().toISOString().slice(0, 10);
            return loadPage();
        }).catch(function (error) {
            showError(error && error.message ? error.message : 'Unable to save rate.');
        }).finally(function () {
            submitButton.disabled = false;
        });
    });

    loadPage().catch(function (error) {
        showError(error && error.message ? error.message : 'Unable to load rate management page.');
    });
})(window, document);
