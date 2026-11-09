classdef CortexTraining < handle
    % CortexTraining  Trains mental command or facial expression actions on the loaded profile.
    %   Training progress arrives as events on the "sys" stream, so subscribe to it to wait for results.

    properties (Access = private)
        client
        context
        headset
        profile
    end

    methods
        function obj = CortexTraining(client, context, headset, profile)
            obj.client = client;
            obj.context = context;
            obj.headset = headset;
            obj.profile = profile;
        end

        function result = train(obj, detection, action, status, waitSeconds)
            if nargin < 5, waitSeconds = 0; end
            detection = CortexTraining.normalizeDetection(detection);
            if isempty(obj.context.sessionId)
                error('CortexTraining:noSession', 'Create a headset session before training.');
            end
            if ~obj.context.isProfileLoaded
                error('CortexTraining:noProfile', 'Load a profile before training.');
            end
            if ~isnumeric(waitSeconds) || ~isscalar(waitSeconds) || waitSeconds < 0
                error('CortexTraining:wait', 'wait must be a non-negative number of seconds.');
            end

            info = obj.context.detectionInfo.(detection);
            action = CortexTraining.canonical(info.actions, action, 'action', detection);
            status = CortexTraining.canonical(info.controls, status, 'status', detection);

            finalEvents = {};
            switch status
                case 'start'
                    finalEvents = {'Succeeded', 'Failed'};
                case 'accept'
                    finalEvents = {'Completed'};
            end
            shouldWait = waitSeconds > 0 && ~isempty(finalEvents);
            if shouldWait && ~any(strcmp(obj.context.subscribedStreams, 'sys'))
                error('CortexTraining:noSysStream', 'Subscribe to the sys stream to wait for training events.');
            end

            obj.context.trainingEvents = {};
            params = struct('cortexToken', obj.context.token, 'detection', detection, ...
                'session', obj.context.sessionId, 'action', action, 'status', status);
            obj.client.sendRequest('training', params);

            result = struct('detection', detection, 'action', action, 'status', status, ...
                'event', '', 'isSucceeded', false, 'profileSaved', false);
            if ~shouldWait, return; end

            event = obj.waitForEvent(finalEvents, waitSeconds);
            if isempty(event)
                error('CortexTraining:timeout', 'No %s %s event within %g seconds.', ...
                    detection, strjoin(finalEvents, '/'), waitSeconds);
            end
            result.event = event;
            result.isSucceeded = ~isempty(regexp(event, '_(Succeeded|Completed)$', 'once'));

            if strcmp(status, 'accept') && result.isSucceeded
                try
                    obj.profile.save();
                    result.profileSaved = true;
                catch err
                    fprintf('[CortexTraining] Training completed but saving the profile failed: %s\n', err.message);
                end
            end
        end
    end

    methods (Access = private)
        function event = waitForEvent(obj, suffixes, waitSeconds)
            event = '';
            pattern = ['_(' strjoin(suffixes, '|') ')$'];
            startedAt = tic();
            seen = 0;
            while toc(startedAt) < waitSeconds
                obj.headset.update(0.25);
                events = obj.context.trainingEvents;
                for index = seen + 1:numel(events)
                    if ~isempty(regexp(events{index}.event, pattern, 'once'))
                        event = events{index}.event;
                        return;
                    end
                end
                seen = numel(events);
            end
        end
    end

    methods (Static, Access = private)
        function detection = normalizeDetection(detection)
            if ~ischar(detection)
                error('CortexTraining:detection', 'detection must be ''mentalCommand'' or ''facialExpression''.');
            end
            switch lower(detection)
                case 'mentalcommand'
                    detection = 'mentalCommand';
                case 'facialexpression'
                    detection = 'facialExpression';
                otherwise
                    error('CortexTraining:detection', ...
                        'detection must be ''mentalCommand'' or ''facialExpression''.');
            end
        end

        function value = canonical(allowed, value, kind, detection)
            if ~ischar(value) || isempty(value)
                error('CortexTraining:argument', '%s must be a non-empty character vector.', kind);
            end
            index = find(strcmpi(allowed, value), 1);
            if isempty(index)
                error('CortexTraining:argument', 'Unsupported %s "%s" for %s. Supported: %s.', ...
                    kind, value, detection, strjoin(allowed, ', '));
            end
            value = allowed{index};
        end
    end
end
