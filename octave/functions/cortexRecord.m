function [cortex, result] = cortexRecord(cortex, action, varargin)
% cortexRecord  Start or stop a record for the active Cortex session.
%   [cortex, result] = cortexRecord(cortex, 'start', title, description, tags)
%   [cortex, result] = cortexRecord(cortex, 'stop')
%   Starting updates cortex.isRecording and cortex.recordId. The result is the
%   Cortex result struct: record and sessionId.

result = [];
try
    if ~isstruct(cortex) || ~isfield(cortex, 'isConnected') || ~cortex.isConnected || ...
            ~isfield(cortex, 'manager') || isempty(cortex.manager)
        error('cortex:recordFailed', 'Connect to Cortex before using record operations.');
    end
    if ~ischar(action)
        error('cortex:recordFailed', 'action must be ''start'' or ''stop''.');
    end

    record = cortex.manager.getRecord();
    switch lower(action)
        case 'start'
            if isempty(varargin) || numel(varargin) > 3
                error('cortex:recordFailed', 'Start requires title and accepts optional description and tags.');
            end
            title = varargin{1};
            description = '';
            tags = {};
            if numel(varargin) >= 2, description = varargin{2}; end
            if numel(varargin) >= 3, tags = varargin{3}; end
            result = record.create(title, description, tags);
        case 'stop'
            if ~isempty(varargin)
                error('cortex:recordFailed', 'Stop does not accept additional arguments.');
            end
            result = record.stop();
        otherwise
            error('cortex:recordFailed', 'action must be ''start'' or ''stop''.');
    end

    context = cortex.manager.getContext();
    cortex.isRecording = context.isRecording;
    cortex.recordId = context.recordId;
catch failure
    if strcmp(failure.identifier, 'cortex:recordFailed'), rethrow(failure); end
    error('cortex:recordFailed', 'Record operation failed: %s', failure.message);
end
end

