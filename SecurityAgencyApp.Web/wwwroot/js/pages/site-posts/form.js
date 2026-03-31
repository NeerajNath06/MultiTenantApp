(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
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

    window.AppCrud.createFormPage({
        pageId: 'sitePostFormPage',
        formId: 'sitePostForm',
        errorSummaryId: 'sitePostFormErrors',
        submitButtonId: 'sitePostFormSubmitButton',
        createSuccessMessage: 'Site post created successfully.',
        updateSuccessMessage: 'Site post updated successfully.',
        saveErrorMessage: 'Unable to save site post.',
        loadErrorMessage: 'Unable to load site post data.',
        initialize: function () {
            return Promise.all([
                window.AppApi.get('Sites', { includeInactive: 'false', pageSize: '1000' }),
                window.AppApi.get('Branches', { includeInactive: 'false' })
            ]).then(function (results) {
                populateSelect(document.getElementById('sitePostSiteId'), results[0].items || [], '-- Select Site --', function (item) { return item.siteName || ''; });
                populateSelect(document.getElementById('sitePostBranchId'), results[1].items || [], '-- Select Branch --', function (item) { return item.branchName || ''; });
            });
        },
        transformPayload: function (payload) {
            if (!payload.branchId) {
                payload.branchId = null;
            }
            if (!payload.shiftName) {
                payload.shiftName = null;
            }
            if (!payload.genderRequirement) {
                payload.genderRequirement = null;
            }
            if (!payload.skillRequirement) {
                payload.skillRequirement = null;
            }
            if (!payload.weeklyOffPattern) {
                payload.weeklyOffPattern = null;
            }
            return payload;
        }
    });
})(window, document);
