classdef CortexMentalCommandConfiguration < handle
    % CortexMentalCommandConfiguration  Reads and writes the mental command configuration of the loaded profile.

    properties (Access = private)
        client
        context
    end

    methods
        function obj = CortexMentalCommandConfiguration(client, context)
            obj.client = client;
            obj.context = context;
        end

        function config = get(obj)
            obj.requireSession();
            token = obj.context.token;
            session = obj.context.sessionId;
            detection = 'mentalCommand';

            info = obj.context.detectionInfo.(detection);
            signature = obj.client.sendRequest('getTrainedSignatureActions', ...
                struct('cortexToken', token, 'detection', detection, 'session', session)).result;
            active = obj.getActiveActions();
            sensitivity = obj.getSensitivity();
            threshold = obj.client.sendRequest('mentalCommandTrainingThreshold', ...
                struct('cortexToken', token, 'session', session)).result;
            duration = obj.client.sendRequest('getTrainingTime', ...
                struct('cortexToken', token, 'detection', detection, 'session', session)).result;

            sensitiveActions = CortexMentalCommandConfiguration.sensitiveActions(active);
            trained = CortexMentalCommandConfiguration.toCell(CortexMentalCommandConfiguration.field(signature, 'trainedActions', {}));
            trainedActions = cell(1, numel(trained));
            for index = 1:numel(trained)
                name = trained{index}.action;
                value = [];
                position = find(strcmp(sensitiveActions, name), 1);
                if ~isempty(position) && position <= numel(sensitivity)
                    value = sensitivity(position);
                end
                trainedActions{index} = struct('action', name, ...
                    'active', strcmp(name, 'neutral') || any(strcmp(active, name)), ...
                    'sensitivity', value, ...
                    'trainingCount', CortexMentalCommandConfiguration.field(trained{index}, 'times', 0));
            end

            config = struct( ...
                'currentTrainingThreshold', CortexMentalCommandConfiguration.field(threshold, 'currentThreshold', []), ...
                'possibleActions', {info.actions}, ...
                'totalTrainingCount', CortexMentalCommandConfiguration.field(signature, 'totalTimesTraining', 0), ...
                'trainedActions', {trainedActions}, ...
                'trainingDuration', CortexMentalCommandConfiguration.field(duration, 'time', []));
        end

        function set(obj, activeActions, sensitivity)
            % Either argument may be [] to leave it unchanged.
            obj.requireSession();
            token = obj.context.token;
            session = obj.context.sessionId;

            if ~isempty(activeActions)
                if ischar(activeActions), activeActions = {activeActions}; end
                if ~iscellstr(activeActions)
                    error('CortexMentalCommandConfiguration:argument', 'activeActions must be a cell array of action names.');
                end
                info = obj.context.detectionInfo.mentalCommand;
                for index = 1:numel(activeActions)
                    if ~any(strcmp(info.actions, activeActions{index}))
                        error('CortexMentalCommandConfiguration:argument', 'Unsupported action "%s".', activeActions{index});
                    end
                end
                if numel(setdiff(activeActions, {'neutral'})) > 4
                    error('CortexMentalCommandConfiguration:argument', 'At most 4 actions (excluding neutral) can be active.');
                end
                obj.client.sendRequest('mentalCommandActiveAction', struct('cortexToken', token, ...
                    'session', session, 'status', 'set', 'actions', {activeActions(:)'}));
            end

            if ~isempty(sensitivity)
                if ~isnumeric(sensitivity) || numel(sensitivity) > 4 || any(sensitivity < 1 | sensitivity > 10)
                    error('CortexMentalCommandConfiguration:argument', ...
                        'sensitivity must have up to 4 values between 1 and 10, in the order of the active actions.');
                end
                values = obj.getSensitivity();
                values(1:numel(sensitivity)) = sensitivity(:)';
                obj.client.sendRequest('mentalCommandActionSensitivity', struct('cortexToken', token, ...
                    'session', session, 'status', 'set', 'values', values(:)'));
            end
        end
    end

    methods (Access = private)
        function requireSession(obj)
            if isempty(obj.context.sessionId)
                error('CortexMentalCommandConfiguration:noSession', 'Create a headset session first.');
            end
            if ~obj.context.isProfileLoaded
                error('CortexMentalCommandConfiguration:noProfile', 'Load a profile first.');
            end
        end

        function active = getActiveActions(obj)
            response = obj.client.sendRequest('mentalCommandActiveAction', struct( ...
                'cortexToken', obj.context.token, 'session', obj.context.sessionId, 'status', 'get'));
            active = CortexMentalCommandConfiguration.toCell(response.result);
        end

        function values = getSensitivity(obj)
            response = obj.client.sendRequest('mentalCommandActionSensitivity', struct( ...
                'cortexToken', obj.context.token, 'session', obj.context.sessionId, 'status', 'get'));
            values = double(response.result(:)');
            values = [values, 5 * ones(1, max(0, 4 - numel(values)))];
        end
    end

    methods (Static, Access = private)
        function actions = sensitiveActions(active)
            % Sensitivity values follow the active actions; neutral has none, and only 4 are kept.
            actions = active(~strcmp(active, 'neutral'));
            actions = actions(1:min(4, numel(actions)));
        end

        function items = toCell(value)
            if iscell(value)
                items = value(:)';
            elseif isstruct(value)
                items = num2cell(value(:))';
            elseif ischar(value)
                items = {value};
            else
                items = {};
            end
        end

        function value = field(source, name, default)
            if isstruct(source) && isfield(source, name)
                value = source.(name);
            else
                value = default;
            end
        end
    end
end
