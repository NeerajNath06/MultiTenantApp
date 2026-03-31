(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var genders = ['Male', 'Female', 'Other'];

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

    function populateGenderSelect(select) {
        if (!select) {
            return;
        }

        var currentValue = select.value;
        select.innerHTML = '<option value="">-- Select --</option>' + genders.map(function (gender) {
            return '<option value="' + gender + '">' + gender + '</option>';
        }).join('');
        if (currentValue) {
            select.value = currentValue;
        }
    }

    function toggleLoginFields() {
        var checkbox = document.getElementById('createLogin');
        var container = document.getElementById('loginFields');
        if (!checkbox || !container) {
            return;
        }

        container.classList.toggle('d-none', !checkbox.checked);
    }

    function formatDate(value) {
        return value ? String(value).slice(0, 10) : '';
    }

    window.AppCrud.createFormPage({
        pageId: 'securityGuardFormPage',
        formId: 'securityGuardForm',
        errorSummaryId: 'securityGuardFormErrors',
        submitButtonId: 'securityGuardFormSubmitButton',
        createSuccessMessage: 'Security guard created successfully.',
        updateSuccessMessage: 'Security guard updated successfully.',
        saveErrorMessage: 'Unable to save guard.',
        loadErrorMessage: 'Unable to load guard data.',
        initialize: function () {
            populateGenderSelect(document.getElementById('securityGuardGender'));
            toggleLoginFields();

            var createLogin = document.getElementById('createLogin');
            if (createLogin) {
                createLogin.addEventListener('change', toggleLoginFields);
            }

            return window.AppApi.get('Supervisors', { pageSize: '500', isActive: 'true' }).then(function (data) {
                populateSelect(document.getElementById('securityGuardSupervisorId'), data.items || [], '-- No Supervisor --', function (item) {
                    var fullName = ((item.firstName || '') + ' ' + (item.lastName || '')).trim();
                    return fullName || item.userName || '';
                });
            });
        },
        mapResponseToForm: function (form, data) {
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
                    return;
                }
                if (element.type === 'date') {
                    element.value = formatDate(value);
                    return;
                }

                element.value = value == null ? '' : value;
            });
        },
        transformPayload: function (payload, form, mode) {
            if (!payload.email) {
                payload.email = null;
            }
            if (!payload.aadharNumber) {
                payload.aadharNumber = null;
            }
            payload.panNumber = payload.pANNumber || null;
            delete payload.pANNumber;
            if (!payload.supervisorId) {
                payload.supervisorId = null;
            }
            if (!payload.emergencyContactName) {
                payload.emergencyContactName = null;
            }
            if (!payload.emergencyContactPhone) {
                payload.emergencyContactPhone = null;
            }

            if (mode === 'create') {
                if (!payload.loginUserName) {
                    payload.loginUserName = null;
                }
                if (!payload.loginPassword) {
                    payload.loginPassword = null;
                }
            }

            return payload;
        }
    });
})(window, document);
