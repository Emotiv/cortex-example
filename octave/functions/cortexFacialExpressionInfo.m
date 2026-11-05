function config = cortexFacialExpressionInfo(cortex)
% cortexFacialExpressionInfo  Get the facial expression settings of the loaded profile.
%   config = cortexFacialExpressionInfo(cortex)
%   config fields:
%     possibleActions   cell array of every action Cortex supports for facial expression
%     trainedActions    cell array of structs with action, threshold (0 to 1000, [] for neutral)
%                       and trainingCount
%     trainingDuration  seconds a training lasts
%   Requires cortexSession and cortexProfile(cortex, 'load', ...).

try
    if ~isstruct(cortex) || ~isfield(cortex, 'isConnected') || ~cortex.isConnected || ...
            ~isfield(cortex, 'manager') || isempty(cortex.manager)
        error('cortex:facialExpressionFailed', 'Connect to Cortex before using facial expression operations.');
    end
    config = cortex.manager.getFacialExpressionConfiguration().get();
catch failure
    if strcmp(failure.identifier, 'cortex:facialExpressionFailed'), rethrow(failure); end
    error('cortex:facialExpressionFailed', 'Facial expression operation failed: %s', failure.message);
end
end
