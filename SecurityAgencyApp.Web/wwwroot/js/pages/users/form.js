(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var availableRoles = [];

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

    function renderRoles(selectedIds) {
        var container = document.getElementById('userRolesContainer');
        if (!container) {
            return;
        }

        var selectedLookup = {};
        (selectedIds || []).forEach(function (id) {
            selectedLookup[String(id)] = true;
        });

        if (!availableRoles.length) {
            container.innerHTML = '<span class="text-muted small">No roles available. Create roles from <a href="/Roles">Roles</a> first.</span>';
            return;
        }

        container.innerHTML = availableRoles.map(function (role) {
            var roleId = String(role.id);
            return [
                '<div class="form-check">',
                '<input class="form-check-input" type="checkbox" data-role-checkbox="true" value="' + roleId + '" id="user_role_' + roleId + '"' + (selectedLookup[roleId] ? ' checked' : '') + ' />',
                '<label class="form-check-label" for="user_role_' + roleId + '">' + window.AppCrud.escapeHtml((role.name || '') + ' (' + (role.code || '-') + ')') + '</label>',
                '</div>'
            ].join('');
        }).join('');
    }

    function getSelectedRoleIds() {
        return Array.from(document.querySelectorAll('[data-role-checkbox="true"]:checked')).map(function (checkbox) {
            return checkbox.value;
        });
    }

    window.AppCrud.createFormPage({
        pageId: 'userFormPage',
        formId: 'userForm',
        errorSummaryId: 'userFormErrors',
        submitButtonId: 'userFormSubmitButton',
        createSuccessMessage: 'User created successfully.',
        updateSuccessMessage: 'User updated successfully.',
        saveErrorMessage: 'Unable to save user.',
        loadErrorMessage: 'Unable to load user form data.',
        initialize: function () {
            return Promise.all([
                window.AppApi.get('Departments', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('Designations', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('Roles', { includeInactive: 'false', pageSize: '500' })
            ]).then(function (results) {
                populateSelect(document.getElementById('userDepartmentId'), results[0].items || [], '-- Select Department --', function (item) {
                    return item.name || '';
                });
                populateSelect(document.getElementById('userDesignationId'), results[1].items || [], '-- Select Designation --', function (item) {
                    return item.name || '';
                });
                availableRoles = results[2].items || [];
                renderRoles([]);
            });
        },
        loadEntity: function (form, page) {
            var endpoint = page.getAttribute('data-load-endpoint');
            return window.AppApi.get(endpoint).then(function (data) {
                var fieldMap = {
                    Id: data.id,
                    FirstName: data.firstName,
                    LastName: data.lastName,
                    Email: data.email,
                    PhoneNumber: data.phoneNumber,
                    AadharNumber: data.aadharNumber,
                    PANNumber: data.panNumber,
                    UAN: data.uan,
                    DepartmentId: data.departmentId,
                    DesignationId: data.designationId
                };

                Object.keys(fieldMap).forEach(function (name) {
                    var element = form.elements[name];
                    if (element) {
                        element.value = fieldMap[name] == null ? '' : fieldMap[name];
                    }
                });

                var activeCheckbox = form.elements.IsActive;
                if (activeCheckbox) {
                    activeCheckbox.checked = !!data.isActive;
                }

                renderRoles(data.roleIds || []);
            });
        },
        transformPayload: function (payload, form, mode) {
            payload.roleIds = getSelectedRoleIds();
            payload.panNumber = payload.pANNumber || null;
            payload.uan = payload.uAN || null;
            delete payload.pANNumber;
            delete payload.uAN;

            if (!payload.phoneNumber) {
                payload.phoneNumber = null;
            }
            if (!payload.aadharNumber) {
                payload.aadharNumber = null;
            }
            if (!payload.departmentId) {
                payload.departmentId = null;
            }
            if (!payload.designationId) {
                payload.designationId = null;
            }

            if (mode === 'edit') {
                delete payload.userName;
                delete payload.password;
            }

            return payload;
        }
    });
})(window, document);
