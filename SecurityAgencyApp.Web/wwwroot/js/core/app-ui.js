(function (window, document) {
    'use strict';

    var loaderCount = 0;

    function getLoader() {
        return document.getElementById('globalApiLoader');
    }

    function getToastContainer() {
        return document.getElementById('appToastContainer');
    }

    function setLoaderVisible(isVisible) {
        var loader = getLoader();
        if (!loader) {
            return;
        }

        loader.classList.toggle('d-none', !isVisible);
        document.body.classList.toggle('overflow-hidden', isVisible);
    }

    function normalizeType(type) {
        var value = (type || 'info').toLowerCase();
        if (value === 'error') {
            return 'danger';
        }

        return value;
    }

    function createToastMarkup(type, title, message) {
        return [
            '<div class="toast align-items-center border-0 app-toast app-toast-', type, '" role="alert" aria-live="assertive" aria-atomic="true">',
            '<div class="d-flex">',
            '<div class="toast-body">',
            title ? '<strong class="d-block mb-1">' + title + '</strong>' : '',
            message || '',
            '</div>',
            '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>',
            '</div>',
            '</div>'
        ].join('');
    }

    function showToast(options) {
        var container = getToastContainer();
        if (!container || typeof bootstrap === 'undefined') {
            return;
        }

        var type = normalizeType(options && options.type);
        var toastHost = document.createElement('div');
        toastHost.innerHTML = createToastMarkup(type, options && options.title, options && options.message);

        var toastEl = toastHost.firstElementChild;
        container.appendChild(toastEl);

        var toast = bootstrap.Toast.getOrCreateInstance(toastEl, {
            autohide: options && options.autohide !== false,
            delay: options && options.delay ? options.delay : 4000
        });

        toastEl.addEventListener('hidden.bs.toast', function () {
            toastEl.remove();
        });

        toast.show();
    }

    function showPopup(options) {
        if (typeof window.showCrudFeedbackModal === 'function') {
            window.showCrudFeedbackModal({
                type: options && options.type ? options.type : 'info',
                title: options && options.title ? options.title : 'Information',
                message: options && options.message ? options.message : '',
                buttonText: options && options.buttonText ? options.buttonText : 'OK'
            });
        }
    }

    function confirmAction(options) {
        return new Promise(function (resolve) {
            var modalEl = document.getElementById('confirmModal');
            if (!modalEl || typeof bootstrap === 'undefined') {
                resolve(window.confirm(options && options.message ? options.message : 'Please confirm this action.'));
                return;
            }

            var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            var messageEl = modalEl.querySelector('.confirm-modal-message');
            var confirmBtn = modalEl.querySelector('.btn-confirm-submit');
            var confirmLabel = modalEl.querySelector('.btn-confirm-label');

            if (messageEl) {
                messageEl.textContent = options && options.message ? options.message : 'Please confirm this action.';
            }

            if (confirmLabel) {
                confirmLabel.textContent = options && options.confirmText ? options.confirmText : 'Confirm';
            }

            var onConfirm = function () {
                cleanup();
                modal.hide();
                resolve(true);
            };

            var onHidden = function () {
                cleanup();
                resolve(false);
            };

            function cleanup() {
                if (confirmBtn) {
                    confirmBtn.removeEventListener('click', onConfirm);
                }
                modalEl.removeEventListener('hidden.bs.modal', onHidden);
            }

            if (confirmBtn) {
                confirmBtn.addEventListener('click', onConfirm, { once: true });
            }
            modalEl.addEventListener('hidden.bs.modal', onHidden, { once: true });
            modal.show();
        });
    }

    function showApiError(error) {
        var status = error && error.status ? error.status : 0;
        var message = error && error.message ? error.message : 'An unexpected error occurred.';
        var title = 'Request failed';

        if (status === 401) {
            title = 'Session expired';
        } else if (status === 403) {
            title = 'Access denied';
        } else if (status >= 500) {
            title = 'Server error';
        }

        showPopup({
            type: 'danger',
            title: title,
            message: message
        });
    }

    window.AppUi = {
        showLoader: function () {
            loaderCount += 1;
            setLoaderVisible(true);
        },
        hideLoader: function () {
            loaderCount = Math.max(0, loaderCount - 1);
            setLoaderVisible(loaderCount > 0);
        },
        resetLoader: function () {
            loaderCount = 0;
            setLoaderVisible(false);
        },
        showToast: showToast,
        showPopup: showPopup,
        confirm: confirmAction,
        showApiError: showApiError
    };
})(window, document);
