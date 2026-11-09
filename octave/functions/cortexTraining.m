function result = cortexTraining(cortex, detection, action, status, varargin)
% cortexTraining  Train an action of a detection on the loaded profile.
%   result = cortexTraining(cortex, 'mentalCommand', 'push', 'start', 'wait', 30)
%   result = cortexTraining(cortex, 'mentalCommand', 'push', 'accept', 'wait', 15)
%   detection  'mentalCommand' ('facialExpression' uses the same call)
%   action     any action Cortex lists for the detection, for example neutral, push, left, right
%   status     start, accept, reject, reset or erase
%   'wait', seconds  block until Cortex reports the outcome (needs the sys stream):
%                    start waits for MC_Succeeded or MC_Failed, accept waits for MC_Completed.
%   Accepting a completed training also saves the profile.
%   result fields: detection, action, status, event ('' without wait), isSucceeded, profileSaved.
%   Requires cortexSession, cortexProfile(cortex, 'load', ...) and cortexSubscribe(cortex, {'sys'}).

try
    if ~isstruct(cortex) || ~isfield(cortex, 'isConnected') || ~cortex.isConnected || ...
            ~isfield(cortex, 'manager') || isempty(cortex.manager)
        error('cortex:trainingFailed', 'Connect to Cortex before training.');
    end
    if ~ischar(detection) || ~ischar(action) || ~ischar(status)
        error('cortex:trainingFailed', 'detection, action and status must be character vectors.');
    end
    if mod(numel(varargin), 2) ~= 0
        error('cortex:trainingFailed', 'Options must be supplied as name/value pairs.');
    end

    waitSeconds = 0;
    for index = 1:2:numel(varargin)
        if ~ischar(varargin{index}) || ~strcmp(varargin{index}, 'wait')
            error('cortex:trainingFailed', 'Unknown training option. Supported: wait.');
        end
        waitSeconds = varargin{index + 1};
    end

    result = cortex.manager.getTraining().train(detection, action, status, waitSeconds);
catch failure
    if strcmp(failure.identifier, 'cortex:trainingFailed'), rethrow(failure); end
    error('cortex:trainingFailed', 'Training operation failed: %s', failure.message);
end
end
