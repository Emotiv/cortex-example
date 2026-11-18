classdef CortexHeadset < handle
    % CortexHeadset  Scans for headsets, connects one and opens a session, optionally subscribing to streams.
    %   The client is single-threaded, so the scan "timer" runs inside update(), which also reads
    %   the incoming data samples into the context buffer.

    properties
        connectTimeout = 10      % seconds to wait for warning 104 (headset connected)
    end

    properties (SetAccess = private)
        isScanning = false
        isSubscribed = false
        autoRescan = true
        queryInterval = 3
    end

    properties (Constant, Access = private)
        flexMapping = struct('CMS', 'TP8', 'DRL', 'P6', ...
            'RM', 'TP10', 'RN', 'P4', 'RO', 'P8')
    end

    properties (Access = private)
        client
        context
        scanFinished = false
        lastQueryAt = []
        parser
        reconnectHeadsetId = ''
        reconnectStreams = {}
        reconnectMappings = []
        reconnectProfileName = ''
        reconnectProfileWasLoaded = false
        reconnectPending = false
        reconnectSessionReady = false
    end

    methods
        function obj = CortexHeadset(client, context)
            obj.client = client;
            obj.context = context;
            obj.parser = CortexStreamParser();
            if isempty(context.buffer)
                context.buffer = CortexDataBuffer(10000);
            end
            obj.client.onWarning(0, @(warning) obj.onSessionClosed(warning));
            obj.client.onWarning(1, @(warning) obj.onSessionClosed(warning));
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

        function [ok, message] = connectToHeadset(obj, headsetId, streams, mappings)
            % Connects the headset if needed, creates an active session, and subscribes when streams are provided.
            if nargin < 3, streams = {}; end
            if nargin < 4, mappings = []; end
            ok = false;
            message = '';

            try
                headset = obj.findHeadset(headsetId);
                if isempty(headset)
                    message = sprintf('Headset %s is not in the scanned headset list.', headsetId);
                    return;
                end

                isFlex = ~isempty(strfind(upper(headsetId), 'FLEX'));
                if isFlex && isempty(mappings)
                    mappings = CortexHeadset.flexMapping;
                elseif ~isFlex && ~isempty(mappings)
                    error('CortexHeadset:mappings', 'mappings can only be supplied for an EPOC Flex headset.');
                end
                obj.reconnectMappings = mappings;

                if ~strcmp(headset.status, 'connected')
                    if ~strcmp(headset.status, 'connecting')
                        fprintf('[CortexHeadset] Connecting to %s...\n', headsetId);
                        params = struct('command', 'connect', 'headset', headsetId);
                        if isFlex
                            params.mappings = mappings;
                        end
                        obj.client.sendRequest('controlDevice', params);
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
            obj.reconnectHeadsetId = headsetId;
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
            obj.reconnectStreams = obj.context.subscribedStreams;
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

        function onSessionClosed(obj, warning)
            if ~isfield(warning, 'message') || ~isstruct(warning.message) || ...
                    ~isfield(warning.message, 'sessionId') || ...
                    ~strcmp(warning.message.sessionId, obj.context.sessionId)
                return;
            end

            if ~obj.reconnectPending
                obj.reconnectHeadsetId = obj.context.headsetId;
                obj.reconnectStreams = obj.context.subscribedStreams;
                obj.reconnectProfileName = obj.context.profileName;
                obj.reconnectProfileWasLoaded = obj.context.isProfileLoaded;
            end
            obj.reconnectPending = ~isempty(obj.reconnectHeadsetId);
            obj.reconnectSessionReady = false;
            obj.isScanning = obj.reconnectPending;
            obj.autoRescan = true;
            obj.scanFinished = obj.reconnectPending;
            obj.lastQueryAt = [];
            obj.isSubscribed = false;
            obj.context.sessionId = '';
            obj.context.headsetId = '';
            obj.context.subscribedStreams = {};
            obj.context.streamLabels = struct();
            obj.context.isRecording = false;
            obj.context.recordId = '';
            obj.context.profileName = '';
            obj.context.isProfileLoaded = false;
            obj.context.detectionInfo = struct();
            obj.context.trainingEvents = {};
            obj.parser.reset();
            fprintf('[CortexHeadset] Session %s was closed by Cortex.\n', warning.message.sessionId);
            if obj.reconnectPending
                fprintf('[CortexHeadset] Scanning to reconnect %s...\n', obj.reconnectHeadsetId);
            end
        end

        function runScanTimer(obj)
            if ~obj.isScanning, return; end
            if obj.autoRescan && obj.scanFinished
                obj.refreshHeadsets();
            end
            if isempty(obj.lastQueryAt) || toc(obj.lastQueryAt) >= obj.queryInterval
                obj.queryHeadsets();
                if obj.reconnectPending
                    obj.reconnectIfAvailable();
                end
            end
        end

        function reconnectIfAvailable(obj)
            headset = obj.findHeadset(obj.reconnectHeadsetId);
            if isempty(headset), return; end

            if ~obj.reconnectSessionReady
                [ok, message] = obj.connectToHeadset( ...
                    obj.reconnectHeadsetId, {}, obj.reconnectMappings);
                if ~ok
                    fprintf('[CortexHeadset] Reconnect attempt failed: %s\n', message);
                    obj.isScanning = true;
                    obj.lastQueryAt = tic();
                    return;
                end
                obj.reconnectSessionReady = true;
            end

            try
                if obj.reconnectProfileWasLoaded
                    profile = CortexProfile(obj.client, obj.context);
                    profile.loadOrCreate(obj.reconnectProfileName);
                end
                if ~isempty(obj.reconnectStreams)
                    obj.subscribe(obj.reconnectStreams);
                end
                obj.reconnectPending = false;
                obj.reconnectSessionReady = false;
                obj.isScanning = false;
                fprintf('[CortexHeadset] Reconnected to %s.\n', obj.reconnectHeadsetId);
            catch failure
                fprintf('[CortexHeadset] Could not restore the session: %s\n', failure.message);
                obj.isScanning = true;
                obj.lastQueryAt = tic();
            end
        end

        function storeSample(obj, msg)
            [stream, row, time, bciAction] = obj.parser.parse(msg);
            if isempty(stream), return; end
            if strcmp(stream, 'sys')
                obj.storeSystemEvent(msg.sys, time);
                return;
            end
            obj.context.buffer.append(stream, row, time, bciAction);
        end

        function storeSystemEvent(obj, values, time)
            % A sys sample is {detection, event}, for example {'mentalCommand', 'MC_Started'}.
            names = CortexStreamParser.stringsOf(values);
            if numel(names) < 2, return; end
            obj.context.trainingEvents{end + 1} = ...
                struct('detection', names{1}, 'event', names{2}, 'time', time);
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
