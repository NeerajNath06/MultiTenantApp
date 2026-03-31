(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'siteDetailsPage',
        render: function (page, data, helpers) {
            helpers.setText('siteName', data.siteName);
            helpers.setText('siteCode', data.siteCode);
            helpers.setText('siteCodeText', data.siteCode);
            helpers.setText('clientName', data.clientName);
            helpers.setText('branchName', data.branchName);
            helpers.setText('address', data.address);
            helpers.setText('city', data.city);
            helpers.setText('state', data.state);
            helpers.setText('pinCode', data.pinCode);
            helpers.setText('contactPerson', data.contactPerson);
            helpers.setText('contactPhone', data.contactPhone);
            helpers.setText('contactEmail', data.contactEmail);
            helpers.setText('supervisorCount', data.supervisorIds ? data.supervisorIds.length : 0);
            helpers.setText('emergencyContact', data.emergencyContactName ? (data.emergencyContactName + ' (' + (data.emergencyContactPhone || '-') + ')') : '-');
            helpers.setText('musterPoint', data.musterPoint);
            helpers.setText('accessZoneNotes', data.accessZoneNotes);
            helpers.setText('siteInstructionBook', data.siteInstructionBook);
            helpers.setText('geofenceExceptionNotes', data.geofenceExceptionNotes);
            helpers.setText('latitude', data.latitude);
            helpers.setText('longitude', data.longitude);
            helpers.setText('geofenceRadiusMeters', data.geofenceRadiusMeters);
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));
            helpers.setText('modifiedDate', data.modifiedDate ? helpers.formatDate(data.modifiedDate) : '-');
            helpers.setText('planInstructionSummary', data.activeDeploymentPlan && data.activeDeploymentPlan.instructionSummary ? data.activeDeploymentPlan.instructionSummary : '-');
            helpers.setText('planWindow', data.activeDeploymentPlan ? (new Date(data.activeDeploymentPlan.effectiveFrom).toLocaleDateString() + ' - ' + (data.activeDeploymentPlan.effectiveTo ? new Date(data.activeDeploymentPlan.effectiveTo).toLocaleDateString() : 'Open')) : '-');
            helpers.setText('reserveZones', data.activeDeploymentPlan ? ((data.activeDeploymentPlan.reservePoolMapping || '-') + ' | ' + (data.activeDeploymentPlan.accessZones || '-')) : '-');

            var statusBadge = page.querySelector('[data-field="statusBadge"]');
            if (statusBadge) {
                statusBadge.textContent = data.isActive ? 'Active' : 'Inactive';
                statusBadge.className = 'badge ' + (data.isActive ? 'bg-success' : 'bg-danger');
            }

            var postsList = document.getElementById('sitePostsList');
            if (postsList) {
                if (!data.posts || !data.posts.length) {
                    postsList.textContent = '-';
                } else {
                    postsList.innerHTML = '<ul class="mb-0 ps-3">' + data.posts.map(function (post) {
                        return '<li>' + window.AppCrud.escapeHtml((post.postCode || '-') + ' - ' + (post.postName || '-') + ' (' + (post.sanctionedStrength || 0) + ')') + '</li>';
                    }).join('') + '</ul>';
                }
            }
        }
    });
})(window, document);
