function recordAndMarker()
% recordAndMarker  Start/stop a record and inject/update markers from keyboard input.
%   Set headsetId and recordTitlePrefix below, then call recordAndMarker().
%   Press S to start, any other letter to toggle a marker interval, and Esc to stop and exit.

% Set your headset id and the prefix used to generate each record title.
headsetId = '<YOUR_HEADSET_ID>';
recordTitlePrefix = 'my-record';

currentDir = fileparts(mfilename('fullpath'));
addpath(fullfile(currentDir, '../functions'));

% Connect: establish authorization, connect the selected headset, and create its active session.
cortex = cortexConnect();
fig = [];
connectionCleanup = onCleanup(@cleanupConnection);
try
    if ~cortex.isConnected
        error('recordAndMarker:connect', 'Cortex connection or authorization failed.');
    end
    cortex = cortexSession(cortex, headsetId);
    fprintf('[recordAndMarker] Connected to %s. Key help:\n', headsetId);
    fprintf('  S: start a record.\n');
    fprintf('  A-Z except S: inject an instance marker; press that key again to end its interval.\n');
    fprintf('  Esc: stop the record and exit.\n');

    markerIds = struct();
    fig = figure('Name', 'Cortex Record and Marker Example', 'NumberTitle', 'off', ...
        'KeyPressFcn', @onKeyPress);
    helpLines = {'Keyboard controls', '', ...
        'S: start a new record', ...
        'A-Z (except S): add an instance marker while recording', ...
        'Press the same letter again: end its marker interval', ...
        'Esc: stop the record and exit', '', ...
        'Marker ID, type, start time, and end time are printed', ...
        'in the Command Window after each marker action.'};
    helpText = uicontrol(fig, 'Style', 'text', 'Units', 'normalized', ...
        'Position', [0.08 0.12 0.84 0.78], ...
        'String', helpLines, ...
        'HorizontalAlignment', 'left', 'FontSize', 12, ...
        'BackgroundColor', get(fig, 'Color'), 'Enable', 'on', ...
        'KeyPressFcn', @onKeyPress);
    statusText = uicontrol(fig, 'Style', 'text', 'Units', 'normalized', ...
        'Position', [0.08 0.02 0.84 0.08], ...
        'String', 'Ready: press S to start a record.', ...
        'HorizontalAlignment', 'left', 'FontSize', 10, ...
        'BackgroundColor', get(fig, 'Color'), 'Enable', 'on', ...
        'KeyPressFcn', @onKeyPress);

    % Acquire: wait for keyboard events while Cortex keeps the active session open.
    uiwait(fig);
catch failure
    fprintf('[recordAndMarker] ERROR %s: %s\n', failure.identifier, failure.message);
end

% Disconnect: stop any active record and close the Cortex connection on every exit path.
clear connectionCleanup;
fprintf('[recordAndMarker] Finished.\n');

    function onKeyPress(source, event)
        keyName = event.Key;
        showStatus(sprintf('Key %s received.', keyName));
        if strcmpi(keyName, 'escape') || strcmpi(keyName, 'esc')
            if cortex.isRecording
                try
                    [cortex, ~] = cortexRecord(cortex, 'stop');
                    fprintf('[recordAndMarker] Record stopped.\n');
                    showStatus('Record stopped. Closing example.');
                catch stopFailure
                    fprintf('[recordAndMarker] ERROR %s: %s\n', stopFailure.identifier, stopFailure.message);
                    showStatus(['Could not stop record: ' stopFailure.message]);
                end
            end
            if ishandle(fig), uiresume(fig); end
            return;
        end

        keyCharacter = event.Character;
        if isempty(keyCharacter) && numel(keyName) == 1
            keyCharacter = keyName;
        end
        if ~ischar(keyCharacter) || numel(keyCharacter) ~= 1 || ...
                isempty(regexp(keyCharacter, '^[A-Za-z]$', 'once'))
            showStatus(sprintf('Ignored key: %s. Use S, A-Z, or Esc.', keyName));
            return;
        end

        % Process: reserve S for record start and use every other letter to toggle marker intervals.
        if lower(keyCharacter) == 's'
            if cortex.isRecording
                fprintf('[recordAndMarker] A record is already active.\n');
                showStatus('A record is already active.');
                return;
            end
            try
                recordTitle = sprintf('%s_%d', recordTitlePrefix, floor(currentTimeMilliseconds() / 1000));
                description = 'Keyboard-controlled Cortex record and marker example';
                tags = {'octave', 'record-marker-demo'};
                [cortex, result] = cortexRecord(cortex, 'start', recordTitle, description, tags);
                fprintf('[recordAndMarker] Started record %s.\n', result.record.uuid);
                showStatus(['Recording started: ' result.record.uuid]);
            catch startFailure
                fprintf('[recordAndMarker] ERROR %s: %s\n', startFailure.identifier, startFailure.message);
                showStatus(['Record start failed: ' startFailure.message]);
            end
            return;
        end

        if ~cortex.isRecording
            fprintf('[recordAndMarker] Start a record with S before adding markers.\n');
            showStatus('No active record. Press S to start one first.');
            return;
        end

        markerKey = lower(keyCharacter);
        try
            if isfield(markerIds, markerKey)
                markerId = markerIds.(markerKey);
                [cortex, result] = cortexMarker(cortex, 'update', markerId, currentTimeMilliseconds());
                markerIds = rmfield(markerIds, markerKey);
                fprintf('[recordAndMarker] Updated marker %s (type: %s).\n', ...
                    result.marker.uuid, result.marker.type);
                fprintf('  Start: %s\n  End:   %s\n', ...
                    result.marker.startDatetime, result.marker.endDatetime);
                showStatus(['Marker interval updated: ' result.marker.uuid]);
            else
                [cortex, result] = cortexMarker(cortex, 'inject', keyCharacter, keyCharacter, ...
                    'time', currentTimeMilliseconds(), 'port', 'MATLAB/Octave example');
                markerIds.(markerKey) = result.marker.uuid;
                fprintf('[recordAndMarker] Injected marker %s (type: %s, port: %s) with value %s.\n', ...
                    result.marker.uuid, result.marker.type, result.marker.port, keyCharacter);
                fprintf('  Start: %s\n  End:   %s\n', ...
                    result.marker.startDatetime, result.marker.endDatetime);
                showStatus(['Marker injected: ' result.marker.uuid ...
                    ' (value: ' result.marker.value ')']);
            end
        catch markerFailure
            fprintf('[recordAndMarker] ERROR %s: %s\n', markerFailure.identifier, markerFailure.message);
            showStatus(['Marker action failed: ' markerFailure.message]);
        end
    end

    function showStatus(message)
        if ~isempty(statusText) && ishandle(statusText)
            set(statusText, 'String', message);
            drawnow();
        end
    end

    function cleanupConnection()
        if cortex.isRecording
            try
                [cortex, ~] = cortexRecord(cortex, 'stop');
            catch stopFailure
                fprintf('[recordAndMarker] Could not stop the active record: %s\n', stopFailure.message);
            end
        end
        if ~isempty(fig) && ishandle(fig)
            delete(fig);
        end
        cortex = cortexDisconnect(cortex);
    end
end

function time = currentTimeMilliseconds()
time = double(javaMethod('currentTimeMillis', 'java.lang.System'));
end
