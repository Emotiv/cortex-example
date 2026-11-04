function mentalCommandTraining()
% mentalCommandTraining  Train mental command actions from the keyboard and watch live commands.
%   Set headsetId and profileName below, then call mentalCommandTraining().
%   Keys (focus the figure): N neutral, P push, L left, D drop start a training;
%   A accept or R reject the training result; S show live commands;
%   U (or Esc) unload the profile and exit. Train neutral first, then at least one action.
%
%   The com stream sends one sample per update with two values:
%     act  the detected action name (for example 'neutral' or 'push')
%     pow  the action power from 0 to 1
%   It is a label plus a strength rather than a waveform, so it is shown as text instead of a plot.
%   To train other actions, add a key that calls trainAction with another action name
%   (see getDetectionInfo in the Cortex API for the supported list).

% Set your headset id and the name of the training profile to create or load.
headsetId = '<YOUR_HEADSET_ID>';
profileName = 'my-mental-command';

trainStartTimeout = 10;
trainAcceptTimeout = 15;

currentDir = fileparts(mfilename('fullpath'));
addpath(fullfile(currentDir, '../functions'));

% Connect: authorize, connect the headset, create its session and load (or create) the profile.
cortex = cortexConnect();
fig = [];
statusText = [];
commandText = [];
powerAxes = [];
powerBar = [];
powerLabel = [];
isBusy = false;
isExitRequested = false;
isCommandStreaming = false;
pendingAction = '';
lastAction = '';
lastPower = NaN;
connectionCleanup = onCleanup(@cleanupConnection);
try
    if ~cortex.isConnected
        error('mentalCommandTraining:connect', 'Cortex connection or authorization failed.');
    end
    cortex = cortexSession(cortex, headsetId);
    [cortex, profileResult] = cortexProfile(cortex, 'load', profileName);
    if profileResult.created
        fprintf('[mentalCommandTraining] Created and loaded profile %s.\n', profileName);
    else
        fprintf('[mentalCommandTraining] Loaded existing profile %s.\n', profileName);
    end

    mentalCommandInfo = cortexMentalCommandInfo(cortex, 'get');
    fprintf('[mentalCommandTraining] Mental command info:\n%s\n', ...
        jsonencode(mentalCommandInfo, 'PrettyPrint', true));

    % The sys stream reports the training events (started, succeeded, completed).
    cortex = cortexSubscribe(cortex, {'sys'});
    if ~cortex.isSubscribed
        error('mentalCommandTraining:subscribe', 'Could not subscribe to the sys stream.');
    end

    fprintf('[mentalCommandTraining] Key help:\n');
    fprintf('  N / P / L / D: start training neutral / push / left / drop.\n');
    fprintf('  A / R: accept / reject the training result.\n');
    fprintf('  S: show live mental commands (act and pow).\n');
    fprintf('  C: set sensitivity push 7, left 8, drop 2.\n');
    fprintf('  U or Esc: unload the profile and exit.\n');

    fig = figure('Name', 'Cortex Mental Command Training', 'NumberTitle', 'off', ...
        'KeyPressFcn', @onKeyPress);
    helpLines = {'Keyboard controls', '', ...
        'N: train neutral (do this first)', ...
        'P: train push', ...
        'L: train left', ...
        'D: train drop', ...
        'A: accept the training result (saves the profile)', ...
        'R: reject the training result', ...
        'S: show live mental commands (act = action, pow = power 0-1)', ...
        'C: set sensitivity (push 7, left 8, drop 2)', ...
        'U or Esc: unload the profile and exit'};
    uicontrol(fig, 'Style', 'text', 'Units', 'normalized', ...
        'Position', [0.08 0.44 0.84 0.52], 'String', helpLines, ...
        'HorizontalAlignment', 'left', 'FontSize', 11, ...
        'BackgroundColor', get(fig, 'Color'), 'Enable', 'on', 'KeyPressFcn', @onKeyPress);
    commandText = uicontrol(fig, 'Style', 'text', 'Units', 'normalized', ...
        'Position', [0.08 0.28 0.40 0.12], 'String', '', ...
        'HorizontalAlignment', 'left', 'FontSize', 20, ...
        'BackgroundColor', get(fig, 'Color'), 'Enable', 'on', 'KeyPressFcn', @onKeyPress);
    powerAxes = axes('Parent', fig, 'Units', 'normalized', 'Position', [0.52 0.29 0.40 0.08], ...
        'XLim', [0 1], 'YLim', [0 1], 'XTick', [], 'YTick', [], 'Box', 'on', 'Visible', 'off');
    powerBar = patch('Parent', powerAxes, 'Faces', [1 2 3 4], ...
        'Vertices', [0 0; 0 0; 0 1; 0 1], 'FaceColor', [0.8 0.2 0.1], 'EdgeColor', 'none', ...
        'Visible', 'off');
    powerLabel = text('Parent', powerAxes, 'Position', [0.5 0.5], 'String', '', ...
        'HorizontalAlignment', 'center', 'Visible', 'off');
    statusText = uicontrol(fig, 'Style', 'text', 'Units', 'normalized', ...
        'Position', [0.08 0.02 0.84 0.20], ...
        'String', ['Profile ' profileName ' ready. Press N to train neutral.'], ...
        'HorizontalAlignment', 'left', 'FontSize', 10, ...
        'BackgroundColor', get(fig, 'Color'), 'Enable', 'on', 'KeyPressFcn', @onKeyPress);

    % Acquire: poll keyboard events and, once subscribed, the live mental command stream.
    while ~isExitRequested && ishandle(fig)
        if isCommandStreaming && ~isBusy
            updateCommandDisplay();
        else
            pause(0.05);
        end
        drawnow();
    end
