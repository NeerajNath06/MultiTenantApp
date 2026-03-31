(function (window, document) {
    'use strict';

    if (!window.AppCrud) {
        return;
    }

    var page = document.getElementById('menusIndexPage');
    var stats = document.getElementById('menusStats');
    var showingSummary = document.getElementById('menusShowingSummary');
    var pageSizeSelect = document.getElementById('menusPageSize');

    function setStat(name, value) {
        if (!stats) {
            return;
        }

        var element = stats.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function sortLink(context, column, label) {
        var nextDirection = context.query.sortBy === column && context.query.sortDirection === 'asc' ? 'desc' : 'asc';
        var query = Object.assign({}, context.query, {
            pageNumber: '1',
            sortBy: column,
            sortDirection: nextDirection
        });

        var icon = '<i class="fas fa-sort text-muted"></i>';
        if (context.query.sortBy === column) {
            icon = '<i class="fas fa-sort-' + (context.query.sortDirection === 'asc' ? 'up' : 'down') + '"></i>';
        }

        return '<a href="' + context.indexUrl + '?' + context.buildQueryString(query) + '" class="text-decoration-none text-dark">' + label + ' ' + icon + '</a>';
    }

    window.AppCrud.createListPage({
        pageId: 'menusIndexPage',
        filterFormId: 'menusFilterForm',
        tableWrapperId: 'menusTableWrapper',
        recordCountId: 'menusRecordCount',
        paginationContainerId: 'menusPaginationContainer',
        paginationSummaryId: 'menusPaginationSummary',
        paginationId: 'menusPagination',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'Menus',
        queryDefaults: {
            pageNumber: '1',
            pageSize: pageSizeSelect ? pageSizeSelect.value : '10',
            search: '',
            sortBy: '',
            sortDirection: 'asc',
            includeInactive: 'false'
        },
        updateStats: function (data, items) {
            var activeCount = 0;
            var totalSubmenus = 0;

            (items || []).forEach(function (item) {
                if (item.isActive) {
                    activeCount += 1;
                }
                totalSubmenus += (item.subMenus && item.subMenus.length) || 0;
            });

            setStat('total', data.totalCount || 0);
            setStat('active', activeCount);
            setStat('submenus', totalSubmenus);

            if (showingSummary) {
                if (!items.length) {
                    showingSummary.textContent = 'No menus to display';
                } else {
                    var start = ((data.pageNumber - 1) * data.pageSize) + 1;
                    var end = Math.min(data.pageNumber * data.pageSize, data.totalCount);
                    showingSummary.textContent = 'Showing ' + start + ' to ' + end + ' of ' + data.totalCount + ' menus';
                }
            }
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-list fa-3x text-muted mb-3"></i><h5 class="text-muted">No menus found</h5><p class="text-muted">Get started by creating your first menu.</p><a href="/Menus/Create" class="btn btn-primary mt-3"><i class="fas fa-list me-2"></i>Add New Menu</a></div>';
        },
        renderTable: function (context) {
            var submenuCreateBase = context.page.getAttribute('data-submenu-create-base');
            var submenuIndexBase = context.page.getAttribute('data-submenu-index-base');

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>', sortLink(context, 'name', 'Name'), '</th>',
                '<th>Icon</th>',
                '<th>Route</th>',
                '<th>', sortLink(context, 'order', 'Order'), '</th>',
                '<th>Sub Menus</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (menu) {
                    var displayTitle = menu.displayName || menu.name;
                    var hasDifferentName = menu.name && menu.name !== menu.displayName;

                    return [
                        '<tr>',
                        '<td><div class="fw-semibold">', context.escapeHtml(displayTitle), '</div>', hasDifferentName ? '<small class="text-muted">' + context.escapeHtml(menu.name) + '</small>' : '', '</td>',
                        '<td>', menu.icon ? '<i class="' + context.escapeHtml(menu.icon) + ' fa-lg"></i>' : '<span class="text-muted">-</span>', '</td>',
                        '<td>', context.escapeHtml(menu.route || '-'), '</td>',
                        '<td><span class="badge bg-secondary">', context.escapeHtml(menu.displayOrder), '</span></td>',
                        '<td><span class="badge bg-info"><i class="fas fa-list-ul me-1"></i>', context.escapeHtml((menu.subMenus && menu.subMenus.length) || 0), '</span></td>',
                        '<td>', menu.isActive ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Active</span>' : '<span class="badge bg-danger"><i class="fas fa-times-circle me-1"></i>Inactive</span>', '</td>',
                        '<td class="text-end"><div class="btn-group" role="group"><a href="', context.replaceToken(submenuCreateBase, '__id__', menu.id), '" class="btn btn-sm btn-outline-success" title="Add SubMenu"><i class="fas fa-plus"></i></a><a href="', context.replaceToken(submenuIndexBase, '__id__', menu.id), '" class="btn btn-sm btn-outline-info" title="View SubMenus"><i class="fas fa-list-ul"></i></a></div></td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        loadErrorMessage: 'Unable to load menus right now.'
    });

    if (pageSizeSelect) {
        pageSizeSelect.addEventListener('change', function () {
            var url = new URL(window.location.href);
            url.searchParams.set('pageSize', pageSizeSelect.value);
            url.searchParams.set('pageNumber', '1');
            window.location.href = url.toString();
        });
    }
})(window, document);
