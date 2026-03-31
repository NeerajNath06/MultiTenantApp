(function (window, document) {
    'use strict';

    var root = document.getElementById('dashboardPage');
    if (!root || !window.AppApi) {
        return;
    }

    function setField(name, value) {
        var element = root.querySelector('[data-dashboard-field="' + name + '"]');
        if (element) {
            element.textContent = value == null ? '0' : String(value);
        }
    }

    function formatDate(value) {
        if (!value) {
            return '-';
        }

        var date = new Date(value);
        return isNaN(date.getTime()) ? value : date.toLocaleString();
    }

    function renderActivities(items) {
        var container = document.getElementById('dashboardRecentActivities');
        if (!container) {
            return;
        }

        if (!items || !items.length) {
            container.innerHTML = '<div class="list-group-item border-0 py-4 px-4 text-muted">No recent activity available.</div>';
            return;
        }

        container.innerHTML = items.map(function (activity) {
            return [
                '<div class="list-group-item border-0 border-bottom py-3 px-4">',
                '<div class="d-flex w-100 justify-content-between align-items-start gap-2">',
                '<div class="flex-grow-1 min-width-0">',
                '<h6 class="mb-1 fw-semibold">', activity.description || '-', '</h6>',
                '<small class="text-muted"><i class="fas fa-user me-1"></i>', activity.userName || '-', '</small>',
                '</div>',
                '<small class="text-muted text-nowrap"><i class="fas fa-clock me-1"></i>', formatDate(activity.activityDate), '</small>',
                '</div>',
                '</div>'
            ].join('');
        }).join('');
    }

    window.AppApi.get(root.getAttribute('data-endpoint'))
        .then(function (data) {
            setField('totalUsers', data.totalUsers);
            setField('activeUsers', data.activeUsers);
            setField('totalSecurityGuards', data.totalSecurityGuards);
            setField('activeGuards', data.activeGuards);
            setField('totalDepartments', data.totalDepartments);
            setField('totalRoles', data.totalRoles);
            setField('totalMenus', data.totalMenus);
            setField('totalFormSubmissions', data.totalFormSubmissions);
            setField('pendingFormSubmissions', data.pendingFormSubmissions);
            renderActivities(data.recentActivities || []);
        })
        .catch(function () {
            renderActivities([]);
        });
})(window, document);
