(function (window, document) {
    'use strict';

    if (!window.AppApi || !window.AppCrud) {
        return;
    }

    var page = document.getElementById('billFormPage');
    var form = document.getElementById('billForm');
    if (!page || !form) {
        return;
    }

    var mode = page.getAttribute('data-mode') || 'create';
    var itemIndex = 0;
    var sites = [];
    var clients = [];
    var errorSummary = document.getElementById('billFormErrors');
    var submitButton = document.getElementById('billFormSubmitButton');

    function showError(message) {
        if (!errorSummary) {
            return;
        }
        errorSummary.textContent = message || 'Unable to save bill.';
        errorSummary.classList.remove('d-none');
    }

    function clearError() {
        if (!errorSummary) {
            return;
        }
        errorSummary.textContent = '';
        errorSummary.classList.add('d-none');
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

    function renderItem(item) {
        var container = document.getElementById('billItemsContainer');
        var html = [
            '<div class="card mb-3 bill-item" data-index="', itemIndex, '"><div class="card-body">',
            '<div class="d-flex justify-content-between align-items-center mb-3"><h6 class="mb-0">Item ', (itemIndex + 1), '</h6><button type="button" class="btn btn-sm btn-danger" data-remove-bill-item="', itemIndex, '"><i class="fas fa-trash"></i></button></div>',
            '<div class="row g-3">',
            '<div class="col-md-6"><label class="form-label">Item Name <span class="text-danger">*</span></label><input type="text" class="form-control" data-field="itemName" value="', window.AppCrud.escapeHtml(item.itemName || ''), '" required /></div>',
            '<div class="col-md-3"><label class="form-label">Quantity <span class="text-danger">*</span></label><input type="number" class="form-control" data-field="quantity" min="1" value="', window.AppCrud.escapeHtml(item.quantity != null ? item.quantity : 1), '" required /></div>',
            '<div class="col-md-3"><label class="form-label">Unit Price <span class="text-danger">*</span></label><input type="number" class="form-control" data-field="unitPrice" step="0.01" min="0" value="', window.AppCrud.escapeHtml(item.unitPrice != null ? item.unitPrice : 0), '" required /></div>',
            '</div>',
            '<div class="row g-3 mt-2">',
            '<div class="col-md-4"><label class="form-label">Tax Rate (%)</label><input type="number" class="form-control" data-field="taxRate" step="0.01" min="0" max="100" value="', window.AppCrud.escapeHtml(item.taxRate != null ? item.taxRate : 0), '" /></div>',
            '<div class="col-md-4"><label class="form-label">Discount Amount</label><input type="number" class="form-control" data-field="discountAmount" step="0.01" min="0" value="', window.AppCrud.escapeHtml(item.discountAmount != null ? item.discountAmount : 0), '" /></div>',
            '<div class="col-md-4"><label class="form-label">Description</label><input type="text" class="form-control" data-field="description" value="', window.AppCrud.escapeHtml(item.description || ''), '" /></div>',
            '</div></div></div>'
        ].join('');
        container.insertAdjacentHTML('beforeend', html);
        itemIndex += 1;
        calculateTotals();
    }

    function collectItems() {
        return Array.from(document.querySelectorAll('.bill-item')).map(function (row) {
            function value(field) {
                var el = row.querySelector('[data-field="' + field + '"]');
                return el ? el.value : '';
            }
            return {
                itemName: value('itemName'),
                description: value('description') || null,
                quantity: Number(value('quantity') || 0),
                unitPrice: Number(value('unitPrice') || 0),
                taxRate: Number(value('taxRate') || 0),
                discountAmount: Number(value('discountAmount') || 0)
            };
        }).filter(function (item) {
            return item.itemName;
        });
    }

    function calculateTotals() {
        var subTotal = 0;
        var taxTotal = 0;
        var discountTotal = 0;

        collectItems().forEach(function (item) {
            var itemSub = item.quantity * item.unitPrice;
            var itemAfterDiscount = itemSub - item.discountAmount;
            var itemTax = itemAfterDiscount * (item.taxRate / 100);
            subTotal += itemSub;
            taxTotal += itemTax;
            discountTotal += item.discountAmount;
        });

        taxTotal += Number(form.elements.TaxAmount.value || 0);
        discountTotal += Number(form.elements.DiscountAmount.value || 0);
        var total = subTotal - discountTotal + taxTotal;

        document.getElementById('subTotal').textContent = '₹' + subTotal.toFixed(2);
        var taxEl = document.getElementById('taxTotal') || document.getElementById('totalTax');
        var discountEl = document.getElementById('discountTotal') || document.getElementById('totalDiscount');
        var totalEl = document.getElementById('grandTotal') || document.getElementById('totalAmount');
        if (taxEl.tagName === 'INPUT') {
            taxEl.value = taxTotal.toFixed(2);
            discountEl.value = discountTotal.toFixed(2);
            totalEl.value = total.toFixed(2);
        } else {
            taxEl.textContent = '₹' + taxTotal.toFixed(2);
            discountEl.textContent = '₹' + discountTotal.toFixed(2);
            totalEl.textContent = '₹' + total.toFixed(2);
        }
    }

    function loadLookups() {
        return Promise.all([
            window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
            window.AppApi.get('Clients', { includeInactive: 'false', pageSize: '1000' })
        ]).then(function (results) {
            sites = results[0].items || [];
            clients = results[1].items || [];
            populateSelect(document.getElementById('billSiteId'), sites, 'Select Site', function (site) {
                return (site.siteName || '') + (site.clientName ? ' - ' + site.clientName : '');
            });
            var clientSelect = document.getElementById('billClientId');
            if (clientSelect) {
                populateSelect(clientSelect, clients, 'Select Client', function (client) {
                    return client.companyName || '';
                });
            }
        });
    }

    function fillForm(data) {
        Array.from(form.elements).forEach(function (element) {
            if (!element.name) {
                return;
            }
            var key = Object.keys(data || {}).find(function (name) {
                return name.toLowerCase() === element.name.toLowerCase();
            });
            if (!key) {
                return;
            }
            var value = data[key];
            if (element.type === 'date') {
                element.value = value ? String(value).slice(0, 10) : '';
            } else {
                element.value = value == null ? '' : value;
            }
        });

        var container = document.getElementById('billItemsContainer');
        container.innerHTML = '';
        itemIndex = 0;
        (data.items || []).forEach(function (item) {
            renderItem(item);
        });
        if (!(data.items || []).length) {
            renderItem({});
        }
        calculateTotals();
    }

    document.getElementById('addBillItemButton').addEventListener('click', function () {
        renderItem({ quantity: 1, unitPrice: 0, taxRate: 0, discountAmount: 0 });
    });

    form.addEventListener('click', function (event) {
        var removeBtn = event.target.closest('[data-remove-bill-item]');
        if (!removeBtn) {
            return;
        }
        var row = removeBtn.closest('.bill-item');
        if (row) {
            row.remove();
            calculateTotals();
        }
    });

    form.addEventListener('input', function (event) {
        if (event.target.closest('.bill-item') || event.target.name === 'TaxAmount' || event.target.name === 'DiscountAmount') {
            calculateTotals();
        }
    });

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        clearError();
        submitButton.disabled = true;

        var payload = {
            id: form.elements.Id ? form.elements.Id.value : undefined,
            billNumber: form.elements.BillNumber.value,
            billDate: form.elements.BillDate.value,
            siteId: form.elements.SiteId.value || null,
            clientId: form.elements.ClientId ? (form.elements.ClientId.value || null) : undefined,
            clientName: form.elements.ClientName.value,
            description: form.elements.Description.value || null,
            taxAmount: Number(form.elements.TaxAmount.value || 0),
            discountAmount: Number(form.elements.DiscountAmount.value || 0),
            paymentTerms: form.elements.PaymentTerms.value || null,
            dueDate: form.elements.DueDate.value || null,
            status: form.elements.Status.value,
            notes: form.elements.Notes.value || null,
            items: collectItems()
        };

        if (!payload.items.length) {
            showError('Add at least one bill item.');
            submitButton.disabled = false;
            return;
        }

        var request = mode === 'edit'
            ? window.AppApi.put(page.getAttribute('data-submit-endpoint'), payload)
            : window.AppApi.post(page.getAttribute('data-submit-endpoint'), payload);

        request.then(function () {
            if (window.AppUi) {
                window.AppUi.showToast({
                    type: 'success',
                    title: mode === 'edit' ? 'Updated' : 'Created',
                    message: mode === 'edit' ? 'Bill updated successfully.' : 'Bill created successfully.'
                });
            }
            window.setTimeout(function () {
                window.location.href = page.getAttribute('data-redirect-url');
            }, 300);
        }).catch(function (error) {
            showError(error && error.message ? error.message : 'Unable to save bill.');
        }).finally(function () {
            submitButton.disabled = false;
        });
    });

    loadLookups().then(function () {
        if (mode === 'edit') {
            return window.AppApi.get(page.getAttribute('data-load-endpoint')).then(fillForm);
        }
        renderItem({ quantity: 1, unitPrice: 0, taxRate: 0, discountAmount: 0 });
        calculateTotals();
        return null;
    }).catch(function (error) {
        showError(error && error.message ? error.message : 'Unable to load bill form data.');
    });
})(window, document);
