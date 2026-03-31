(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    window.AppCrud.createDetailsPage({
        pageId: 'leaveRequestDetailsPage',
        deleteButtonId: 'leaveRequestDeleteButton',
        getDeleteEndpoint: function (page) {
            return 'LeaveRequests/' + page.getAttribute('data-leave-request-id');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete (cancel) this leave request?';
        },
        deleteSuccessMessage: 'Leave request deleted successfully.',
        render: function (page, data, helpers) {
            var statusBadge = page.querySelector('[data-field="statusBadge"]');

            helpers.setText('guardCode', data.guardCode);
            helpers.setText('guardName', data.guardName);
            helpers.setText('leaveType', data.leaveType);
            helpers.setText('startDate', helpers.formatDate(data.startDate));
            helpers.setText('endDate', helpers.formatDate(data.endDate));
            helpers.setText('totalDays', data.totalDays);
            helpers.setText('reason', data.reason);
            helpers.setText('notes', data.notes);
            helpers.setText('approvedDate', data.approvedDate ? helpers.formatDate(data.approvedDate) : '-');
            helpers.setText('approvedByName', data.approvedByName);
            helpers.setText('rejectionReason', data.rejectionReason);
            helpers.setText('createdDate', helpers.formatDate(data.createdDate));

            if (statusBadge) {
                statusBadge.textContent = data.status || '-';
                statusBadge.className = 'badge ' + (
                    data.status === 'Approved'
                        ? 'bg-success'
                        : data.status === 'Rejected' || data.status === 'Cancelled'
                            ? 'bg-danger'
                            : 'bg-warning'
                );
            }
        }
    });
})(window, document);
