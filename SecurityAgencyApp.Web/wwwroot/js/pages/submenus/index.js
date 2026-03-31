(function (window, document) {
    'use strict';

    if (!window.AppCrud || !window.AppApi) {
        return;
    }

    var page = document.getElementById('subMenusIndexPage');
    var stats = document.getElementById('subMenusStats');
    var menuFilter = document.getElementById('menuFilter');
    var createButton = document.getElementById('subMenusCreateButton');

    function setStat(name, value) {
        if (!stats) {
            return;
        }

        var element = stats.querySelector('[data-stat="' + name + '"]');
        if (element) {
            element.textContent = String(value || 0);
        }
    }

    function loadMenus() {
        return window.AppApi.get('Menus', {
            includeInactive: 'false',
            pageSize: '100'
        }).then(function (data) {
            var selectedId = page.getAttribute('data-selected-menu-id') || '';
            menuFilter.innerHTML = '<option value="">All Menus</option>' + (data.items || []).map(function (menu) {
                var title = menu.displayName || menu.name;
                var selected = String(menu.id) === String(selectedId) ? ' selected' : '';
                return '<option value="' + menu.id + '"' + selected + '>' + window.AppCrud.escapeHtml(title) + '</option>';
            }).join('');
        });
    }

    function updateCreateButton() {
        if (!createButton) {
            return;
        }

        var selectedId = menuFilter ? menuFilter.value : '';
        if (selectedId) {
            createButton.classList.remove('d-none');
            createButton.href = page.getAttribute('data-create-base').replace('__id__', selectedId);
        } else {
            createButton.classList.add('d-none');
        }
    }

    loadMenus().then(updateCreateButton);

    if (menuFilter) {
        menuFilter.addEventListener('change', function () {
            var url = new URL(window.location.href);
            if (menuFilter.value) {
                url.searchParams.set('menuId', menuFilter.value);
            } else {
                url.searchParams.delete('menuId');
            }
            window.location.href = url.toString();
        });
    }

    window.AppCrud.createListPage({
        pageId: 'subMenusIndexPage',
        tableWrapperId: 'subMenusTableWrapper',
        recordCountId: 'subMenusRecordCount',
        indexUrl: page.getAttribute('data-index-url'),
        endpoint: 'SubMenus',
        queryDefaults: {
            includeInactive: 'false',
            menuId: page.getAttribute('data-selected-menu-id') || ''
        },
        updateStats: function (data, items) {
            var activeCount = 0;
            var linkedMenus = {};

            (items || []).forEach(function (item) {
                if (item.isActive) {
                    activeCount += 1;
                }
                if (item.menuName) {
                    linkedMenus[item.menuName] = true;
                }
            });

            setStat('total', items.length);
            setStat('active', activeCount);
            setStat('menus', Object.keys(linkedMenus).length);
        },
        renderEmptyState: function () {
            return '<div class="text-center py-5"><i class="fas fa-list-ul fa-3x text-muted mb-3"></i><h5 class="text-muted">No submenus found</h5><p class="text-muted">Get started by creating your first submenu.</p><a href="/Menus/Create" class="btn btn-primary mt-3"><i class="fas fa-plus me-2"></i>Create Menu First</a></div>';
        },
        renderTable: function (context) {
            var editBase = context.page.getAttribute('data-edit-base');

            return [
                '<div class="table-responsive">',
                '<table class="table table-hover align-middle">',
                '<thead class="table-light">',
                '<tr>',
                '<th>Menu</th>',
                '<th>Name</th>',
                '<th>Display Name</th>',
                '<th>Icon</th>',
                '<th>Route</th>',
                '<th>Order</th>',
                '<th>Status</th>',
                '<th class="text-end">Actions</th>',
                '</tr>',
                '</thead>',
                '<tbody>',
                context.items.map(function (subMenu) {
                    return [
                        '<tr>',
                        '<td><span class="badge bg-info">', context.escapeHtml(subMenu.menuName), '</span></td>',
                        '<td><div class="fw-semibold">', context.escapeHtml(subMenu.name), '</div></td>',
                        '<td>', context.escapeHtml(subMenu.displayName || '-'), '</td>',
                        '<td>', subMenu.icon ? '<i class="' + context.escapeHtml(subMenu.icon) + '"></i>' : '<span class="text-muted">-</span>', '</td>',
                        '<td>', context.escapeHtml(subMenu.route || '-'), '</td>',
                        '<td><span class="badge bg-secondary">', context.escapeHtml(subMenu.displayOrder), '</span></td>',
                        '<td>', subMenu.isActive ? '<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i>Active</span>' : '<span class="badge bg-danger"><i class="fas fa-times-circle me-1"></i>Inactive</span>', '</td>',
                        '<td class="text-end"><div class="btn-group" role="group"><a href="', context.replaceToken(editBase, '__id__', subMenu.id), '" class="btn btn-sm btn-outline-warning" title="Edit"><i class="fas fa-edit"></i></a><button type="button" class="btn btn-sm btn-outline-danger" title="Delete" data-crud-delete-id="', subMenu.id, '"><i class="fas fa-trash"></i></button></div></td>',
                        '</tr>'
                    ].join('');
                }).join(''),
                '</tbody>',
                '</table>',
                '</div>'
            ].join('');
        },
        getDeleteMessage: function (item) {
            return 'Are you sure you want to delete submenu "' + ((item && item.name) || 'this submenu') + '"?';
        },
        deleteSuccessMessage: 'SubMenu deleted successfully.',
        loadErrorMessage: 'Unable to load submenus right now.'
    });
})(window, document);
