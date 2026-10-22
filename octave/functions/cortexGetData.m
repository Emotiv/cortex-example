function [labels, data, time] = cortexGetData(cortex, stream, lastN)
% cortexGetData  Read buffered samples of one subscribed stream (eeg, mot, dev, eq, pow or met).
%   [labels, data, time] = cortexGetData(cortex, 'eeg', 256) first collects the data that has
%   arrived (this can take up to about 1 second), then returns the newest 256 samples:
%     labels  cell array with one name per column of data
%     data    samples-by-columns matrix, oldest sample first (NaN where a value is empty)
%     time    sample times in seconds
%   Omit lastN to get everything in the buffer (the last 10000 samples per stream).

if nargin < 3, lastN = []; end
labels = {};
data = [];
time = [];

if ~isstruct(cortex) || ~isfield(cortex, 'isSubscribed') || ~cortex.isSubscribed
    fprintf('[cortexGetData] No active subscription. Call cortexSubscribe first.\n');
    return;
end

headset = cortex.manager.getHeadset();
headset.update();
if ~headset.isSubscribed
    fprintf('[cortexGetData] No active subscription. Call cortexSubscribe first.\n');
    return;
end
[labels, data, time] = cortex.manager.getContext().buffer.read(stream, lastN);
end
