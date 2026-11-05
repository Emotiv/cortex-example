function [labels, data, time, bciActions] = cortexGetData(cortex, stream, lastN)
% cortexGetData  Read buffered samples of one subscribed stream (eeg, mot, dev, eq, pow, met, com or fac).
%   [labels, data, time, bciActions] = cortexGetData(cortex, 'eeg', 256) first collects the data that has
%   arrived (this can take up to about 1 second), then returns the newest 256 samples:
%     labels  cell array with one name per column of data
%     data    samples-by-columns matrix, oldest sample first (NaN where a value is empty)
%     time    sample times in seconds
%     bciActions  cell array with one action per sample for com and fac, '' for other streams
%   For com, labels are {'act','pow'}: the act column of data is NaN and bciActions holds the action,
%   while pow (0 to 1) is the action power.
%   For fac, labels are {'eyeAct','uAct','uPow','lAct','lPow'}: the act columns of data are NaN and
%   each element of bciActions is {eyeAct, uAct, lAct}; uPow and lPow (0 to 1) are the powers.
%   Omit lastN to get everything in the buffer (the last 10000 samples per stream).

if nargin < 3, lastN = []; end
labels = {};
data = [];
time = [];
bciActions = {};

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
[labels, data, time, bciActions] = cortex.manager.getContext().buffer.read(stream, lastN);
end
