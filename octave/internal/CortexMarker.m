classdef CortexMarker < handle
    % CortexMarker  Injects instance markers and updates them to interval markers.

    properties (Access = private)
        client
        context
    end

    methods
        function obj = CortexMarker(client, context)
            obj.client = client;
            obj.context = context;
        end

        function result = inject(obj, label, value, time, port, extras)
            if nargin < 4 || isempty(time)
                time = double(javaMethod('currentTimeMillis', 'java.lang.System'));
            end
            if nargin < 5 || isempty(port), port = 'Software'; end
            if nargin < 6 || isempty(extras), extras = struct(); end
            obj.validateRecording();
            if ~ischar(label) || isempty(label)
                error('CortexMarker:label', 'label must be a non-empty character vector.');
            end
            if ~(ischar(value) || (isnumeric(value) && isscalar(value) && isfinite(value)))
                error('CortexMarker:value', 'value must be a character vector or finite numeric scalar.');
            end
            if ~isnumeric(time) || ~isscalar(time) || ~isfinite(time) || time < 0
                error('CortexMarker:time', 'time must be a non-negative numeric epoch timestamp in milliseconds.');
            end
            if ~ischar(port) || isempty(port)
                error('CortexMarker:port', 'port must be a non-empty character vector.');
            end
            if ~isstruct(extras) || numel(extras) ~= 1
                error('CortexMarker:extras', 'extras must be a scalar struct.');
            end

            params = struct('cortexToken', obj.context.token, 'session', obj.context.sessionId, ...
                'time', double(time), 'value', value, 'label', label, 'port', port);
            if ~isempty(fieldnames(extras)), params.extras = extras; end

            response = obj.client.sendRequest('injectMarker', params);
            result = response.result;
        end

        function result = update(obj, markerId, endTime, extras)
            if nargin < 4 || isempty(extras), extras = struct(); end
            obj.validateRecording();
            if ~ischar(markerId) || isempty(markerId)
                error('CortexMarker:markerId', 'markerId must be a non-empty character vector.');
            end
            if ~isnumeric(endTime) || ~isscalar(endTime) || ~isfinite(endTime) || endTime < 0
                error('CortexMarker:time', 'endTime must be a non-negative numeric epoch timestamp in milliseconds.');
            end
            if ~isstruct(extras) || numel(extras) ~= 1
                error('CortexMarker:extras', 'extras must be a scalar struct.');
            end

            params = struct('cortexToken', obj.context.token, 'session', obj.context.sessionId, ...
                'markerId', markerId, 'time', double(endTime));
            if ~isempty(fieldnames(extras)), params.extras = extras; end

            response = obj.client.sendRequest('updateMarker', params);
            result = response.result;
        end
    end

    methods (Access = private)
        function validateRecording(obj)
            if isempty(obj.context.sessionId)
                error('CortexMarker:noSession', 'Create an active session before using markers.');
            end
            if ~obj.context.isRecording
                error('CortexMarker:notRecording', 'Markers can only be used while a record is active.');
            end
        end
    end
end