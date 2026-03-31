(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('leaveRequestsIndexPage');
    if (!page) {
        return;
    }

    function setStat(name, value) {
        var element = page.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    var listPage = window.AppCrud.createListPage({
        pageId: 'leaveRequestsIndexPage',
        tableWrapperId: 'leaveRequestsTableWrapper',
        recordCountId: 'leaveRequestsRecordCount',
        paginationContainerId: 'leaveRequestsPaginationContainer',
        paginationSummaryId: 'leaveRequestsPaginationSummary',
        paginationId: 'leaveRequestsPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'LeaveRequests',
        queryDefaults: {
            pageNumber: '1',
            pageSize: '10',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var approved = 0;
            var pending = 0;

            (items || []).forEach(function (item) {
                if (item.status === 'Approved') {
                    approved += 1;
                }
                if (item.status === 'Pending') {
                    pending += 1;
                }
            });

            setStat('total', data.totalCount || items.length);
            setStat('approved', approved);
            setStat('pending', pending);
        },
        renderEmptyState: function () {
            return [
                '<div class="text-center py-5">',
                '<i class="fas fa-calendar-times fa-3x text-muted mb-3"></i>',
                '<h5 class="text-muted">No leave requests found</h5>',
                '<a href="', page.getAttribute('data-create-url'), '" class="btn btn-primary mt-3">',
                '<i class="fas fa-calendar-times me-2"></i>New Leave Request',
                '</a>',
                '</div>'
            ].join('');
        },
        renderTable: function (context) {
            var detailsBase = page.getAttribute('data-details-base');

            function statusBadge(status) {
                var className = status === 'Approved'
                    ? 'success'
                    : status === 'Rejected' || status === 'Cancelled'
                        ? 'danger'
                        : 'warning';
                return '<span class="badge bg-' + className + '">' + context.escapeHtml(status || '-') + '</span>';
            }

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>Guard</th>',
                '<th>Leave Type</th>',
                '<th>Start Date</th>',
                '<th>End Date</th>',
                '<th>Days</th>',
                '<th>Reason</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (leave) {
                    var actionButtons = [
                        '<a href="' + context.replaceToken(detailsBase, '__id__', leave.id) + '" class="btn btn-outline-primary" title="View"><i class="fas fa-eye"></i></a>'
                    ];

                    if (leave.status === 'Pending') {
                        actionButtons.push('<button type="button" class="btn btn-success" title="Approve" data-leave-approve-id="' + leave.id + '" data-leave-approve-value="true"><i class="fas fa-check"></i></button>');
                        actionButtons.push('<button type="button" class="btn btn-danger" title="Reject" data-leave-approve-id="' + leave.id + '" data-leave-approve-value="false"><i class="fas fa-times"></i></button>');
                    }

                    actionButtons.push('<button type="button" class="btn btn-outline-danger" title="Delete" data-crud-delete-id="' + leave.id + '"><i class="fas fa-trash"></i></button>');

                    return [
                        '<tr>',
                        '<td><strong>', context.escapeHtml(leave.guardCode), '</strong><br><small class="text-muted">', context.escapeHtml(leave.guardName), '</small></td>',
                        '<td>', context.escapeHtml(leave.leaveType), '</td>',
                        '<td>', context.formatDate(leave.startDate), '</td>',
                        '<td>', context.formatDate(leave.endDate), '</td>',
                        '<td><strong>', context.escapeHtml(leave.totalDays), '</strong></td>',
                        '<td>', context.escapeHtml(leave.reason), '</td>',
                        '<td>', statusBadge(leave.status), '</td>',
                        '<td class="text-end"><div class="btn-group btn-group-sm">', actionButtons.join(''), '</div></td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        getDeleteMessage: function () {
            return 'Are you sure you want to delete (cancel) this leave request?';
        },
        deleteSuccessMessage: 'Leave request deleted successfully.',
        loadErrorMessage: 'Unable to load leave requests right now.'
    });

    page.addEventListener('click', function (event) {
        var actionButton = event.target.closest('[data-leave-approve-id]');
        if (!actionButton) {
            return;
        }

        var id = actionButton.getAttribute('data-leave-approve-id');
        var isApproved = actionButton.getAttribute('data-leave-approve-value') === 'true';
        var message = isApproved
            ? 'Are you sure you want to approve this leave request?'
            : 'Are you sure you want to reject this leave request?';

        var confirmPromise = window.AppUi && typeof window.AppUi.confirm === 'function'
            ? window.AppUi.confirm({ message: message, confirmText: isApproved ? 'Approve' : 'Reject' })
            : Promise.resolve(window.confirm(message));

        confirmPromise.then(function (confirmed) {
            if (!confirmed) {
                return;
            }

            return window.AppApi.post('LeaveRequests/' + id + '/approve', {
                leaveRequestId: id,
                isApproved: isApproved,
                rejectionReason: null
            }).then(function () {
                if (window.AppUi) {
                    window.AppUi.showToast({
                        type: 'success',
                        title: isApproved ? 'Approved' : 'Rejected',
                        message: isApproved ? 'Leave request approved.' : 'Leave request rejected.'
                    });
                }

                if (listPage && typeof listPage.reload === 'function') {
                    listPage.reload();
                }
            });
        });
    });
})(window, document);
