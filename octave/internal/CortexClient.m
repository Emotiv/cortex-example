classdef CortexClient < handle
    % CortexClient  JSON-RPC client that connects straight to Cortex over WSS.
    %   Works in GNU Octave (with Java) and MATLAB via lib/cortex-ws-client.jar (nv-websocket-client).

    properties (Access = private)
        ws
        nextId = 1
        pendingResponses = {}
        streamMessages = {}
        warningMessages = {}
        warningHandlers = {}
    end

    properties (SetAccess = private)
        lastWarning = []
    end

    properties
        requestTimeout = 60
    end

    methods
        function obj = CortexClient(host, port, requestTimeout)
            if nargin < 1, host = 'localhost'; end
            if nargin < 2, port = 6868; end
            if nargin < 3, requestTimeout = 60; end

            obj.requestTimeout = requestTimeout;
            obj.validateRequestTimeout();

            internalDir = fileparts(mfilename('fullpath'));
            libDir = fullfile(internalDir, '..', 'lib');
            CortexClient.addJars(libDir);
            caPath = fullfile(internalDir, '..', '..', 'certificates', 'rootCA.pem');
            if ~exist(caPath, 'file')
                error('CortexClient:certificate', ...
                    'Required Cortex CA certificate not found: %s', caPath);
            end

            url = sprintf('wss://%s:%d', host, port);
            obj.ws = javaObject('cortex.CortexWsClient');
            obj.ws.connect(url, caPath, int32(10000));
        end

        function response = sendRequest(obj, method, params)
            if nargin < 3, params = struct(); end

            obj.validateRequestTimeout();
            requestStartedAt = tic();
            id = obj.nextId;
            obj.nextId = id + 1;
            request = struct('jsonrpc', '2.0', 'id', id, 'method', method, 'params', params);
            obj.ws.sendText(jsonencode(request));

            queued = obj.takeResponse(id, method);
            if ~isempty(queued)
                response = queued;
                return;
            end

            while true
                obj.checkRequestDeadline(requestStartedAt, method);
                [messageType, msg] = obj.readMessage();
                obj.checkRequestDeadline(requestStartedAt, method);
                if strcmp(messageType, 'timeout'), continue; end

                switch messageType
                    case 'result'
                        if obj.hasResponseId(msg, id)
                            response = msg;
                            return;
                        end
                        obj.pendingResponses{end + 1} = msg;
                    case 'error'
                        if obj.hasResponseId(msg, id)
                            obj.raiseRpcError(method, msg.error);
                        end
                        obj.pendingResponses{end + 1} = msg;
                    case 'warning'
                        obj.handleWarning(msg.warning);
                    case 'stream'
                        obj.streamMessages{end + 1} = msg;
                    otherwise
                        error('CortexClient:message', 'Unsupported Cortex message type: %s', messageType);
                end
            end
        end

        function line = readLine(obj)
            % Returns '' when no data arrives within the 1 s socket timeout.
            line = obj.readSocketLine();
        end

        function stream = readStreamData(obj)
            if ~isempty(obj.streamMessages)
                stream = obj.streamMessages{1};
                obj.streamMessages(1) = [];
                return;
            end

            while true
                [messageType, msg] = obj.readMessage();
                if strcmp(messageType, 'timeout')
                    stream = [];
                    return;
                end

                switch messageType
                    case 'stream'
                        stream = msg;
                        return;
                    case 'warning'
                        obj.handleWarning(msg.warning);
                    case 'result'
                        obj.pendingResponses{end + 1} = msg;
                    case 'error'
                        obj.raiseRpcError('unsolicited message', msg.error);
                    otherwise
                        error('CortexClient:message', 'Unsupported Cortex message type: %s', messageType);
                end
            end
        end

        function onWarning(obj, warningCode, handler)
            if ~isnumeric(warningCode) || ~isscalar(warningCode)
                error('CortexClient:warningHandler', 'Warning code must be a numeric scalar.');
            end
            if ~isa(handler, 'function_handle')
                error('CortexClient:warningHandler', 'Warning handler must be a function handle.');
            end

            registration = struct('code', warningCode, 'handler', handler);
            obj.warningHandlers{end + 1} = registration;
        end

        function warning = waitForWarning(obj, warningCode, timeoutSeconds)
            if nargin < 3, timeoutSeconds = 60; end
            if ~isnumeric(warningCode) || ~isscalar(warningCode)
                error('CortexClient:warningWait', 'Warning code must be a numeric scalar.');
            end
            if ~isnumeric(timeoutSeconds) || ~isscalar(timeoutSeconds) || timeoutSeconds < 0
                error('CortexClient:warningWait', 'Timeout must be a non-negative numeric scalar.');
            end

            warning = obj.takeWarning(warningCode);
            if ~isempty(warning), return; end

            startedAt = tic();
            while toc(startedAt) < timeoutSeconds
                [messageType, msg] = obj.readMessage();
                if strcmp(messageType, 'timeout'), continue; end

                switch messageType
                    case 'warning'
                        obj.handleWarning(msg.warning);
                        warning = obj.takeWarning(warningCode);
                        if ~isempty(warning), return; end
                    case 'stream'
                        obj.streamMessages{end + 1} = msg;
                    case 'result'
                        obj.pendingResponses{end + 1} = msg;
                    case 'error'
                        obj.raiseRpcError('unsolicited message', msg.error);
                    otherwise
                        error('CortexClient:message', 'Unsupported Cortex message type: %s', messageType);
                end
            end
            warning = [];
        end

        function close(obj)
            if isempty(obj.ws), return; end
            try
                obj.ws.close();
            catch
            end
            obj.ws = [];
        end
    end

    methods (Static, Access = private)
        function addJars(libDir)
            loaded = javaclasspath('-all');
            [status, libInfo] = fileattrib(libDir);
            if status
                libDir = libInfo.Name;
            end
            for jarName = {'nv-websocket-client-2.14.jar', 'cortex-ws-client.jar'}
                jarPath = fullfile(libDir, jarName{1});
                if ~exist(jarPath, 'file')
                    error('CortexClient:jar', 'Missing %s. Restore the lib folder from the repository.', jarPath);
                end
                if ~any(strcmp(loaded, jarPath))
                    javaaddpath(jarPath);
                end
            end
        end
    end

    methods (Access = private)
        function validateRequestTimeout(obj)
            if ~isnumeric(obj.requestTimeout) || ~isscalar(obj.requestTimeout) || ...
                    ~isfinite(obj.requestTimeout) || obj.requestTimeout <= 0
                error('CortexClient:requestTimeoutConfig', ...
                    'requestTimeout must be a finite, positive numeric scalar.');
            end
        end

        function checkRequestDeadline(obj, requestStartedAt, method)
            if toc(requestStartedAt) >= obj.requestTimeout
                error('CortexClient:requestTimeout', ...
                    'Timed out waiting for a response to "%s" after %.3g seconds.', ...
                    method, obj.requestTimeout);
            end
        end

        function [messageType, msg] = readMessage(obj)
            line = obj.readSocketLine();
            if isempty(line)
                messageType = 'timeout';
                msg = [];
                return;
            end

            msg = jsondecode(line);
            messageType = obj.classifyMessage(msg);
        end

        function messageType = classifyMessage(obj, msg)
            if ~isstruct(msg)
                error('CortexClient:message', 'Cortex returned a non-object JSON message.');
            elseif isfield(msg, 'sid')
                messageType = 'stream';
            elseif isfield(msg, 'result')
                messageType = 'result';
            elseif isfield(msg, 'error')
                messageType = 'error';
            elseif isfield(msg, 'warning')
                messageType = 'warning';
            else
                error('CortexClient:message', 'Cortex returned an object with no sid, result, error, or warning field.');
            end
        end

        function matched = hasResponseId(obj, msg, requestId)
            matched = isfield(msg, 'id') && isequal(msg.id, requestId);
        end

        function response = takeResponse(obj, requestId, method)
            response = [];
            for index = 1:numel(obj.pendingResponses)
                candidate = obj.pendingResponses{index};
                if obj.hasResponseId(candidate, requestId)
                    response = candidate;
                    obj.pendingResponses(index) = [];
                    if strcmp(obj.classifyMessage(candidate), 'error')
                        obj.raiseRpcError(method, candidate.error);
                    end
                    return;
                end
            end
        end

        function warning = takeWarning(obj, warningCode)
            warning = [];
            for index = 1:numel(obj.warningMessages)
                candidate = obj.warningMessages{index};
                if isequal(candidate.code, warningCode)
                    warning = candidate;
                    obj.warningMessages(index) = [];
                    return;
                end
            end
        end

        function raiseRpcError(obj, method, rpcError)
            code = 'unknown';
            message = jsonencode(rpcError);
            if isstruct(rpcError)
                if isfield(rpcError, 'code')
                    code = num2str(rpcError.code);
                end
                if isfield(rpcError, 'message')
                    message = char(rpcError.message);
                end
            end
            error('CortexClient:rpc', '%s failed (code %s): %s', method, code, message);
        end

        function handleWarning(obj, warning)
            if ~isstruct(warning) || ~isfield(warning, 'code')
                error('CortexClient:warning', 'Cortex returned an invalid warning object.');
            end

            obj.lastWarning = warning;
            obj.warningMessages{end + 1} = warning;

            for index = 1:numel(obj.warningHandlers)
                registration = obj.warningHandlers{index};
                if isequal(registration.code, warning.code)
                    registration.handler(warning);
                end
            end
        end

        function line = readSocketLine(obj)
            % Java returns null (empty in Octave) on timeout; a closed link throws.
            try
                line = obj.ws.poll(int32(1000));
            catch err
                error('CortexClient:closed', 'Connection to Cortex closed: %s', err.message);
            end
            if isempty(line)
                line = '';
            else
                line = char(line);
            end
        end
    end
end
