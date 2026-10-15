% subscribeAndPlot  Subscribe to the EEG stream and plot it live.
%   Start the proxy first: python octave/proxy/cortex_proxy.py

% 1. Initialization & Configuration: make the SDK functions and classes visible.
currentDir = fileparts(mfilename('fullpath'));
addpath(fullfile(currentDir, '../internal'));
addpath(fullfile(currentDir, '../api'));
addpath(currentDir);

% Set your headset id (shown in EMOTIV Launcher, for example 'INSIGHT-A1B2C3D4').
headsetId = '<YOUR_HEADSET_ID>';
streams = {'eeg'};
durationSeconds = 30;

% Client id/secret come from config/config.json (or an input() prompt if it is missing).
setCredentials;

% 2. Connection / Authentication, then headset connection and subscription.
cortex = cortexConnect(clientId, clientSecret);
if cortex.isConnected
    cortex = cortexSubscribe(cortex, headsetId, streams);
end

% 3. Acquisition Loop: each cortexGetData call collects new samples into the buffer.
try
    if cortex.isSubscribed
        figure('Name', 'Live EEG', 'NumberTitle', 'off');
        startedAt = tic();
        while toc(startedAt) < durationSeconds && ishandle(gcf)
            [labels, data, time] = cortexGetData(cortex, 'eeg', 256);
            if ~isempty(data)
                % Electrode columns are between INTERPOLATED and RAW_CQ in Cortex's labels.
                channels = find(strcmp(labels, 'INTERPOLATED')) + 1:find(strcmp(labels, 'RAW_CQ')) - 1;
                plot(time - time(end), data(:, channels));
                title('EEG');
                legend(labels(channels));
                xlabel('Seconds');
                ylabel('EEG (uV)');
                drawnow;
            end
        end

        [~, data] = cortexGetData(cortex, 'eeg');
        fprintf('[subscribeAndPlot] eeg: %d samples buffered.\n', size(data, 1));
    end
catch failure
    fprintf('[subscribeAndPlot] ERROR %s: %s\n', failure.identifier, failure.message);
end

% 4. Safe Teardown: unsubscribes and releases the socket, even after an error.
cortex = cortexDisconnect(cortex);
fprintf('[subscribeAndPlot] Done.\n');