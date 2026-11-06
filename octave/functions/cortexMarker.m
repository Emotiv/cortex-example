function [cortex, result] = cortexMarker(cortex, action, varargin)
% cortexMarker  Inject an instance marker or update it to an interval marker.
%   [cortex, result] = cortexMarker(cortex, 'inject', label, value, ...)
%   Optional inject name/value pairs: time (epoch milliseconds), port, extras.
%   [cortex, result] = cortexMarker(cortex, 'update', markerId, endTime, ...)
%   Optional update name/value pair: extras. Markers require an active record.

result = [];
try
    if ~isstruct(cortex) || ~isfield(cortex, 'isConnected') || ~cortex.isConnected || ...
            ~isfield(cortex, 'isRecording') || ~cortex.isRecording || ...
            ~isfield(cortex, 'manager') || isempty(cortex.manager)
        error('cortex:markerFailed', 'Start a record before using marker operations.');
    end
    if ~ischar(action)
        error('cortex:markerFailed', 'action must be ''inject'' or ''update''.');
    end

    marker = cortex.manager.getMarker();
    switch lower(action)
        case 'inject'
            if numel(varargin) < 2
                error('cortex:markerFailed', 'Inject requires a label and value.');
            end
            label = varargin{1};
            value = varargin{2};
            options = parseOptions(varargin(3:end), struct('time', [], 'port', 'Software', 'extras', struct()));
            result = marker.inject(label, value, options.time, options.port, options.extras);
        case 'update'
            if numel(varargin) < 2
                error('cortex:markerFailed', 'Update requires a markerId and endTime.');
            end
            markerId = varargin{1};
            endTime = varargin{2};
            options = parseOptions(varargin(3:end), struct('extras', struct()));
            result = marker.update(markerId, endTime, options.extras);
        otherwise
            error('cortex:markerFailed', 'action must be ''inject'' or ''update''.');
    end
catch failure
    if strcmp(failure.identifier, 'cortex:markerFailed'), rethrow(failure); end
    error('cortex:markerFailed', 'Marker operation failed: %s', failure.message);
end
end

function options = parseOptions(arguments, options)
if mod(numel(arguments), 2) ~= 0
    error('cortex:markerFailed', 'Marker options must be supplied as name/value pairs.');
end
for index = 1:2:numel(arguments)
    name = arguments{index};
    if ~ischar(name) || ~isfield(options, name)
        error('cortex:markerFailed', 'Unknown marker option.');
    end
    options.(name) = arguments{index + 1};
end
end

