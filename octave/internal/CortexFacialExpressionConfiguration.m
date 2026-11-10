classdef CortexFacialExpressionConfiguration < handle
    % CortexFacialExpressionConfiguration  Reads the facial expression configuration of the loaded profile.

    properties (Access = private)
        client
        context
    end

    methods
        function obj = CortexFacialExpressionConfiguration(client, context)
            obj.client = client;
            obj.context = context;
        end

        function config = get(obj)
            if isempty(obj.context.sessionId)
                error('CortexFacialExpressionConfiguration:noSession', 'Create a headset session first.');
            end
            if ~obj.context.isProfileLoaded
                error('CortexFacialExpressionConfiguration:noProfile', 'Load a profile first.');
            end
            token = obj.context.token;
            session = obj.context.sessionId;
            detection = 'facialExpression';

            info = obj.context.detectionInfo.(detection);
            signature = obj.client.sendRequest('getTrainedSignatureActions', ...
                struct('cortexToken', token, 'detection', detection, 'session', session)).result;
            duration = obj.client.sendRequest('getTrainingTime', ...
                struct('cortexToken', token, 'detection', detection, 'session', session)).result;

            trained = CortexFacialExpressionConfiguration.toCell( ...
                CortexFacialExpressionConfiguration.field(signature, 'trainedActions', {}));
            trainedActions = cell(1, numel(trained));
            for index = 1:numel(trained)
                name = trained{index}.action;
                % Neutral has no threshold.
                value = [];
                if ~strcmp(name, 'neutral')
                    value = obj.client.sendRequest('facialExpressionThreshold', struct( ...
                        'cortexToken', token, 'session', session, 'action', name, 'status', 'get')).result;
                    if ~isnumeric(value) || isempty(value), value = []; end
                end
                trainedActions{index} = struct('action', name, 'threshold', value, ...
                    'trainingCount', CortexFacialExpressionConfiguration.field(trained{index}, 'times', 0));
            end

            config = struct( ...
                'possibleActions', {info.actions}, ...
                'trainedActions', {trainedActions}, ...
                'trainingDuration', CortexFacialExpressionConfiguration.field(duration, 'time', []));
        end
    end

    methods (Static, Access = private)
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
