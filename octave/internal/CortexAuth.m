classdef CortexAuth < handle
    % CortexAuth  Opens the proxy connection, checks login/access rights and holds the cortexToken.

    properties
        license = ''
        debit = 100
    end

    properties (SetAccess = private)
        isUserLoggedIn = false
        isAccessGranted = false
    end

    properties
        accessWaitTimeout = 180
    end

    properties (Access = private)
        client
        clientId
        clientSecret
        context
    end

    methods
        function obj = CortexAuth(clientId, clientSecret, host, port, context)
            if nargin < 3, host = '127.0.0.1'; end
            if nargin < 4, port = 9999; end
            if nargin < 5, context = CortexContext(); end
            obj.clientId = clientId;
            obj.clientSecret = clientSecret;
            obj.context = context;
            obj.client = CortexClient(host, port);
            obj.client.onWarning(2, @(warning) obj.handleWarning(warning));
            obj.client.onWarning(3, @(warning) obj.handleWarning(warning));
            obj.client.onWarning(9, @(warning) obj.handleWarning(warning));
        end

        function init(obj)
            % Verifies a user is logged in to EMOTIV Launcher and that this app has access.
            logins = obj.getUserLogin();
            if isempty(logins)
                error('CortexAuth:noUser', 'No user is logged in. Log in via EMOTIV Launcher and run again.');
            end

            if ~obj.hasAccessRight()
                fprintf('[CortexAuth] Application access is not granted. Requesting approval from Cortex...\n');
                if ~obj.requestAccess()
                    fprintf(['[CortexAuth] Application access is not granted yet.\n' ...
                        '[CortexAuth] Open EMOTIV Launcher, sign in, and approve this application.\n' ...
                        '[CortexAuth] Waiting for approval for up to %d seconds...\n'], obj.accessWaitTimeout);
                    grantedWarning = obj.client.waitForWarning(9, obj.accessWaitTimeout);
                    if isempty(grantedWarning)
                        error('CortexAuth:accessDenied', ...
                            ['Application access was not approved within %d seconds. ' ...
                             'Open EMOTIV Launcher, approve the application, and try again.'], ...
                            obj.accessWaitTimeout);
                    end
                end
            end

            if isempty(obj.context.token)
                fprintf('[CortexAuth] Application access confirmed. Authorizing...\n');
                obj.authorize();
            end
        end

        function logins = getUserLogin(obj)
            response = obj.client.sendRequest('getUserLogin', struct());
            logins = response.result;
            obj.isUserLoggedIn = ~isempty(logins);
        end

        function granted = hasAccessRight(obj)
            response = obj.client.sendRequest('hasAccessRight', obj.credentialParams());
            granted = isfield(response.result, 'accessGranted') && logical(response.result.accessGranted);
            obj.isAccessGranted = granted;
        end

        function granted = requestAccess(obj)
            response = obj.client.sendRequest('requestAccess', obj.credentialParams());
            granted = isfield(response.result, 'accessGranted') && logical(response.result.accessGranted);
            obj.isAccessGranted = granted;
        end

        function token = authorize(obj)
            params = obj.credentialParams();
            params.license = obj.license;
            params.debit = obj.debit;
            response = obj.client.sendRequest('authorize', params);
            obj.context.token = response.result.cortexToken;
            token = obj.context.token;
        end

        function userInfo = getUserInformation(obj)
            if isempty(obj.context.token)
                error('CortexAuth:notAuthorized', 'Authorize before requesting user information.');
            end

            params = struct('cortexToken', obj.context.token);
            response = obj.client.sendRequest('getUserInformation', params);
            obj.context.userInfo = response.result;
            userInfo = obj.context.userInfo;
        end

        function licenseInfo = getLicenseInfo(obj)
            if isempty(obj.context.token)
                error('CortexAuth:notAuthorized', 'Authorize before requesting license information.');
            end

            params = struct('cortexToken', obj.context.token);
            response = obj.client.sendRequest('getLicenseInfo', params);
            if ~isfield(response.result, 'license')
                error('CortexAuth:licenseInfo', 'Cortex returned no license information.');
            end
            obj.context.licenseInfo = response.result.license;
            licenseInfo = obj.context.licenseInfo;
        end

        function client = getClient(obj)
            client = obj.client;
        end

        function context = getContext(obj)
            context = obj.context;
        end

        function close(obj)
            obj.client.close();
        end
    end

    methods (Access = private)
        function handleWarning(obj, warning)
            switch warning.code
                case 2
                    obj.isUserLoggedIn = true;
                    description = 'User logged in.';
                case 3
                    obj.isUserLoggedIn = false;
                    obj.isAccessGranted = false;
                    obj.context.token = '';
                    obj.context.userInfo = [];
                    obj.context.licenseInfo = [];
                    description = 'User logged out.';
                case 9
                    obj.isAccessGranted = true;
                    description = 'Application access approved.';
                otherwise
                    description = 'Unhandled authentication warning.';
            end
            fprintf('[CortexAuth] Warning %d: %s\n', warning.code, description);
        end

        function params = credentialParams(obj)
            params = struct('clientId', obj.clientId, 'clientSecret', obj.clientSecret);
        end
    end
end
