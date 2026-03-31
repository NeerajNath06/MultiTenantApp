(function (window, document) {
    'use strict';

    if (!window.AppApi || !window.AppCrud) {
        return;
    }

    var page = document.getElementById('siteFormPage');
    var form = document.getElementById('siteForm');
    if (!page || !form) {
        return;
    }

    var mode = page.getAttribute('data-mode') || 'create';
    var siteId = page.getAttribute('data-site-id') || '';
    var selectedSupervisorIds = [];
    var currentRatePlanId = '';
    var reportInitialized = false;

    function showError(message) {
        var errorSummary = document.getElementById('siteFormErrors');
        if (errorSummary) {
            errorSummary.textContent = message || 'Unable to save site.';
            errorSummary.classList.remove('d-none');
        }
    }

    function clearError() {
        var errorSummary = document.getElementById('siteFormErrors');
        if (errorSummary) {
            errorSummary.textContent = '';
            errorSummary.classList.add('d-none');
        }
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

    function buildSupervisorMenu(items) {
        var menu = document.getElementById('siteSupervisorsMenu');
        var label = document.getElementById('siteSupervisorsLabel');
        var hiddenContainer = document.getElementById('siteSupervisorsHiddenContainer');
        if (!menu || !label || !hiddenContainer) {
            return;
        }

        menu.innerHTML = (items || []).map(function (item) {
            var name = ((item.firstName || '') + ' ' + (item.lastName || '')).trim() || item.userName || '';
            return '<li><label class="dropdown-item d-flex align-items-center gap-2 cursor-pointer mb-0"><input type="checkbox" class="form-check-input site-supervisor-cb" value="' + item.id + '" data-name="' + window.AppCrud.escapeHtml(name) + '"' + (selectedSupervisorIds.indexOf(item.id) >= 0 ? ' checked' : '') + ' /><span>' + window.AppCrud.escapeHtml(name) + '</span></label></li>';
        }).join('');

        function sync() {
            hiddenContainer.innerHTML = '';
            var names = [];
            Array.from(menu.querySelectorAll('.site-supervisor-cb')).forEach(function (checkbox) {
                if (checkbox.checked) {
                    hiddenContainer.insertAdjacentHTML('beforeend', '<input type="hidden" name="SupervisorIds" value="' + checkbox.value + '" />');
                    names.push(checkbox.getAttribute('data-name'));
                }
            });
            selectedSupervisorIds = Array.from(menu.querySelectorAll('.site-supervisor-cb:checked')).map(function (checkbox) { return checkbox.value; });
            label.textContent = names.length ? names.join(', ') : '-- Select Supervisors --';
        }

        Array.from(menu.querySelectorAll('.site-supervisor-cb')).forEach(function (checkbox) {
            checkbox.addEventListener('change', sync);
        });
        menu.addEventListener('click', function (event) {
            event.stopPropagation();
        });
        sync();
    }

    function createPostRow(post) {
        var row = document.createElement('tr');
        row.className = 'site-post-row';
        row.innerHTML =
            '<td>'
                + '<input type="hidden" data-post-field="Id" value="' + window.AppCrud.escapeHtml(post.id || '00000000-0000-0000-0000-000000000000') + '" />'
                + '<input type="text" class="form-control form-control-sm mb-2" data-post-field="PostCode" value="' + window.AppCrud.escapeHtml(post.postCode || '') + '" placeholder="Code" />'
                + '<input type="text" class="form-control form-control-sm" data-post-field="PostName" value="' + window.AppCrud.escapeHtml(post.postName || '') + '" placeholder="Post name" />'
            + '</td>'
            + '<td>'
                + '<input type="text" class="form-control form-control-sm mb-2" data-post-field="ShiftName" value="' + window.AppCrud.escapeHtml(post.shiftName || '') + '" placeholder="Shift" />'
                + '<input type="text" class="form-control form-control-sm" data-post-field="WeeklyOffPattern" value="' + window.AppCrud.escapeHtml(post.weeklyOffPattern || '') + '" placeholder="Weekly off pattern" />'
            + '</td>'
            + '<td>'
                + '<input type="number" min="0" class="form-control form-control-sm mb-2" data-post-field="SanctionedStrength" value="' + window.AppCrud.escapeHtml(post.sanctionedStrength != null ? post.sanctionedStrength : 1) + '" />'
                + '<select class="form-select form-select-sm" data-post-field="IsActive"><option value="true"' + (post.isActive !== false ? ' selected' : '') + '>Active</option><option value="false"' + (post.isActive === false ? ' selected' : '') + '>Inactive</option></select>'
            + '</td>'
            + '<td>'
                + '<input type="text" class="form-control form-control-sm mb-2" data-post-field="GenderRequirement" value="' + window.AppCrud.escapeHtml(post.genderRequirement || '') + '" placeholder="Gender requirement" />'
                + '<input type="text" class="form-control form-control-sm" data-post-field="SkillRequirement" value="' + window.AppCrud.escapeHtml(post.skillRequirement || '') + '" placeholder="Skill requirement" />'
            + '</td>'
            + '<td>'
                + '<select class="form-select form-select-sm mb-2" data-post-field="RequiresWeapon"><option value="false"' + (!post.requiresWeapon ? ' selected' : '') + '>Weapon: No</option><option value="true"' + (post.requiresWeapon ? ' selected' : '') + '>Weapon: Yes</option></select>'
                + '<select class="form-select form-select-sm" data-post-field="RelieverRequired"><option value="false"' + (!post.relieverRequired ? ' selected' : '') + '>Reliever: No</option><option value="true"' + (post.relieverRequired ? ' selected' : '') + '>Reliever: Yes</option></select>'
            + '</td>'
            + '<td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger remove-site-post-btn"><i class="fas fa-trash"></i></button></td>';
        return row;
    }

    function initPostsEditor(posts) {
        var table = document.getElementById('sitePostsTable');
        var addBtn = document.getElementById('addSitePostBtn');
        if (!table || !addBtn || table.dataset.sitePlanningInitialized === '1') {
            return;
        }

        table.dataset.sitePlanningInitialized = '1';
        var tbody = table.querySelector('tbody');

        function renderEmptyState() {
            tbody.innerHTML = '<tr class="site-post-empty-row"><td colspan="6" class="text-center text-muted py-4">No posts added yet. Use <b>Add Post</b> to define staffing positions.</td></tr>';
        }

        function renderPosts(items) {
            tbody.innerHTML = '';
            if (!items || !items.length) {
                renderEmptyState();
                return;
            }
            items.forEach(function (item) {
                tbody.appendChild(createPostRow(item));
            });
        }

        addBtn.addEventListener('click', function () {
            var emptyRow = tbody.querySelector('.site-post-empty-row');
            if (emptyRow) {
                emptyRow.remove();
            }
            tbody.appendChild(createPostRow({}));
        });

        table.addEventListener('click', function (event) {
            var removeBtn = event.target.closest('.remove-site-post-btn');
            if (!removeBtn) {
                return;
            }
            var row = removeBtn.closest('.site-post-row');
            if (row) {
                row.remove();
            }
            if (!tbody.querySelector('.site-post-row')) {
                renderEmptyState();
            }
        });

        renderPosts(posts || []);
    }

    function collectPosts() {
        return Array.from(document.querySelectorAll('#sitePostsTable .site-post-row')).map(function (row) {
            function value(field) {
                var element = row.querySelector('[data-post-field="' + field + '"]');
                return element ? element.value : '';
            }
            return {
                id: value('Id'),
                postCode: value('PostCode'),
                postName: value('PostName'),
                shiftName: value('ShiftName'),
                weeklyOffPattern: value('WeeklyOffPattern'),
                sanctionedStrength: Number(value('SanctionedStrength') || 0),
                isActive: value('IsActive') !== 'false',
                genderRequirement: value('GenderRequirement') || null,
                skillRequirement: value('SkillRequirement') || null,
                requiresWeapon: value('RequiresWeapon') === 'true',
                relieverRequired: value('RelieverRequired') === 'true'
            };
        }).filter(function (post) {
            return post.postCode || post.postName || post.shiftName;
        });
    }

    function collectDeploymentPlan() {
        function named(name) {
            return form.querySelector('[name="' + name + '"]');
        }

        return {
            id: named('ActiveDeploymentPlan.Id') ? named('ActiveDeploymentPlan.Id').value || null : null,
            effectiveFrom: named('ActiveDeploymentPlan.EffectiveFrom') ? named('ActiveDeploymentPlan.EffectiveFrom').value || null : null,
            effectiveTo: named('ActiveDeploymentPlan.EffectiveTo') ? named('ActiveDeploymentPlan.EffectiveTo').value || null : null,
            emergencyContactSet: named('ActiveDeploymentPlan.EmergencyContactSet') ? named('ActiveDeploymentPlan.EmergencyContactSet').value || null : null,
            isActive: named('ActiveDeploymentPlan.IsActive') ? named('ActiveDeploymentPlan.IsActive').value !== 'false' : true,
            reservePoolMapping: named('ActiveDeploymentPlan.ReservePoolMapping') ? named('ActiveDeploymentPlan.ReservePoolMapping').value || null : null,
            accessZones: named('ActiveDeploymentPlan.AccessZones') ? named('ActiveDeploymentPlan.AccessZones').value || null : null,
            instructionSummary: named('ActiveDeploymentPlan.InstructionSummary') ? named('ActiveDeploymentPlan.InstructionSummary').value || null : null,
            notes: named('ActiveDeploymentPlan.Notes') ? named('ActiveDeploymentPlan.Notes').value || null : null
        };
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
            if (element.type === 'checkbox') {
                element.checked = !!value;
            } else if (element.type === 'date') {
                element.value = value ? String(value).slice(0, 10) : '';
            } else {
                element.value = value == null ? '' : value;
            }
        });

        selectedSupervisorIds = (data.supervisorIds || []).map(function (id) { return String(id); });
        initPostsEditor(data.posts || []);

        var plan = data.activeDeploymentPlan || {};
        Array.from(form.querySelectorAll('[name^="ActiveDeploymentPlan."]')).forEach(function (element) {
            var key = element.name.replace('ActiveDeploymentPlan.', '');
            var value = plan[key.charAt(0).toLowerCase() + key.slice(1)];
            if (element.type === 'date') {
                element.value = value ? String(value).slice(0, 10) : '';
            } else {
                element.value = value == null ? element.value || '' : value;
            }
        });
    }

    function loadCurrentRate() {
        if (mode !== 'edit') {
            return Promise.resolve();
        }
        return window.AppApi.get('SiteRates/' + siteId, { asOfDate: new Date().toISOString().slice(0, 10) }).then(function (rate) {
            document.getElementById('siteRateCurrentRate').value = rate ? Number(rate.rateAmount || 0).toFixed(2) : 'Not set';
            document.getElementById('siteRateCurrentClient').value = rate && rate.clientName ? rate.clientName : '-';
            if (rate && rate.effectiveFrom) {
                document.getElementById('rateEffectiveFrom').value = String(rate.effectiveFrom).slice(0, 10);
            }
        }).catch(function () {
            document.getElementById('siteRateCurrentRate').value = 'Not set';
            document.getElementById('siteRateCurrentClient').value = '-';
        });
    }

    function renderRateHistory(items) {
        var wrapper = document.getElementById('siteRateHistoryWrapper');
        if (!wrapper) {
            return;
        }
        if (!items || !items.length) {
            wrapper.innerHTML = '<div class="text-muted">No rate plans found.</div>';
            return;
        }

        wrapper.innerHTML = '<div class="table-responsive"><table class="table table-sm table-bordered"><thead class="table-light"><tr><th>Effective From</th><th>Effective To</th><th>Rate (₹)</th><th>Status</th><th style="width:140px;">Actions</th></tr></thead><tbody>'
            + items.map(function (rate) {
                return '<tr>'
                    + '<td>' + window.AppCrud.escapeHtml(new Date(rate.effectiveFrom).toLocaleDateString()) + '</td>'
                    + '<td>' + window.AppCrud.escapeHtml(rate.effectiveTo ? new Date(rate.effectiveTo).toLocaleDateString() : '—') + '</td>'
                    + '<td>' + Number(rate.rateAmount || 0).toFixed(2) + '</td>'
                    + '<td>' + (rate.isActive ? 'Active' : 'Inactive') + '</td>'
                    + '<td><button type="button" class="btn btn-sm btn-outline-secondary me-1 rate-plan-edit" data-rate-plan=\'' + JSON.stringify(rate).replace(/'/g, '&#39;') + '\'>Edit</button><button type="button" class="btn btn-sm btn-outline-danger rate-plan-delete" data-rate-plan-id="' + rate.id + '">Delete</button></td>'
                    + '</tr>';
            }).join('') + '</tbody></table></div>';
    }

    function loadRateHistory() {
        if (mode !== 'edit') {
            return Promise.resolve();
        }
        return window.AppApi.get('SiteRates/' + siteId + '/history', { includeInactive: 'true' }).then(function (items) {
            renderRateHistory(items || []);
        });
    }

    function setRateValidation(message) {
        var element = document.getElementById('siteRateValidationMessage');
        if (!element) {
            return;
        }
        if (!message) {
            element.textContent = '';
            element.classList.add('d-none');
            return;
        }
        element.textContent = message;
        element.classList.remove('d-none');
    }

    function saveRate() {
        var clientSelect = document.getElementById('siteClientIdDropdown');
        var amountInput = document.getElementById('rateAmount');
        if (!clientSelect || !clientSelect.value) {
            setRateValidation('Select a client before saving the site rate.');
            return;
        }
        if (!amountInput.value || Number(amountInput.value) <= 0) {
            setRateValidation('Enter a valid rate amount greater than zero.');
            return;
        }
        setRateValidation('');

        var payload = {
            id: currentRatePlanId || null,
            siteId: siteId,
            clientId: clientSelect.value,
            rateAmount: Number(amountInput.value || 0),
            effectiveFrom: document.getElementById('rateEffectiveFrom').value || new Date().toISOString().slice(0, 10),
            effectiveTo: null,
            epfPercent: document.getElementById('rateEpfPercent').value ? Number(document.getElementById('rateEpfPercent').value) : null,
            esicPercent: document.getElementById('rateEsicPercent').value ? Number(document.getElementById('rateEsicPercent').value) : null,
            allowancePercent: document.getElementById('rateAllowancePercent').value ? Number(document.getElementById('rateAllowancePercent').value) : null,
            epfWageCap: document.getElementById('rateEpfWageCap').value ? Number(document.getElementById('rateEpfWageCap').value) : null
        };

        var button = document.getElementById('saveSiteRateButton');
        button.disabled = true;
        window.AppApi.post('SiteRates', payload).then(function () {
            currentRatePlanId = '';
            amountInput.value = '';
            document.getElementById('rateEpfPercent').value = '';
            document.getElementById('rateEsicPercent').value = '';
            document.getElementById('rateAllowancePercent').value = '';
            document.getElementById('rateEpfWageCap').value = '';
            if (window.AppUi) {
                window.AppUi.showToast({ type: 'success', title: 'Saved', message: 'Site rate saved successfully.' });
            }
            return Promise.all([loadCurrentRate(), loadRateHistory()]);
        }).catch(function (error) {
            setRateValidation(error && error.message ? error.message : 'Unable to save site rate.');
        }).finally(function () {
            button.disabled = false;
        });
    }

    function reportEndpoint(reportType) {
        switch ((reportType || '').toLowerCase()) {
            case 'bill': return 'Report/generate-bill';
            case 'attendance': return 'Report/generate-attendance';
            case 'wages': return 'Report/generate-wages';
            case 'full': return 'Report/generate-full-report';
            default: return null;
        }
    }

    function initReportModal() {
        if (reportInitialized || mode !== 'edit') {
            return;
        }
        reportInitialized = true;
        var yearSelect = document.getElementById('reportYear');
        var currentYear = new Date().getFullYear();
        var currentMonth = new Date().getMonth() + 1;
        for (var year = currentYear; year >= currentYear - 5; year -= 1) {
            var option = document.createElement('option');
            option.value = year;
            option.textContent = year;
            if (year === currentYear) {
                option.selected = true;
            }
            yearSelect.appendChild(option);
        }
        document.getElementById('reportMonth').value = String(currentMonth);

        Array.from(document.querySelectorAll('.report-btn')).forEach(function (button) {
            button.addEventListener('click', function () {
                var endpoint = reportEndpoint(button.getAttribute('data-report-type'));
                var msgEl = document.getElementById('reportMessage');
                var trackingEl = document.getElementById('reportTracking');
                var trackBillIdEl = document.getElementById('trackBillId');
                var trackWageIdEl = document.getElementById('trackWageId');
                msgEl.classList.add('d-none');
                trackingEl.classList.add('d-none');
                button.disabled = true;

                var trackingPromise = button.getAttribute('data-report-type') === 'full'
                    ? window.AppApi.post('MonthlyDocuments/generate-all', { siteId: siteId, year: Number(document.getElementById('reportYear').value), month: Number(document.getElementById('reportMonth').value) }).then(function (data) {
                        trackBillIdEl.textContent = data.billId || '-';
                        trackWageIdEl.textContent = data.wageId || '-';
                        trackingEl.classList.remove('d-none');
                    })
                    : Promise.resolve();

                trackingPromise.then(function () {
                    return window.AppApi.client.get(endpoint, {
                        params: {
                            siteId: siteId,
                            year: document.getElementById('reportYear').value,
                            month: document.getElementById('reportMonth').value,
                            format: document.getElementById('reportFormat').value
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
            });
        });
    }

    function collectPayload() {
        return {
            id: form.elements.Id ? form.elements.Id.value : undefined,
            siteCode: form.elements.SiteCode.value,
            siteName: form.elements.SiteName.value,
            clientId: form.elements.ClientId ? (form.elements.ClientId.value || null) : undefined,
            branchId: form.elements.BranchId.value || null,
            clientName: form.elements.ClientName.value,
            address: form.elements.Address.value,
            city: form.elements.City.value,
            state: form.elements.State.value,
            pinCode: form.elements.PinCode.value,
            contactPerson: form.elements.ContactPerson.value || null,
            contactPhone: form.elements.ContactPhone.value || null,
            contactEmail: form.elements.ContactEmail.value || null,
            emergencyContactName: form.elements.EmergencyContactName.value || null,
            emergencyContactPhone: form.elements.EmergencyContactPhone.value || null,
            musterPoint: form.elements.MusterPoint.value || null,
            accessZoneNotes: form.elements.AccessZoneNotes.value || null,
            siteInstructionBook: form.elements.SiteInstructionBook.value || null,
            geofenceExceptionNotes: form.elements.GeofenceExceptionNotes.value || null,
            isActive: !!form.elements.IsActive.checked,
            latitude: form.elements.Latitude.value ? Number(form.elements.Latitude.value) : null,
            longitude: form.elements.Longitude.value ? Number(form.elements.Longitude.value) : null,
            geofenceRadiusMeters: form.elements.GeofenceRadiusMeters.value ? Number(form.elements.GeofenceRadiusMeters.value) : null,
            supervisorIds: selectedSupervisorIds,
            posts: collectPosts(),
            activeDeploymentPlan: collectDeploymentPlan()
        };
    }

    function loadDependencies(data) {
        var promises = [
            window.AppApi.get('Branches', { includeInactive: 'false' }).then(function (response) {
                populateSelect(document.getElementById('siteBranchId'), (response.items || []).sort(function (a, b) {
                    return String(a.branchName || '').localeCompare(String(b.branchName || ''));
                }), '-- Select Branch --', function (branch) { return branch.branchName || ''; });
            }),
            window.AppApi.get('Supervisors', { pageSize: '500', isActive: 'true' }).then(function (response) {
                buildSupervisorMenu(response.items || []);
            })
        ];

        if (mode === 'edit') {
            promises.push(window.AppApi.get('Clients', { includeInactive: 'false', pageSize: '1000', siteId: siteId }).then(function (response) {
                populateSelect(document.getElementById('siteClientIdDropdown'), response.items || [], '-- Select Client --', function (client) {
                    return client.companyName || '';
                });
                if (data && data.clientId) {
                    document.getElementById('siteClientIdDropdown').value = data.clientId;
                }
            }));
        }

        return Promise.all(promises);
    }

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        clearError();
        var submitButton = document.getElementById('siteFormSubmitButton');
        submitButton.disabled = true;

        var payload = collectPayload();
        var request = mode === 'edit'
            ? window.AppApi.put(page.getAttribute('data-submit-endpoint'), payload)
            : window.AppApi.post(page.getAttribute('data-submit-endpoint'), payload);

        request.then(function () {
            if (window.AppUi) {
                window.AppUi.showToast({
                    type: 'success',
                    title: mode === 'edit' ? 'Updated' : 'Created',
                    message: mode === 'edit' ? 'Site updated successfully.' : 'Site created successfully.'
                });
            }
            window.setTimeout(function () {
                window.location.href = page.getAttribute('data-redirect-url');
            }, 300);
        }).catch(function (error) {
            showError(error && error.message ? error.message : 'Unable to save site.');
        }).finally(function () {
            submitButton.disabled = false;
        });
    });

    if (mode === 'edit') {
        window.AppApi.get(page.getAttribute('data-load-endpoint')).then(function (data) {
            fillForm(data);
            return loadDependencies(data).then(function () {
                return Promise.all([loadCurrentRate(), loadRateHistory()]);
            });
        }).catch(function (error) {
            showError(error && error.message ? error.message : 'Unable to load site data.');
        });
    } else {
        initPostsEditor([]);
        loadDependencies().catch(function (error) {
            showError(error && error.message ? error.message : 'Unable to load site form data.');
        });
    }

    initReportModal();

    if (mode === 'edit') {
        document.getElementById('saveSiteRateButton').addEventListener('click', saveRate);
        document.addEventListener('click', function (event) {
            var editButton = event.target.closest('.rate-plan-edit');
            if (editButton) {
                var rate = JSON.parse(editButton.getAttribute('data-rate-plan'));
                currentRatePlanId = rate.id || '';
                document.getElementById('rateAmount').value = rate.rateAmount || '';
                document.getElementById('rateEffectiveFrom').value = rate.effectiveFrom ? String(rate.effectiveFrom).slice(0, 10) : '';
                document.getElementById('rateEpfPercent').value = rate.epfPercent == null ? '' : rate.epfPercent;
                document.getElementById('rateEsicPercent').value = rate.esicPercent == null ? '' : rate.esicPercent;
                document.getElementById('rateAllowancePercent').value = rate.allowancePercent == null ? '' : rate.allowancePercent;
                document.getElementById('rateEpfWageCap').value = rate.epfWageCap == null ? '' : rate.epfWageCap;
                document.getElementById('siteClientIdDropdown').value = rate.clientId || '';
                setRateValidation('');
                return;
            }

            var deleteButton = event.target.closest('.rate-plan-delete');
            if (!deleteButton) {
                return;
            }

            var confirmPromise = window.AppUi && typeof window.AppUi.confirm === 'function'
                ? window.AppUi.confirm({ message: 'Deactivate this rate plan?', confirmText: 'Delete' })
                : Promise.resolve(window.confirm('Deactivate this rate plan?'));

            confirmPromise.then(function (confirmed) {
                if (!confirmed) {
                    return;
                }
                window.AppApi.delete('SiteRates/' + deleteButton.getAttribute('data-rate-plan-id')).then(function () {
                    if (window.AppUi) {
                        window.AppUi.showToast({ type: 'success', title: 'Deleted', message: 'Rate plan deleted.' });
                    }
                    currentRatePlanId = '';
                    return Promise.all([loadCurrentRate(), loadRateHistory()]);
                });
            });
        });
    }
})(window, document);
