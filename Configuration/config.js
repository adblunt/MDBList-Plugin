define(['baseView', 'loading', 'toast', 'emby-input', 'emby-button'], function (BaseView, loading, toast) {
    'use strict';

    const pluginId = '2f0cbf4d-249d-4f22-b451-2f2e1766fb7e';

    return class extends BaseView {
        constructor(view, params) {
            super(view, params);
        }

        onResume(options) {
            super.onResume(options);
            const view = this.view;
            this.loadConfig(view);

            const form = view.querySelector('.MdbListRatingsProviderConfigForm');
            if (form) {
                form.addEventListener('submit', (e) => this.onSubmit(e));
            }
        }

        loadConfig(view) {
            const self = this;
            loading.show();

            ApiClient.getPluginConfiguration(pluginId).then(function (config) {
                const apiKeyInput = view.querySelector('#ApiKey');
                if (apiKeyInput) {
                    apiKeyInput.value = (config && config.ApiKey) || '';
                }
                loading.hide();
            }).catch(function (err) {
                console.error('MdbList: Failed to load configuration:', err);
                loading.hide();
            });
        }

        onSubmit(e) {
            e.preventDefault();
            const self = this;
            const view = this.view;
            loading.show();

            const apiKey = view.querySelector('#ApiKey').value;

            ApiClient.getPluginConfiguration(pluginId).then(function (config) {
                config = config || {};
                config.ApiKey = apiKey;
                return ApiClient.updatePluginConfiguration(pluginId, config);
            }).then(function () {
                loading.hide();
                toast({
                    text: 'Settings saved successfully.'
                });
            }).catch(function (err) {
                console.error('MdbList: Failed to save configuration:', err);
                loading.hide();
                Dashboard.alert({
                    message: 'Unable to save settings. Please check server logs.',
                    title: 'Error'
                });
            });

            return false;
        }
    };
});
