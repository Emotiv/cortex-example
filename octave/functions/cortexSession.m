function cortex = cortexSession(cortex, headsetId, varargin)
% cortexSession  Connect a headset and create or reuse its active Cortex session.
%   cortex = cortexSession(cortex, 'INSIGHT-XXXXXXXX') creates one session for the headset.
%   Calling it again for the same active headset reuses that session; another headset
%   cannot be connected until the current Cortex connection is closed.
%   Optional name/value pairs: autoRescan (true), queryInterval (3 seconds),
%   and scanTimeout (30 seconds).

options = struct('autoRescan', true, 'queryInterval', 3, 'scanTimeout', 30);
if mod(numel(varargin), 2) ~= 0
    error('cortex:sessionFailed', 'Options must be supplied as name/value pairs.');
end
for index = 1:2:numel(varargin)
    name = varargin{index};
    if ~ischar(name) || ~isfield(options, name)
        error('cortex:sessionFailed', 'Unknown headset session option.');
    end
    options.(name) = varargin{index + 1};
end

if ~isstruct(cortex) || ~isfield(cortex, 'isConnected') || ~cortex.isConnected || ...
        ~isfield(cortex, 'manager') || isempty(cortex.manager)
    error('cortex:sessionFailed', 'Connect and authorize Cortex before creating a headset session.');
end
if ~ischar(headsetId) || isempty(headsetId)
    error('cortex:sessionFailed', 'headsetId must be a non-empty character vector.');
end

manager = cortex.manager;
context = manager.getContext();
if ~isempty(context.sessionId)
    if ~strcmp(context.headsetId, headsetId)
        error('cortex:sessionFailed', ...
            'A session for headset %s is already active. Disconnect before selecting headset %s.', ...
            context.headsetId, headsetId);
    end
    cortex = syncSessionState(cortex, context);
    return;
end

try
    headset = manager.getHeadset();
    headset.scanHeadset(options.autoRescan, options.queryInterval);

    if ~headset.waitForHeadset(headsetId, options.scanTimeout)
        headset.stopScan();
        error('CortexHeadset:notFound', 'Headset %s was not found within %g seconds.', ...
            headsetId, options.scanTimeout);
    end

    [ok, message] = headset.connectToHeadset(headsetId, {});
    if ~ok
        error('CortexHeadset:connect', '%s', message);
    end

    cortex = syncSessionState(cortex, manager.getContext());
catch failure
    try
        manager.close();
    catch
    end
    cortex.manager = [];
    cortex.isConnected = false;
    error('cortex:sessionFailed', 'Could not create a headset session: %s', failure.message);
end
end

function cortex = syncSessionState(cortex, context)
cortex.headsetId = context.headsetId;
cortex.sessionId = context.sessionId;
cortex.streams = context.subscribedStreams;
cortex.isSubscribed = ~isempty(context.subscribedStreams);
cortex.isRecording = context.isRecording;
cortex.recordId = context.recordId;
end