catch failure
    fprintf('[mentalCommandTraining] ERROR %s: %s\n', failure.identifier, failure.message);
end

% Disconnect: unload the profile and close the Cortex connection on every exit path.
clear connectionCleanup;
fprintf('[mentalCommandTraining] Finished.\n');

    function onKeyPress(~, event)
        if isBusy, return; end
        keyName = event.Key;
        if strcmpi(keyName, 'escape') || strcmpi(keyName, 'esc')
            unloadAndExit();
            return;
        end

        keyCharacter = event.Character;
        if isempty(keyCharacter) && numel(keyName) == 1
            keyCharacter = keyName;
        end
        if ~ischar(keyCharacter) || numel(keyCharacter) ~= 1
            showStatus(sprintf('Ignored key: %s.', keyName));
            return;
        end

        % Process: map each key to a training action, live display or exit.
        switch lower(keyCharacter)
            case 'n'
                trainAction('neutral');
            case 'p'
                trainAction('push');
            case 'l'
                trainAction('left');
            case 'd'
                trainAction('drop');
            case 'a'
                finishTraining('accept');
            case 'r'
                finishTraining('reject');
            case 's'
                startCommandStream();
            case 'c'
                configureSensitivity();
            case 'u'
                unloadAndExit();
            otherwise
                showStatus(sprintf('Ignored key: %s. Use N, P, L, D, A, R, S, C, U or Esc.', keyName));
        end
    end

    function trainAction(actionName)
        if ~isempty(pendingAction)
            showStatus(sprintf('Press A to accept or R to reject the %s training first.', pendingAction));
            return;
        end
        isBusy = true;
        try
            showStatus(sprintf('Training %s: hold the mental state until it finishes...', actionName));
            started = cortexTraining(cortex, 'mentalCommand', actionName, 'start', ...
                'wait', trainStartTimeout);
            if ~started.isSucceeded
                fprintf('[mentalCommandTraining] Training %s failed (%s).\n', actionName, started.event);
                showStatus(sprintf('Training %s failed (%s). Press the key to try again.', ...
                    actionName, started.event));
            else
                pendingAction = actionName;
                fprintf('[mentalCommandTraining] Training %s succeeded (%s).\n', actionName, started.event);
                showStatus(sprintf('Training %s succeeded. Press A to accept or R to reject.', actionName));
            end
        catch trainFailure
            fprintf('[mentalCommandTraining] ERROR %s: %s\n', trainFailure.identifier, trainFailure.message);
            showStatus(['Training failed: ' trainFailure.message]);
        end
        isBusy = false;
    end

    function finishTraining(status)
        if isempty(pendingAction)
            showStatus('No training result to accept or reject. Start a training first.');
            return;
        end
        isBusy = true;
        try
            if strcmp(status, 'accept')
                accepted = cortexTraining(cortex, 'mentalCommand', pendingAction, 'accept', ...
                    'wait', trainAcceptTimeout);
                fprintf('[mentalCommandTraining] Training %s completed (%s); profile saved: %d.\n', ...
                    pendingAction, accepted.event, accepted.profileSaved);
                showStatus(sprintf('Training %s accepted and profile saved.', pendingAction));
            else
                cortexTraining(cortex, 'mentalCommand', pendingAction, 'reject');
                fprintf('[mentalCommandTraining] Training %s rejected.\n', pendingAction);
                showStatus(sprintf('Training %s rejected. Press its key to train again.', pendingAction));
            end
            pendingAction = '';
        catch finishFailure
            fprintf('[mentalCommandTraining] ERROR %s: %s\n', finishFailure.identifier, finishFailure.message);
            showStatus(['Could not ' status ' the training: ' finishFailure.message]);
        end
        isBusy = false;
    end

    function configureSensitivity()
        isBusy = true;
        try
            showStatus('Configuring sensitivity...');
            % Sensitivities follow the order of the active actions, so activate them in this order.
            settings = struct('activeActions', {{'neutral', 'push', 'left', 'drop'}}, ...
                'actionSensitivities', [7 8 2]);
            configured = cortexMentalCommandInfo(cortex, 'configure', settings);
            fprintf('[mentalCommandTraining] Mental command info:\n%s\n', ...
                jsonencode(configured, 'PrettyPrint', true));
            showStatus('Sensitivity set: push 7, left 8, drop 2. Profile saved.');
        catch configureFailure
            fprintf('[mentalCommandTraining] ERROR %s: %s\n', configureFailure.identifier, configureFailure.message);
            showStatus(['Could not configure sensitivity: ' configureFailure.message]);
        end
        isBusy = false;
    end

    function startCommandStream()
        if isCommandStreaming
            showStatus('Live commands are already shown.');
            return;
        end
        isBusy = true;
        try
            cortex = cortexSubscribe(cortex, {'com'});
            if ~any(strcmp(cortex.streams, 'com'))
                error('mentalCommandTraining:subscribe', 'Could not subscribe to the com stream.');
            end
            isCommandStreaming = true;
            set([powerAxes powerBar powerLabel], 'Visible', 'on');
            showStatus('Showing live mental commands. Train more actions with N, P, L or D.');
        catch subscribeFailure
            fprintf('[mentalCommandTraining] ERROR %s: %s\n', subscribeFailure.identifier, subscribeFailure.message);
            showStatus(['Subscribe failed: ' subscribeFailure.message]);
        end
        isBusy = false;
    end

    function updateCommandDisplay()
        [labels, data, ~, actions] = cortexGetData(cortex, 'com', 1);
        if isempty(data) || isempty(actions), return; end
        powerColumn = find(strcmp(labels, 'pow'), 1);
        if isempty(powerColumn), return; end

        action = actions{end};
        power = data(end, powerColumn);
        isSamePower = (isnan(power) && isnan(lastPower)) || power == lastPower;
        if strcmp(action, lastAction) && isSamePower, return; end
        if ~strcmp(action, lastAction)
            fprintf('[mentalCommandTraining] Action: %s (pow %.2f)\n', action, power);
        end
        lastAction = action;
        lastPower = power;

        if isnan(power), power = 0; end
        level = max(0, min(1, power));
        % Red (weak) through yellow to green (strong).
        barColor = [min(1, 2 * (1 - level)), min(1, 2 * level), 0.1];
        if ishandle(commandText)
            set(commandText, 'String', action);
        end
        if ishandle(powerBar)
            set(powerBar, 'Vertices', [0 0; level 0; level 1; 0 1], 'FaceColor', barColor);
        end
        if ishandle(powerLabel)
            set(powerLabel, 'String', sprintf('%.2f', power));
        end
    end

    function unloadAndExit()
        isBusy = true;
        showStatus('Unloading the profile, please wait...');
        try
            [cortex, unloadResult] = cortexProfile(cortex, 'unload');
            fprintf('[mentalCommandTraining] Profile %s unloaded.\n', unloadResult.name);
            isExitRequested = true;
        catch unloadFailure
            fprintf('[mentalCommandTraining] ERROR %s: %s\n', unloadFailure.identifier, unloadFailure.message);
            showStatus(['Could not unload the profile: ' unloadFailure.message ' Press U to try again.']);
            isBusy = false;
        end
    end

    function showStatus(message)
        if ~isempty(statusText) && ishandle(statusText)
            set(statusText, 'String', message);
            drawnow();
        end
    end

    function cleanupConnection()
        if ~isempty(fig) && ishandle(fig)
            delete(fig);
        end
        cortex = cortexDisconnect(cortex);
    end
end
