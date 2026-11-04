function [cortex, result] = cortexProfile(cortex, action, profileName)
% cortexProfile  Load, save or unload a training profile for the active headset.
%   [cortex, result] = cortexProfile(cortex, 'load', profileName) loads the profile and
%   creates it first when it does not exist yet.
%   [cortex, result] = cortexProfile(cortex, 'save') saves the loaded profile.
%   [cortex, result] = cortexProfile(cortex, 'unload') unloads the loaded profile.
%   result fields: name, uuid ('' when Cortex gives none), existed, created and isLoaded.
%   cortex.isProfileLoaded and cortex.profileName are updated. Call cortexSession first.

result = [];
try
    if ~isstruct(cortex) || ~isfield(cortex, 'isConnected') || ~cortex.isConnected || ...
            ~isfield(cortex, 'manager') || isempty(cortex.manager)
        error('cortex:profileFailed', 'Connect to Cortex before using profile operations.');
    end
    if ~ischar(action)
        error('cortex:profileFailed', 'action must be ''load'', ''save'' or ''unload''.');
    end

    profile = cortex.manager.getProfile();
    switch lower(action)
        case 'load'
            if nargin < 3
                error('cortex:profileFailed', 'Load requires a profile name.');
            end
            result = profile.loadOrCreate(profileName);
        case 'save'
            result = profile.save();
        case 'unload'
            result = profile.unload();
        otherwise
            error('cortex:profileFailed', 'action must be ''load'', ''save'' or ''unload''.');
    end

    context = cortex.manager.getContext();
    cortex.isProfileLoaded = context.isProfileLoaded;
    cortex.profileName = context.profileName;
catch failure
    if strcmp(failure.identifier, 'cortex:profileFailed'), rethrow(failure); end
    error('cortex:profileFailed', 'Profile operation failed: %s', failure.message);
end
end
