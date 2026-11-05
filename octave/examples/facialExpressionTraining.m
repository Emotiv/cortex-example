function facialExpressionTraining()
% facialExpressionTraining  Train facial expression actions from the keyboard and watch live expressions.
%   Set headsetId and profileName below, then call facialExpressionTraining().
%   Keys (focus the figure): N neutral, S surprise, F frown, M smile, C clench start a training;
%   A accept or R reject the training result; L show live expressions;
%   U (or Esc) unload the profile and exit. Train neutral first, then at least one action.
%
%   The fac stream sends one sample per update with five values:
%     eyeAct  the eye action (for example 'neutral' or 'blink')
%     uAct    the upper face action, with its power uPow from 0 to 1
%     lAct    the lower face action, with its power lPow from 0 to 1
%   Only neutral, surprise, frown, smile and clench can be trained.

% Set your headset id and the name of the training profile to create or load.
headsetId = '<YOUR_HEADSET_ID>';
profileName = 'my-facial-expression';

% Allow the action duration plus time for Cortex to deliver FE_Succeeded or FE_Failed.
trainStartEventWaitLimit = 15;
% FE_Completed usually arrives immediately; keep this short timeout as a safeguard. No deadline for choosing A or R.
trainAcceptEventWaitLimit = 3;

currentDir = fileparts(mfilename('fullpath'));
addpath(fullfile(currentDir, '../functions'));

% Connect: authorize, connect the headset, create its session and load (or create) the profile.
cortex = cortexConnect();
fig = [];
statusText = [];
liveText = [];
powerAxes = [];
powerBars = [];
powerLabels = [];
isBusy = false;
isExitRequested = false;
isLiveStreaming = false;
pendingAction = '';
lastSample = '';
connectionCleanup = onCleanup(@cleanupConnection);
try
    if ~cortex.isConnected
        error('facialExpressionTraining:connect', 'Cortex connection or authorization failed.');
    end
    cortex = cortexSession(cortex, headsetId);
    [cortex, profileResult] = cortexProfile(cortex, 'load', profileName);
    if profileResult.created
        fprintf('[facialExpressionTraining] Created and loaded profile %s.\n', profileName);
    else
        fprintf('[facialExpressionTraining] Loaded existing profile %s.\n', profileName);
    end

    facialExpressionInfo = cortexFacialExpressionInfo(cortex);
    trainingDuration = facialExpressionInfo.trainingDuration;
    fprintf('[facialExpressionTraining] Facial expression info:\n%s\n', ...
        jsonencode(facialExpressionInfo, 'PrettyPrint', true));

    % The sys stream reports the training events (started, succeeded, completed).
    cortex = cortexSubscribe(cortex, {'sys'});
    if ~cortex.isSubscribed
        error('facialExpressionTraining:subscribe', 'Could not subscribe to the sys stream.');
    end

    fprintf('[facialExpressionTraining] Key help:\n');
    fprintf('  N / S / F / M / C: start training neutral / surprise / frown / smile / clench.\n');
    fprintf('  A / R: accept / reject the training result.\n');
    fprintf('  L: show live facial expressions.\n');
    fprintf('  U or Esc: unload the profile and exit.\n');

    fig = figure('Name', 'Cortex Facial Expression Training', 'NumberTitle', 'off', ...
        'KeyPressFcn', @onKeyPress);
    figurePosition = get(fig, 'Position');
    figurePosition(3) = 2 * figurePosition(3);
    set(fig, 'Position', figurePosition);
    helpLines = {'Keyboard controls', '', ...
        'N: train neutral (do this first)', ...
        'S: train surprise', ...
        'F: train frown', ...
        'M: train smile', ...
        'C: train clench', ...
        'A: accept the training result (saves the profile)', ...
        'R: reject the training result', ...
        'L: show live expressions (eye, upper and lower face with power 0-1)', ...
        'U or Esc: unload the profile and exit'};
    uicontrol(fig, 'Style', 'text', 'Units', 'normalized', ...
        'Position', [0.08 0.48 0.84 0.48], 'String', helpLines, ...
        'HorizontalAlignment', 'left', 'FontSize', 11, ...
        'BackgroundColor', get(fig, 'Color'), 'Enable', 'on', 'KeyPressFcn', @onKeyPress);
    separatorAxes = axes('Parent', fig, 'Units', 'normalized', 'Position', [0.08 0.465 0.84 0.005], ...
        'XLim', [0 1], 'YLim', [0 1], 'Visible', 'off');
    line('Parent', separatorAxes, 'XData', [0 1], 'YData', [0.5 0.5], 'Color', [0.5 0.5 0.5], 'LineWidth', 1);
    % One row per action; the upper and lower rows share their vertical center with their power bar.
    rowBottoms = [0.40 0.33 0.26];
    liveText = zeros(1, 3);
    for row = 1:3
        liveText(row) = uicontrol(fig, 'Style', 'text', 'Units', 'normalized', ...
            'Position', [0.08 rowBottoms(row) 0.40 0.06], 'String', '', ...
            'HorizontalAlignment', 'left', 'FontSize', 14, ...
            'BackgroundColor', get(fig, 'Color'), 'Enable', 'on', 'KeyPressFcn', @onKeyPress);
    end
    powerAxes = zeros(1, 2);
    powerBars = zeros(1, 2);
    powerLabels = zeros(1, 2);
    for row = 1:2
        powerAxes(row) = axes('Parent', fig, 'Units', 'normalized', ...
            'Position', [0.52 rowBottoms(row + 1) + 0.01 0.40 0.04], ...
            'XLim', [0 1], 'YLim', [0 1], 'XTick', [], 'YTick', [], 'Box', 'on', 'Visible', 'off');
        powerBars(row) = patch('Parent', powerAxes(row), 'Faces', [1 2 3 4], ...
            'Vertices', [0 0; 0 0; 0 1; 0 1], 'FaceColor', [0.8 0.2 0.1], ...
            'EdgeColor', 'none', 'Visible', 'off');
        powerLabels(row) = text('Parent', powerAxes(row), 'Position', [0.5 0.5], 'String', '', ...
            'HorizontalAlignment', 'center', 'Visible', 'off');
    end
    statusText = uicontrol(fig, 'Style', 'text', 'Units', 'normalized', ...
        'Position', [0.08 0.02 0.84 0.20], ...
        'String', ['Profile ' profileName ' ready. Press N to train neutral.'], ...
        'HorizontalAlignment', 'left', 'FontSize', 10, ...
        'BackgroundColor', get(fig, 'Color'), 'Enable', 'on', 'KeyPressFcn', @onKeyPress);

    % Acquire: poll keyboard events and, once subscribed, the live facial expression stream.
    while ~isExitRequested && ishandle(fig)
        if isLiveStreaming && ~isBusy
            updateLiveDisplay();
        else
            pause(0.05);
        end
        drawnow();
    end
