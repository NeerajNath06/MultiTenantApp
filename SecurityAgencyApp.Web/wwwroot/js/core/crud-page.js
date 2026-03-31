(function (window, document) {
    'use strict';

    function escapeHtml(value) {
        return (value == null ? '' : String(value))
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function formatDate(value) {
        if (!value) {
            return '-';
        }

        var date = new Date(value);
        return isNaN(date.getTime()) ? value : date.toLocaleString();
    }

    function replaceToken(template, token, value) {
        return (template || '').replace(token || '__id__', value);
    }

    function getCurrentQuery(defaults) {
        var params = new URLSearchParams(window.location.search);
        var query = {};

        Object.keys(defaults || {}).forEach(function (key) {
            query[key] = params.get(key) || defaults[key];
        });

        return query;
    }

    function buildQueryString(query) {
        var params = new URLSearchParams();

        Object.keys(query || {}).forEach(function (key) {
            var value = query[key];
            if (value !== undefined && value !== null && value !== '') {
                params.set(key, value);
            }
        });

        return params.toString();
    }

    function toCamelCase(value) {
        return value ? value.charAt(0).toLowerCase() + value.slice(1) : value;
    }

    function fillForm(form, data) {
        Array.from(form.elements).forEach(function (element) {
            if (!element.name) {
                return;
            }

            var lowerName = element.name.toLowerCase();
            var matchedKey = Object.keys(data || {}).find(function (key) {
                return key.toLowerCase() === lowerName;
            });

            if (!matchedKey) {
                return;
            }

            var value = data[matchedKey];
            if (element.type === 'checkbox') {
                element.checked = !!value;
                return;
            }

            element.value = value == null ? '' : value;
        });
    }

    function buildPayload(form) {
        var payload = {};

        Array.from(form.elements).forEach(function (element) {
            if (!element.name || element.disabled || element.type === 'submit' || element.type === 'button') {
                return;
            }

            if (element.type === 'checkbox') {
                payload[toCamelCase(element.name)] = !!element.checked;
                return;
            }

            if (element.type === 'number') {
                payload[toCamelCase(element.name)] = element.value === '' ? null : Number(element.value);
                return;
            }

            payload[toCamelCase(element.name)] = element.value;
        });

        return payload;
    }

    function createListPage(config) {
        var page = document.getElementById(config.pageId);
        if (!page || !window.AppApi) {
            return;
        }

        var tableWrapper = document.getElementById(config.tableWrapperId);
        var recordCount = document.getElementById(config.recordCountId);
        var paginationContainer = document.getElementById(config.paginationContainerId);
        var paginationSummary = document.getElementById(config.paginationSummaryId);
        var pagination = document.getElementById(config.paginationId);
        var filterForm = config.filterFormId ? document.getElementById(config.filterFormId) : null;
        var currentItems = [];

        function currentQuery() {
            return getCurrentQuery(config.queryDefaults || {});
        }

        function navigate(query) {
            var qs = buildQueryString(query);
            window.location.href = config.indexUrl + (qs ? '?' + qs : '');
        }

        function renderPagination(data) {
            var totalPages = data.totalPages || 0;
            var pageNumber = data.pageNumber || 1;
            var query = currentQuery();

            if (!paginationContainer || !paginationSummary || !pagination) {
                return;
            }

            if (totalPages <= 1) {
                paginationContainer.classList.add('d-none');
                paginationSummary.textContent = '';
                pagination.innerHTML = '';
                return;
            }

            paginationContainer.classList.remove('d-none');
            paginationSummary.textContent = 'Page ' + pageNumber + ' of ' + totalPages + ' · ' + (data.totalCount || 0) + ' total';

            var html = [];

            function pageLink(label, targetPage, isActive, isIcon, direction) {
                var nextQuery = Object.assign({}, query, { pageNumber: String(targetPage) });
                return [
                    '<li class="page-item', isActive ? ' active' : '', '">',
                    '<a class="page-link" href="', config.indexUrl, '?', buildQueryString(nextQuery), '">',
                    isIcon ? '<i class="fas fa-chevron-' + direction + '"></i>' : label,
                    '</a>',
                    '</li>'
                ].join('');
            }

            if (pageNumber > 1) {
                html.push(pageLink('', pageNumber - 1, false, true, 'left'));
            }

            for (var i = Math.max(1, pageNumber - 2); i <= Math.min(totalPages, pageNumber + 2); i += 1) {
                html.push(pageLink(String(i), i, i === pageNumber, false));
            }

            if (pageNumber < totalPages) {
                html.push(pageLink('', pageNumber + 1, false, true, 'right'));
            }

            pagination.innerHTML = html.join('');
        }

        function load() {
            window.AppApi.get(config.endpoint, config.buildQuery ? config.buildQuery(currentQuery()) : currentQuery())
                .then(function (data) {
                    currentItems = data.items || [];

                    if (recordCount) {
                        recordCount.textContent = (config.recordCountText ? config.recordCountText(data) : ((data.totalCount || 0) + ' total records'));
                    }

                    if (config.updateStats) {
                        config.updateStats(data, currentItems);
                    }

                    if (!currentItems.length) {
                        tableWrapper.innerHTML = config.renderEmptyState();
                    } else {
                        tableWrapper.innerHTML = config.renderTable({
                            data: data,
                            items: currentItems,
                            query: currentQuery(),
                            indexUrl: config.indexUrl,
                            escapeHtml: escapeHtml,
                            replaceToken: replaceToken,
                            formatDate: formatDate,
                            buildQueryString: buildQueryString,
                            page: page
                        });
                    }

                    renderPagination(data);
                })
                .catch(function () {
                    if (tableWrapper) {
                        tableWrapper.innerHTML = '<div class="text-center py-5 text-danger">' + escapeHtml(config.loadErrorMessage || 'Unable to load data right now.') + '</div>';
                    }
                });
        }

        if (filterForm) {
            filterForm.addEventListener('submit', function (event) {
                event.preventDefault();
                var formData = new FormData(filterForm);
                var query = currentQuery();
                query.pageNumber = '1';

                Array.from(formData.entries()).forEach(function (entry) {
                    if (!entry[0]) {
                        return;
                    }

                    if (entry[1]) {
                        query[entry[0]] = entry[1];
                    } else {
                        delete query[entry[0]];
                    }
                });

                navigate(query);
            });
        }

        page.addEventListener('click', function (event) {
            var deleteButton = event.target.closest(config.deleteButtonSelector || '[data-crud-delete-id]');
            if (!deleteButton) {
                return;
            }

            var itemId = deleteButton.getAttribute(config.deleteIdAttribute || 'data-crud-delete-id');
            var item = currentItems.find(function (entry) {
                return String(entry.id) === String(itemId);
            }) || null;

            var confirmPromise = window.AppUi && typeof window.AppUi.confirm === 'function'
                ? window.AppUi.confirm({
                    message: config.getDeleteMessage ? config.getDeleteMessage(item, deleteButton) : 'Please confirm delete.',
                    confirmText: config.deleteConfirmText || 'Delete'
                })
                : Promise.resolve(window.confirm(config.getDeleteMessage ? config.getDeleteMessage(item, deleteButton) : 'Please confirm delete.'));

            confirmPromise.then(function (confirmed) {
                if (!confirmed) {
                    return;
                }

                var deleteEndpoint = config.getDeleteEndpoint ? config.getDeleteEndpoint(item, deleteButton) : (config.endpoint + '/' + itemId);
                window.AppApi.delete(deleteEndpoint)
                    .then(function () {
                        if (window.AppUi) {
                            window.AppUi.showToast({
                                type: 'success',
                                title: 'Deleted',
                                message: config.deleteSuccessMessage || 'Deleted successfully.'
                            });
                        }

                        load();
                    });
            });
        });

        load();

        return {
            reload: load
        };
    }

    function createFormPage(config) {
        var page = document.getElementById(config.pageId);
        var form = document.getElementById(config.formId);
        if (!page || !form || !window.AppApi) {
            return;
        }

        var errorSummary = config.errorSummaryId ? document.getElementById(config.errorSummaryId) : null;
        var submitButton = config.submitButtonId ? document.getElementById(config.submitButtonId) : null;
        var mode = page.getAttribute('data-mode') || config.mode || 'create';

        function clearErrors() {
            if (!errorSummary) {
                return;
            }

            errorSummary.textContent = '';
            errorSummary.classList.add('d-none');
        }

        function showErrors(message) {
            if (!errorSummary) {
                return;
            }

            errorSummary.textContent = message || 'Please review the form and try again.';
            errorSummary.classList.remove('d-none');
        }

        function setSubmitting(isSubmitting) {
            if (submitButton) {
                submitButton.disabled = isSubmitting;
            }
        }

        function loadEntity() {
            if (typeof config.loadEntity === 'function') {
                return config.loadEntity(form, page);
            }

            var endpoint = page.getAttribute(config.loadEndpointAttribute || 'data-load-endpoint');
            if (!endpoint) {
                return Promise.resolve();
            }

            return window.AppApi.get(endpoint)
                .then(function (data) {
                    if (config.mapResponseToForm) {
                        config.mapResponseToForm(form, data);
                    } else {
                        fillForm(form, data);
                    }
                });
        }

        form.addEventListener('submit', function (event) {
            event.preventDefault();
            clearErrors();
            setSubmitting(true);

            var payload = buildPayload(form);
            if (config.transformPayload) {
                payload = config.transformPayload(payload, form, mode);
            }

            var endpoint = page.getAttribute(config.submitEndpointAttribute || 'data-submit-endpoint');
            var redirectUrl = page.getAttribute(config.redirectUrlAttribute || 'data-redirect-url');
            var request = mode === 'edit'
                ? window.AppApi.put(endpoint, payload)
                : window.AppApi.post(endpoint, payload);

            request.then(function () {
                if (window.AppUi) {
                    window.AppUi.showToast({
                        type: 'success',
                        title: mode === 'edit' ? 'Updated' : 'Created',
                        message: mode === 'edit'
                            ? (config.updateSuccessMessage || 'Updated successfully.')
                            : (config.createSuccessMessage || 'Created successfully.')
                    });
                }

                window.setTimeout(function () {
                    window.location.href = redirectUrl;
                }, 300);
            }).catch(function (error) {
                showErrors(error && error.message ? error.message : (config.saveErrorMessage || 'Unable to save changes.'));
            }).finally(function () {
                setSubmitting(false);
            });
        });

        var initializePromise = typeof config.initialize === 'function'
            ? Promise.resolve(config.initialize(form, page, mode))
            : Promise.resolve();

        initializePromise.then(function () {
            if (mode === 'edit') {
                return loadEntity();
            }

            return null;
        }).catch(function (error) {
            showErrors(error && error.message ? error.message : (config.loadErrorMessage || 'Unable to load data.'));
        });
    }

    function createDetailsPage(config) {
        var page = document.getElementById(config.pageId);
        if (!page || !window.AppApi) {
            return;
        }

        var deleteButton = config.deleteButtonId ? document.getElementById(config.deleteButtonId) : null;

        function load() {
            window.AppApi.get(page.getAttribute(config.loadEndpointAttribute || 'data-load-endpoint'))
                .then(function (data) {
                    config.render(page, data, {
                        setText: function (field, value) {
                            var element = page.querySelector('[data-client-field="' + field + '"], [data-field="' + field + '"]');
                            if (element) {
                                element.textContent = value == null || value === '' ? '-' : String(value);
                            }
                        },
                        formatDate: formatDate
                    });
                });
        }

        if (deleteButton) {
            deleteButton.addEventListener('click', function () {
                var confirmMessage = config.getDeleteMessage ? config.getDeleteMessage(page) : 'Please confirm delete.';
                var deleteEndpoint = config.getDeleteEndpoint(page);
                var redirectUrl = page.getAttribute(config.redirectUrlAttribute || 'data-index-url');

                var confirmPromise = window.AppUi && typeof window.AppUi.confirm === 'function'
                    ? window.AppUi.confirm({ message: confirmMessage, confirmText: config.deleteConfirmText || 'Delete' })
                    : Promise.resolve(window.confirm(confirmMessage));

                confirmPromise.then(function (confirmed) {
                    if (!confirmed) {
                        return;
                    }

                    window.AppApi.delete(deleteEndpoint)
                        .then(function () {
                            if (window.AppUi) {
                                window.AppUi.showToast({
                                    type: 'success',
                                    title: 'Deleted',
                                    message: config.deleteSuccessMessage || 'Deleted successfully.'
                                });
                            }

                            window.setTimeout(function () {
                                window.location.href = redirectUrl;
                            }, 300);
                        });
                });
            });
        }

        load();
    }

    window.AppCrud = {
        escapeHtml: escapeHtml,
        formatDate: formatDate,
        replaceToken: replaceToken,
        createListPage: createListPage,
        createFormPage: createFormPage,
        createDetailsPage: createDetailsPage
    };
})(window, document);
