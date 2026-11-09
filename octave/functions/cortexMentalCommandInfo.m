function config = cortexMentalCommandInfo(cortex, action, settings)
% cortexMentalCommandInfo  Get or configure the mental command settings of the loaded profile.
%   config = cortexMentalCommandInfo(cortex, 'get')
%   config = cortexMentalCommandInfo(cortex, 'configure', settings)
%   settings is a struct with any of these fields:
%     activeActions        cell array of actions to activate (at most 4 besides neutral)
%     actionSensitivities  up to 4 values from 1 to 10, in the order of the active actions
%   config fields: currentTrainingThreshold, possibleActions, totalTrainingCount,
%   trainingDuration and trainedActions (cell array of structs with action, active,
%   sensitivity ([] when not applicable) and trainingCount). 'configure' saves the profile,
%   then returns the configuration read back after the change.
%   Requires cortexSession and cortexProfile(cortex, 'load', ...).

try
    if ~isstruct(cortex) || ~isfield(cortex, 'isConnected') || ~cortex.isConnected || ...
            ~isfield(cortex, 'manager') || isempty(cortex.manager)
        error('cortex:mentalCommandFailed', 'Connect to Cortex before using mental command operations.');
    end
    if ~ischar(action)
        error('cortex:mentalCommandFailed', 'action must be ''get'' or ''configure''.');
    end

    mentalCommand = cortex.manager.getMentalCommandConfiguration();
    switch lower(action)
        case 'get'
            config = mentalCommand.get();
        case 'configure'
            if nargin < 3 || ~isstruct(settings)
                error('cortex:mentalCommandFailed', 'Configure requires a settings struct.');
            end
            unknown = setdiff(fieldnames(settings), {'activeActions', 'actionSensitivities'});
            if ~isempty(unknown)
                error('cortex:mentalCommandFailed', ...
                    'Unknown setting "%s". Supported: activeActions, actionSensitivities.', unknown{1});
            end
            activeActions = [];
            sensitivities = [];
            if isfield(settings, 'activeActions'), activeActions = settings.activeActions; end
            if isfield(settings, 'actionSensitivities'), sensitivities = settings.actionSensitivities; end
            mentalCommand.set(activeActions, sensitivities);
            cortex.manager.getProfile().save();
            config = mentalCommand.get();
        otherwise
            error('cortex:mentalCommandFailed', 'action must be ''get'' or ''configure''.');
    end
catch failure
    if strcmp(failure.identifier, 'cortex:mentalCommandFailed'), rethrow(failure); end
    error('cortex:mentalCommandFailed', 'Mental command operation failed: %s', failure.message);
end
end