catch failure
    fprintf('[facialExpressionTraining] ERROR %s: %s\n', failure.identifier, failure.message);
end

% Disconnect: unload the profile and close the Cortex connection on every exit path.
clear connectionCleanup;
fprintf('[facialExpressionTraining] Finished.\n');

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
            case 's'
                trainAction('surprise');
            case 'f'
                trainAction('frown');
            case 'm'
                trainAction('smile');
            case 'c'
                trainAction('clench');
            case 'a'
                finishTraining('accept');
            case 'r'
                finishTraining('reject');
            case 'l'
                startLiveStream();
            case 'u'
                unloadAndExit();
            otherwise
                showStatus(sprintf('Ignored key: %s. Use N, S, F, M, C, A, R, L, U or Esc.', keyName));
        end
    end

    function trainAction(actionName)
        if ~isempty(pendingAction)
            showStatus(sprintf('Press A to accept or R to reject the %s training first.', pendingAction));
            return;
        end
        isBusy = true;
        try
            showStatus(sprintf(['Training %s: hold for about %g seconds. Waiting up to %g seconds total for ' ...
                'FE_Succeeded/FE_Failed (training plus event margin).'], ...
                actionName, trainingDuration, trainStartEventWaitLimit));
            started = cortexTraining(cortex, 'facialExpression', actionName, 'start', ...
                'wait', trainStartEventWaitLimit);
            if ~started.isSucceeded
                fprintf('[facialExpressionTraining] Training %s failed (%s).\n', actionName, started.event);
                showStatus(sprintf('Training %s failed (%s). Press the key to try again.', ...
                    actionName, started.event));
            else
                pendingAction = actionName;
                fprintf('[facialExpressionTraining] Training %s succeeded (%s).\n', actionName, started.event);
                showStatus(sprintf('Training %s succeeded. Press A to accept or R to reject.', actionName));
            end
        catch trainFailure
            fprintf('[facialExpressionTraining] ERROR %s: %s\n', trainFailure.identifier, trainFailure.message);
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
                showStatus(sprintf(['Accepting %s. Waiting up to %g seconds for FE_Completed ' ...
                    '(example limit; configurable).'], pendingAction, trainAcceptEventWaitLimit));
                accepted = cortexTraining(cortex, 'facialExpression', pendingAction, 'accept', ...
                    'wait', trainAcceptEventWaitLimit);
                fprintf('[facialExpressionTraining] Training %s completed (%s); profile saved: %d.\n', ...
                    pendingAction, accepted.event, accepted.profileSaved);
                showStatus(sprintf('Training %s accepted and profile saved.', pendingAction));
            else
                cortexTraining(cortex, 'facialExpression', pendingAction, 'reject');
                fprintf('[facialExpressionTraining] Training %s rejected.\n', pendingAction);
                showStatus(sprintf('Training %s rejected. Press its key to train again.', pendingAction));
            end
            pendingAction = '';
        catch finishFailure
            fprintf('[facialExpressionTraining] ERROR %s: %s\n', finishFailure.identifier, finishFailure.message);
            showStatus(['Could not ' status ' the training: ' finishFailure.message]);
        end
        isBusy = false;
    end

    function startLiveStream()
        if isLiveStreaming
            showStatus('Live expressions are already shown.');
            return;
        end
        isBusy = true;
        try
            cortex = cortexSubscribe(cortex, {'fac'});
            if ~any(strcmp(cortex.streams, 'fac'))
                error('facialExpressionTraining:subscribe', 'Could not subscribe to the fac stream.');
            end
            isLiveStreaming = true;
            set([powerAxes powerBars powerLabels], 'Visible', 'on');
            showStatus('Showing live facial expressions. Train more actions with N, S, F, M or C.');
        catch subscribeFailure
            fprintf('[facialExpressionTraining] ERROR %s: %s\n', subscribeFailure.identifier, subscribeFailure.message);
            showStatus(['Subscribe failed: ' subscribeFailure.message]);
        end
        isBusy = false;
    end

    function updateLiveDisplay()
        [labels, data, ~, actions] = cortexGetData(cortex, 'fac', 1);
        if isempty(data) || isempty(actions) || ~iscell(actions{end}), return; end
        upperColumn = find(strcmp(labels, 'uPow'), 1);
        lowerColumn = find(strcmp(labels, 'lPow'), 1);
        if isempty(upperColumn) || isempty(lowerColumn), return; end

        % actions{end} is {eyeAct, uAct, lAct}.
        names = actions{end};
        if numel(names) < 3, return; end
        powers = [data(end, upperColumn), data(end, lowerColumn)];
        sample = sprintf('%s|%s|%s|%.2f|%.2f', names{1}, names{2}, names{3}, powers(1), powers(2));
        if strcmp(sample, lastSample), return; end
        lastSample = sample;

        powers(isnan(powers)) = 0;
        levels = max(0, min(1, powers));
        if all(ishandle(liveText))
            set(liveText(1), 'String', ['Eye: ' names{1}]);
            set(liveText(2), 'String', ['Upper: ' names{2}]);
            set(liveText(3), 'String', ['Lower: ' names{3}]);
        end
        for row = 1:2
            % Red (weak) through yellow to green (strong).
            barColor = [min(1, 2 * (1 - levels(row))), min(1, 2 * levels(row)), 0.1];
            if ishandle(powerBars(row))
                set(powerBars(row), 'Vertices', [0 0; levels(row) 0; levels(row) 1; 0 1], ...
                    'FaceColor', barColor);
            end
            if ishandle(powerLabels(row))
                set(powerLabels(row), 'String', sprintf('%.2f', powers(row)));
            end
        end
    end

    function unloadAndExit()
        isBusy = true;
        showStatus('Unloading the profile, please wait...');
        try
            [cortex, unloadResult] = cortexProfile(cortex, 'unload');
            fprintf('[facialExpressionTraining] Profile %s unloaded.\n', unloadResult.name);
            isExitRequested = true;
        catch unloadFailure
            fprintf('[facialExpressionTraining] ERROR %s: %s\n', unloadFailure.identifier, unloadFailure.message);
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
