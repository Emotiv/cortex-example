function subscribeAndPlot()
% subscribeAndPlot  Subscribe to EEG, motion (mot), and performance metrics (met) streams and plot live.
%   Start the proxy first: python octave/proxy/cortex_proxy.py

% 1. Initialization & Configuration: make the SDK functions visible.
currentDir = fileparts(mfilename('fullpath'));
addpath(fullfile(currentDir, '../functions'));

% Set your headset id (shown in EMOTIV Launcher, for example 'INSIGHT-A1B2C3D4').
headsetId = '<YOUR_HEADSET_ID>';
streams = {'eeg'};
durationSeconds = 90;

% 2. Connection / Authentication, then headset connection and subscription.
%    Credentials are loaded from config/config.json.
cortex = cortexConnect();
if cortex.isConnected
    cortex = cortexSubscribe(cortex, headsetId, streams);
end

% 3. Acquisition Loop: each cortexGetData call collects new samples into the buffer.
try
    if cortex.isSubscribed
        figs = struct();
        if ismember('eeg', cortex.streams)
            figs.eeg = figure('Name', 'Live EEG', 'NumberTitle', 'off');
        end
        if ismember('mot', cortex.streams)
            figs.mot = figure('Name', 'Live Motion (IMU)', 'NumberTitle', 'off');
        end
        if ismember('met', cortex.streams)
            figs.met = figure('Name', 'Live Performance Metrics', 'NumberTitle', 'off');
        end

        startedAt = tic();
        while toc(startedAt) < durationSeconds && hasOpenFigure(figs)
            if isfield(figs, 'eeg') && ishandle(figs.eeg)
                [labels, data, time] = cortexGetData(cortex, 'eeg', 256);
                if ~isempty(data)
                    plotEeg(figs.eeg, labels, data, time);
                end
            end

            if isfield(figs, 'mot') && ishandle(figs.mot)
                [labels, data, time] = cortexGetData(cortex, 'mot', 160);
                if ~isempty(data)
                    plotMotion(figs.mot, labels, data, time);
                end
            end

            if isfield(figs, 'met') && ishandle(figs.met)
                [labels, data, time] = cortexGetData(cortex, 'met', 60);
                if ~isempty(data)
                    plotPm(figs.met, labels, data, time);
                end
            end

            drawnow;
        end

        for index = 1:numel(cortex.streams)
            streamName = cortex.streams{index};
            [~, data] = cortexGetData(cortex, streamName);
            fprintf('[subscribeAndPlot] %s: %d samples buffered.\n', streamName, size(data, 1));
        end
    end
catch failure
    fprintf('[subscribeAndPlot] ERROR %s: %s\n', failure.identifier, failure.message);
end

% 4. Safe Teardown: unsubscribes and releases the socket, even after an error.
cortex = cortexDisconnect(cortex);
fprintf('[subscribeAndPlot] Done.\n');
end

function open = hasOpenFigure(figs)
fields = fieldnames(figs);
open = false;
for i = 1:numel(fields)
    if ishandle(figs.(fields{i}))
        open = true;
        return;
    end
end
end

function plotEeg(fig, labels, data, time)
% plotEeg  Plot multi-channel EEG with +/-100 uV per trace and 200 uV channel spacing.
figure(fig);
channels = find(strcmp(labels, 'INTERPOLATED')) + 1:find(strcmp(labels, 'RAW_CQ')) - 1;
if isempty(channels), return; end

eeg = data(:, channels);
eeg = eeg - mean(eeg, 1, 'omitnan');
eeg = max(min(eeg, 100), -100);

numChannels = numel(channels);
offsets = (numChannels - 1:-1:0) * 200;
t = time - time(end);

plot(t, bsxfun(@plus, eeg, offsets));
ylim([-100, (numChannels - 1) * 200 + 100]);
set(gca, 'YTick', fliplr(offsets), 'YTickLabel', fliplr(labels(channels)));
title('EEG (+/-100 uV per trace, 200 uV spacing)');
xlabel('Seconds');
ylabel('Channel');
end

function plotMotion(fig, labels, data, time)
% plotMotion  Plot 9-axis IMU motion stream:
%   - Quaternions (Q0..Q3): [-1, 1] (or Gyroscope GYROX..GYROZ: [-100, 100] dps)
%   - Accelerometer (ACCX..ACCZ): [-4, 4] g
%   - Magnetometer (MAGX..MAGZ): [-4900, 4900] uT
figure(fig);
t = time - time(end);

quatCols = find(ismember(labels, {'Q0', 'Q1', 'Q2', 'Q3'}));
gyroCols = find(ismember(labels, {'GYROX', 'GYROY', 'GYROZ'}));
accCols = find(ismember(labels, {'ACCX', 'ACCY', 'ACCZ'}));
magCols = find(ismember(labels, {'MAGX', 'MAGY', 'MAGZ'}));

subplot(3, 1, 1);
if ~isempty(quatCols)
    plot(t, data(:, quatCols));
    ylim([-1, 1]);
    legend(labels(quatCols), 'Location', 'eastoutside');
    title('Quaternions ([-1, 1])');
    ylabel('Quaternion');
elseif ~isempty(gyroCols)
    plot(t, data(:, gyroCols));
    ylim([-100, 100]);
    legend(labels(gyroCols), 'Location', 'eastoutside');
    title('Gyroscope ([-100, 100] dps)');
    ylabel('dps');
end

subplot(3, 1, 2);
if ~isempty(accCols)
    plot(t, data(:, accCols));
    ylim([-4, 4]);
    legend(labels(accCols), 'Location', 'eastoutside');
    title('Accelerometer ([-4, 4] g)');
    ylabel('g');
end

subplot(3, 1, 3);
if ~isempty(magCols)
    plot(t, data(:, magCols));
    ylim([-4900, 4900]);
    legend(labels(magCols), 'Location', 'eastoutside');
    title('Magnetometer ([-4900, 4900] uT)');
    ylabel('uT');
    xlabel('Seconds');
end
end

function plotPm(fig, labels, data, time)
% plotPm  Plot normalized Performance Metrics (met) in the [0, 1] range.
figure(fig);
metricCols = find(cellfun(@isempty, regexp(labels, '\.isActive$')));
if isempty(metricCols), return; end

t = time - time(end);
plot(t, data(:, metricCols), '-o');
ylim([0, 1]);
legend(labels(metricCols), 'Location', 'eastoutside');
title('Performance Metrics ([0, 1])');
xlabel('Seconds');
ylabel('Normalized Score');
end
