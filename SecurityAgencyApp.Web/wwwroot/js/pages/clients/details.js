(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'clientDetailsPage',
        deleteButtonId: 'clientDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'Clients/' + page.getAttribute('data-client-id');
        },
        getDeleteMessage: function (page) {
            var companyName = page.querySelector('[data-client-field="companyName"]').textContent || 'this client';
            return 'Are you sure you want to delete "' + companyName + '"?';
        },
        deleteSuccessMessage: 'Client deleted successfully.',
        render: function (page, data, helpers) {
            function setField(name, value) {
                helpers.setText(name, value);
            }

            var statusBadge = page.querySelector('[data-client-field="statusBadge"]');
            var activeBadge = page.querySelector('[data-client-field="activeBadge"]');
            var websiteLink = page.querySelector('[data-client-field="websiteLink"]');
            var websiteFallback = page.querySelector('[data-client-field="websiteFallback"]');

            setField('clientCode', data.clientCode);
            setField('companyName', data.companyName);
            setField('contactPerson', data.contactPerson);
            setField('email', data.email);
            setField('phoneNumber', data.phoneNumber);
            setField('alternatePhone', data.alternatePhone);
            setField('address', data.address);
            setField('city', data.city);
            setField('state', data.state);
            setField('pinCode', data.pinCode);
            setField('gstNumber', data.gstNumber);
            setField('panNumber', data.panNumber);
            setField('accountManagerName', data.accountManagerName);
            setField('notes', data.notes);
            setField('billingAddress', data.billingAddress);
            setField('billingCity', data.billingCity);
            setField('billingState', data.billingState);
            setField('billingPinCode', data.billingPinCode);
            setField('billingContactName', data.billingContactName);
            setField('billingContactEmail', data.billingContactEmail);
            setField('escalationContactName', data.escalationContactName);
            setField('escalationContactEmail', data.escalationContactEmail);
            setField('escalationTatHours', data.escalationTatHours);
            setField('creditPeriodDays', data.creditPeriodDays);
            setField('billingCycle', data.billingCycle);
            setField('gstState', data.gstState);
            setField('paymentModePreference', data.paymentModePreference);
            setField('invoicePrefix', data.invoicePrefix);
            setField('slaTerms', data.slaTerms);
            setField('taxTreatment', data.taxTreatment);
            setField('penaltyTerms', data.penaltyTerms);
            setField('createdDate', helpers.formatDate(data.createdDate));
            setField('modifiedDate', data.modifiedDate ? helpers.formatDate(data.modifiedDate) : '-');

            if (statusBadge) {
                statusBadge.textContent = data.status || '-';
                statusBadge.className = 'badge ' + (data.status === 'Active' ? 'bg-success' : data.status === 'Suspended' ? 'bg-warning' : 'bg-secondary');
            }

            if (activeBadge) {
                activeBadge.textContent = data.isActive ? 'Active' : 'Inactive';
                activeBadge.className = 'badge ' + (data.isActive ? 'bg-success' : 'bg-danger');
            }

            if (websiteLink && websiteFallback) {
                if (data.website) {
                    websiteLink.href = data.website;
                    websiteLink.textContent = data.website;
                    websiteLink.classList.remove('d-none');
                    websiteFallback.classList.add('d-none');
                } else {
                    websiteLink.classList.add('d-none');
                    websiteFallback.classList.remove('d-none');
                }
            }
        }
    });
})(window, document);
