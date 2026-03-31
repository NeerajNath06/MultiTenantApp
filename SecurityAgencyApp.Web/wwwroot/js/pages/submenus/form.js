(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    window.AppCrud.createFormPage({
        pageId: 'subMenuFormPage',
        formId: 'subMenuForm',
        errorSummaryId: 'subMenuFormErrors',
        submitButtonId: 'subMenuFormSubmitButton',
        createSuccessMessage: 'SubMenu created successfully.',
        updateSuccessMessage: 'SubMenu updated successfully.',
        saveErrorMessage: 'Unable to save submenu.',
        loadErrorMessage: 'Unable to load submenu data.',
        loadEntity: function (form, page) {
            var id = document.getElementById('subMenuId');
            if (!id || !id.value) {
                return Promise.resolve();
            }

            return window.AppApi.get('SubMenus', {
                includeInactive: 'true'
            }).then(function (data) {
                var item = (data.items || []).find(function (entry) {
                    return String(entry.id) === String(id.value);
                });

                if (!item) {
                    throw new Error('Submenu not found.');
                }

                var fields = {
                    Id: item.id,
                    MenuId: item.menuId,
                    Name: item.name,
                    DisplayName: item.displayName,
                    Icon: item.icon,
                    Route: item.route,
                    DisplayOrder: item.displayOrder,
                    IsActive: item.isActive
                };

                Object.keys(fields).forEach(function (key) {
                    var input = form.elements.namedItem(key);
                    if (!input) {
                        return;
                    }

                    if (input.type === 'checkbox') {
                        input.checked = !!fields[key];
                    } else {
                        input.value = fields[key] == null ? '' : fields[key];
                    }
                });

                page.setAttribute('data-redirect-url', '/SubMenus?menuId=' + item.menuId);
            });
        }
    });
})(window, document);
