(function (window, document) {
    'use strict';

    if (!window.AppApi || !window.AppCrud) {
        return;
    }

    var page = document.getElementById('vehicleDetailsPage');
    if (!page) {
        return;
    }

    var deleteButton = document.getElementById('vehicleDeleteButton');
    var exitButton = document.getElementById('vehicleExitButton');

    function load() {
        return window.AppApi.get(page.getAttribute('data-load-endpoint')).then(function (data) {
            var fields = {
                vehicleNumber: data.vehicleNumber,
                vehicleType: data.vehicleType,
                driverName: data.driverName,
                driverPhone: data.driverPhone,
                purpose: data.purpose,
                parkingSlot: data.parkingSlot,
                siteName: data.siteName,
                guardName: data.guardName,
                entryTime: window.AppCrud.formatDate(data.entryTime),
                exitTime: data.exitTime ? window.AppCrud.formatDate(data.exitTime) : 'Inside premises'
            };

            Object.keys(fields).forEach(function (key) {
                var element = page.querySelector('[data-field="' + key + '"]');
                if (element) {
                    element.textContent = fields[key] == null || fields[key] === '' ? '-' : String(fields[key]);
                }
            });

            if (exitButton) {
                exitButton.classList.toggle('d-none', !!data.exitTime);
            }
        });
    }

    if (deleteButton) {
        deleteButton.addEventListener('click', function () {
            var confirmPromise = window.AppUi && typeof window.AppUi.confirm === 'function'
                ? window.AppUi.confirm({ message: 'Are you sure you want to delete this vehicle log entry?', confirmText: 'Delete' })
                : Promise.resolve(window.confirm('Are you sure you want to delete this vehicle log entry?'));

            confirmPromise.then(function (confirmed) {
                if (!confirmed) {
                    return;
                }

                return window.AppApi.delete('Vehicles/' + page.getAttribute('data-vehicle-log-id')).then(function () {
                    if (window.AppUi) {
                        window.AppUi.showToast({
                            type: 'success',
                            title: 'Deleted',
                            message: 'Vehicle log deleted successfully.'
                        });
                    }

                    window.location.href = page.getAttribute('data-index-url');
                });
            });
        });
    }

    if (exitButton) {
        exitButton.addEventListener('click', function () {
            var confirmPromise = window.AppUi && typeof window.AppUi.confirm === 'function'
                ? window.AppUi.confirm({ message: 'Record exit for this vehicle?', confirmText: 'Record Exit' })
                : Promise.resolve(window.confirm('Record exit for this vehicle?'));

            confirmPromise.then(function (confirmed) {
                if (!confirmed) {
                    return;
                }

                return window.AppApi.patch('Vehicles/' + page.getAttribute('data-vehicle-log-id') + '/exit', { exitTime: null }).then(function () {
                    if (window.AppUi) {
                        window.AppUi.showToast({
                            type: 'success',
                            title: 'Exit Recorded',
                            message: 'Vehicle exit recorded.'
                        });
                    }

                    return load();
                });
            });
        });
    }

    load();
})(window, document);
