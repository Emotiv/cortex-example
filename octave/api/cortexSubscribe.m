function cortex = cortexSubscribe(cortex, headsetId, streams, varargin)
% cortexSubscribe  Scan for a headset, connect it, open a session and subscribe to data streams.
%   cortex = cortexSubscribe(cortex, 'INSIGHT-XXXXXXXX', {'eeg','mot','dev','eq','pow','met'})
%   updates the fields:
%     isSubscribed  true when the streams are being received
%     headsetId     the connected headset
%     sessionId     the Cortex session id
%     streams       the subscribed stream names
%   Optional name/value pairs:
%     'autoRescan'    rescan automatically when a scan finishes (default true)
%     'queryInterval' seconds between headset list queries (default 3)
%     'scanTimeout'   seconds to wait for the headset to appear (default 30)
%   Read the samples with cortexGetData.

cortex.isSubscribed = false;
options = struct('autoRescan', true, 'queryInterval', 3, 'scanTimeout', 30);
for index = 1:2:numel(varargin) - 1
    if ~isfield(options, varargin{index})
        fprintf('[cortexSubscribe] ERROR Unknown option "%s".\n', varargin{index});
        return;
    end
    options.(varargin{index}) = varargin{index + 1};
end

if ~cortex.isConnected
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
    headset = cortex.manager.getHeadset();
    headset.scanHeadset(options.autoRescan, options.queryInterval);

    if ~headset.waitForHeadset(headsetId, options.scanTimeout)
        fprintf(['[cortexSubscribe] Headset %s was not found within %g seconds. ' ...
            'Check that it is switched on and in range.\n'], headsetId, options.scanTimeout);
        headset.stopScan();
        return;
    end

    [ok, message] = headset.connectToHeadset(headsetId, streams);
    if ~ok
        fprintf('[cortexSubscribe] ERROR %s\n', message);
        return;
    end

    context = cortex.manager.getContext();
    cortex.headsetId = context.headsetId;
    cortex.sessionId = context.sessionId;
    cortex.streams = context.subscribedStreams;
    cortex.isSubscribed = true;
    fprintf('[cortexSubscribe] Receiving data from %s.\n', headsetId);
catch failure
    fprintf('[cortexSubscribe] ERROR %s: %s\n', failure.identifier, failure.message);
end
end
