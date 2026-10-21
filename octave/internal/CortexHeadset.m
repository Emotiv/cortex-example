classdef CortexHeadset < handle
    % CortexHeadset  Scans for headsets, connects one, opens a session and subscribes to data streams.
    %   The client is single-threaded, so the scan "timer" runs inside update(), which also reads
    %   the incoming data samples into the context buffer.

    properties
        connectTimeout = 5      % seconds to wait for warning 104 (headset connected)
    end

    properties (SetAccess = private)
        isScanning = false
        isSubscribed = false
        autoRescan = true
        queryInterval = 3
    end

    properties (Access = private)
        client
        context
        scanFinished = false
        lastQueryAt = []
        parser
    end

    methods
        function obj = CortexHeadset(client, context)
            obj.client = client;
            obj.context = context;
            obj.parser = CortexStreamParser();
            if isempty(context.buffer)
                context.buffer = CortexDataBuffer(10000);
            end
            obj.client.onWarning(142, @(warning) obj.onScanFinished(warning));
        end

        function scanHeadset(obj, autoRescan, queryInterval)
            % Starts a scan and queries the headset list every queryInterval seconds (see update()).
            if nargin < 2, autoRescan = true; end
            if nargin < 3, queryInterval = 3; end
            if ~(islogical(autoRescan) || isnumeric(autoRescan)) || ~isscalar(autoRescan)
                error('CortexHeadset:scanOptions', 'autoRescan must be true or false.');
            end
            if ~isnumeric(queryInterval) || ~isscalar(queryInterval) || queryInterval <= 0
                error('CortexHeadset:scanOptions', 'queryInterval must be a positive number of seconds.');
            end

            obj.autoRescan = logical(autoRescan);
            obj.queryInterval = double(queryInterval);
            fprintf('[CortexHeadset] Scanning for headsets...\n');
            obj.refreshHeadsets();
            obj.isScanning = true;
            obj.queryHeadsets();
        end

        function stopScan(obj)
            obj.isScanning = false;
        end

        function headsets = queryHeadsets(obj)
            response = obj.client.sendRequest('queryHeadsets', struct());
            obj.context.headsets = CortexHeadset.toCell(response.result);
            obj.lastQueryAt = tic();
            headsets = obj.context.headsets;
            if isempty(headsets)
                fprintf('[CortexHeadset] No headsets found.\n');
            else
                fprintf('[CortexHeadset] Headsets (id | status):\n');
                for index = 1:numel(headsets)
                    fprintf('  %s | %s\n', CortexHeadset.textField(headsets{index}, 'id'), ...
                        CortexHeadset.textField(headsets{index}, 'status'));
                end
            end
        end

        function headset = findHeadset(obj, headsetId)
            headset = [];
            for index = 1:numel(obj.context.headsets)
                candidate = obj.context.headsets{index};
                if isfield(candidate, 'id') && strcmp(candidate.id, headsetId)
                    headset = candidate;
                    return;
                end
            end
        end

        function found = waitForHeadset(obj, headsetId, timeoutSeconds)
            % Keeps scanning until headsetId shows up in the list or the timeout expires.
            startedAt = tic();
            found = false;
            while true
                if ~isempty(obj.findHeadset(headsetId))
                    found = true;
                    return;
                end
                if toc(startedAt) >= timeoutSeconds, return; end
                obj.update(0.5);
            end
        end

        function [ok, message] = connectToHeadset(obj, headsetId, streams)
            % Connects the headset if needed, creates an active session, then subscribes to streams.
            if nargin < 3, streams = {}; end
            ok = false;
            message = '';

            try
                headset = obj.findHeadset(headsetId);
                if isempty(headset)
                    message = sprintf('Headset %s is not in the scanned headset list.', headsetId);
                    return;
                end

                if ~strcmp(headset.status, 'connected')
                    if ~strcmp(headset.status, 'connecting')
                        fprintf('[CortexHeadset] Connecting to %s...\n', headsetId);
                        obj.client.sendRequest('controlDevice', ...
                            struct('command', 'connect', 'headset', headsetId));
                    end
                    if isempty(obj.client.waitForWarning(104, obj.connectTimeout))
                        message = sprintf(['Connection to %s was not successful: warning 104 ' ...
                            '(headset connected) was not received within %g seconds.'], ...
                            headsetId, obj.connectTimeout);
                        return;
                    end
                end

                obj.isScanning = false;
                obj.createSession(headsetId);
                if ~isempty(streams)
                    obj.subscribe(streams);
                end
                ok = true;
            catch failure
                message = failure.message;
            end
        end

        function sessionId = createSession(obj, headsetId)
            fprintf('[CortexHeadset] Creating an active session for %s...\n', headsetId);
            params = struct('cortexToken', obj.context.token, 'status', 'active', 'headset', headsetId);
            response = obj.client.sendRequest('createSession', params);
            sessionId = response.result.id;
            obj.context.sessionId = sessionId;
            obj.context.headsetId = headsetId;
        end

        function subscribe(obj, streams)
            if isempty(obj.context.sessionId)
                error('CortexHeadset:noSession', 'Create a session before subscribing.');
            end

            params = struct('cortexToken', obj.context.token, 'session', obj.context.sessionId, ...
                'streams', {streams});
            response = obj.client.sendRequest('subscribe', params);

            failures = CortexHeadset.toCell(response.result.failure);
            for index = 1:numel(failures)
                item = failures{index};
                fprintf('[CortexHeadset] Subscribe failed for %s (code %s): %s\n', ...
                    CortexHeadset.textField(item, 'streamName'), ...
                    CortexHeadset.textField(item, 'code'), CortexHeadset.textField(item, 'message'));
            end

            successes = CortexHeadset.toCell(response.result.success);
            for index = 1:numel(successes)
                item = successes{index};
                name = item.streamName;
                labels = CortexStreamParser.flattenLabels(item.cols);
                obj.parser.register(item.sid, name);
                obj.context.streamLabels.(name) = labels;
                obj.context.subscribedStreams{end + 1} = name;
                obj.context.buffer.configure(name, labels);
                fprintf('[CortexHeadset] Subscribed to %s (%d columns).\n', name, numel(labels));
            end
            obj.isSubscribed = ~isempty(successes);
            if ~obj.isSubscribed
                error('CortexHeadset:subscribe', 'Cortex did not accept any data stream.');
            end
        end

        function unsubscribe(obj)
            if ~obj.isSubscribed, return; end
            params = struct('cortexToken', obj.context.token, 'session', obj.context.sessionId, ...
                'streams', {obj.context.subscribedStreams});
            obj.isSubscribed = false;
            obj.parser.reset();
            obj.context.subscribedStreams = {};
            obj.client.sendRequest('unsubscribe', params);
        end

        function update(obj, maxSeconds)
            % Reads incoming messages into the buffer for about maxSeconds, then runs the scan timer.
            % It can block up to 1 s when no data is arriving.
            if nargin < 2, maxSeconds = 0.25; end
            startedAt = tic();
            while true
                stream = obj.client.readStreamData();
                if isempty(stream), break; end
                obj.storeSample(stream);
                if toc(startedAt) >= maxSeconds, break; end
            end
            obj.runScanTimer();
        end
    end

    methods (Access = private)
        function refreshHeadsets(obj)
            % Cortex answers with warning 142 when the scan finishes.
            obj.scanFinished = false;
            obj.client.sendRequest('controlDevice', struct('command', 'refresh'));
        end

        function onScanFinished(obj, warning)
            obj.scanFinished = true;
            if ~obj.autoRescan
                fprintf(['[CortexHeadset] Headset scan finished (warning 142). ' ...
                    'Call scanHeadset again to rescan.\n']);
            end
        end

        function runScanTimer(obj)
            if ~obj.isScanning, return; end
            if obj.autoRescan && obj.scanFinished
                obj.refreshHeadsets();
            end
            if isempty(obj.lastQueryAt) || toc(obj.lastQueryAt) >= obj.queryInterval
                obj.queryHeadsets();
            end
        end

        function storeSample(obj, msg)
            [stream, row, time] = obj.parser.parse(msg);
            if isempty(stream), return; end
            obj.context.buffer.append(stream, row, time);
        end
    end

    methods (Static, Access = private)
        function items = toCell(value)
            % jsondecode returns a struct array or a cell array depending on the content.
            if isempty(value)
                items = {};
            elseif iscell(value)
                items = value(:)';
            elseif isstruct(value)
                items = num2cell(value(:))';
            else
                items = {value};
            end
        end
    end

    methods (Static, Access = private)
        function text = textField(item, name)
            text = '?';
            if isstruct(item) && isfield(item, name)
                value = item.(name);
                if isnumeric(value), value = num2str(value); end
                text = char(value);
            end
        end
    end
end
