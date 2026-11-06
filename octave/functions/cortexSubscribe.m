function cortex = cortexSubscribe(cortex, streams)
% cortexSubscribe  Subscribe to streams on an active headset session.
%   cortex = cortexSubscribe(cortex, {'eeg','mot','dev','eq','pow','met'})
%   Update the fields:
%     isSubscribed  true when the streams are being received
%     streams       the subscribed stream names
%   Read the samples with cortexGetData.

if ~isstruct(cortex) || ~isfield(cortex, 'isConnected') || ~cortex.isConnected || ...
        ~isfield(cortex, 'manager') || isempty(cortex.manager)
    fprintf('[cortexSubscribe] ERROR Call cortexConnect successfully first.\n');
    return;
end

if ischar(streams), streams = {streams}; end
validStreams = {'eeg', 'mot', 'dev', 'eq', 'pow', 'met'};
if ~iscellstr(streams) || isempty(streams) || ~all(ismember(streams, validStreams))
    fprintf('[cortexSubscribe] ERROR streams must be a non-empty list of: %s.\n', strjoin(validStreams, ', '));
    return;
end

try
    context = cortex.manager.getContext();
    if isempty(context.sessionId)
        fprintf('[cortexSubscribe] ERROR Call cortexSession first to create an active session.\n');
        return;
    end

    headset = cortex.manager.getHeadset();
    headset.subscribe(streams);

    context = cortex.manager.getContext();
    cortex.headsetId = context.headsetId;
    cortex.sessionId = context.sessionId;
    cortex.streams = context.subscribedStreams;
    cortex.isSubscribed = ~isempty(context.subscribedStreams);
    fprintf('[cortexSubscribe] Receiving data from %s.\n', context.headsetId);
catch failure
    fprintf('[cortexSubscribe] ERROR %s: %s\n', failure.identifier, failure.message);
end
end
