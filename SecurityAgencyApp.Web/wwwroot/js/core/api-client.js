(function (window, document) {
    'use strict';

    function getMetaContent(name) {
        var element = document.querySelector('meta[name="' + name + '"]');
        return element ? element.getAttribute('content') || '' : '';
    }

    function trimSlashes(value) {
        return (value || '').replace(/\/+$/, '');
    }

    function buildQueryString(params) {
        var searchParams = new URLSearchParams();
        Object.keys(params || {}).forEach(function (key) {
            var value = params[key];
            if (value === undefined || value === null || value === '') {
                return;
            }

            searchParams.set(key, value);
        });

        var query = searchParams.toString();
        return query ? '?' + query : '';
    }

    function getApiMessage(data, fallbackMessage) {
        if (!data) {
            return fallbackMessage;
        }

        if (typeof data === 'string') {
            return data;
        }

        if (data.message) {
            return data.message;
        }

        if (Array.isArray(data.errors) && data.errors.length > 0) {
            return data.errors
                .map(function (error) {
                    return error && error.message ? error.message : error;
                })
                .filter(Boolean)
                .join('\n');
        }

        return fallbackMessage;
    }

    var apiBaseUrl = trimSlashes(getMetaContent('app-api-base-url'));
    var accessToken = getMetaContent('app-access-token');
    var tenantId = getMetaContent('app-tenant-id');

    var client = axios.create({
        baseURL: apiBaseUrl,
        withCredentials: true,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    });

    client.interceptors.request.use(function (config) {
        window.AppUi && window.AppUi.showLoader();

        config.headers = config.headers || {};

        if (accessToken) {
            config.headers.Authorization = 'Bearer ' + accessToken;
        }

        if (tenantId) {
            config.headers['X-Tenant-Id'] = tenantId;
        }

        return config;
    }, function (error) {
        window.AppUi && window.AppUi.hideLoader();
        return Promise.reject(error);
    });

    client.interceptors.response.use(function (response) {
        window.AppUi && window.AppUi.hideLoader();
        return response;
    }, function (error) {
        window.AppUi && window.AppUi.hideLoader();

        var response = error && error.response ? error.response : null;
        var status = response ? response.status : 0;
        var message = getApiMessage(response && response.data, 'Unable to complete the request.');

        if (window.AppUi) {
            window.AppUi.showApiError({
                status: status,
                message: message
            });
        }

        return Promise.reject({
            status: status,
            message: message,
            response: response
        });
    });

    function unwrapResponse(response) {
        var payload = response && response.data ? response.data : null;
        if (!payload) {
            return null;
        }

        if (payload.success === false) {
            throw {
                status: response.status,
                message: getApiMessage(payload, 'Request failed.'),
                response: response
            };
        }

        return Object.prototype.hasOwnProperty.call(payload, 'data') ? payload.data : payload;
    }

    window.AppApi = {
        client: client,
        buildQueryString: buildQueryString,
        get: function (url, params, config) {
            return client.get(url + buildQueryString(params), config || {}).then(unwrapResponse);
        },
        post: function (url, data, config) {
            return client.post(url, data, config || {}).then(unwrapResponse);
        },
        put: function (url, data, config) {
            return client.put(url, data, config || {}).then(unwrapResponse);
        },
        patch: function (url, data, config) {
            return client.patch(url, data, config || {}).then(unwrapResponse);
        },
        delete: function (url, config) {
            return client.delete(url, config || {}).then(unwrapResponse);
        }
    };
})(window, document);
