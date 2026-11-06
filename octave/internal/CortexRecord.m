classdef CortexRecord < handle
    % CortexRecord  Creates and stops records for the active Cortex session.

    properties (Access = private)
        client
        context
    end

    methods
        function obj = CortexRecord(client, context)
            obj.client = client;
            obj.context = context;
        end

        function result = create(obj, title, description, tags)
            if isempty(obj.context.sessionId)
                error('CortexRecord:noSession', 'Create an active session before starting a record.');
            end
            if obj.context.isRecording
                error('CortexRecord:alreadyRecording', 'Stop the active record before starting another.');
            end
            if ~ischar(title) || isempty(title) || numel(title) > 200
                error('CortexRecord:title', 'title must be a non-empty character vector of at most 200 characters.');
            end
            if ~ischar(description)
                error('CortexRecord:description', 'description must be a character vector.');
            end
            if ischar(tags), tags = {tags}; end
            if ~iscellstr(tags)
                error('CortexRecord:tags', 'tags must be a character vector or a cell array of character vectors.');
            end

            params = struct('cortexToken', obj.context.token, 'session', obj.context.sessionId, ...
                'title', title);
            if ~isempty(description), params.description = description; end
            if ~isempty(tags), params.tags = tags; end

            response = obj.client.sendRequest('createRecord', params);
            result = response.result;
            if ~isfield(result, 'record') || ~isfield(result.record, 'uuid')
                error('CortexRecord:response', 'Cortex did not return the created record id.');
            end

            obj.context.recordId = char(result.record.uuid);
            obj.context.isRecording = true;
        end

        function result = stop(obj)
            if ~obj.context.isRecording || isempty(obj.context.sessionId)
                error('CortexRecord:notRecording', 'There is no active record to stop.');
            end

            params = struct('cortexToken', obj.context.token, 'session', obj.context.sessionId);
            response = obj.client.sendRequest('stopRecord', params);
            result = response.result;
            obj.context.isRecording = false;
            obj.context.recordId = '';
        end
    end
end