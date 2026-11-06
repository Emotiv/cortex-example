function cortex = cortexDisconnect(cortex)
% cortexDisconnect  Close the Cortex connection opened by cortexConnect.
%   cortex = cortexDisconnect(cortex) is safe to call even if cortexConnect failed.

if isstruct(cortex) && isfield(cortex, 'manager') && ~isempty(cortex.manager)
    fprintf('[cortexDisconnect] Closing Cortex connection...\n');
    try
        cortex.manager.close();
    catch failure
        fprintf('[cortexDisconnect] Could not close the Cortex connection cleanly: %s\n', failure.message);
    end
    cortex.manager = [];
end

cortex.isConnected = false;
cortex.isSubscribed = false;
cortex.isRecording = false;
cortex.recordId = '';
cortex.headsetId = '';
cortex.sessionId = '';
cortex.streams = {};
cortex.token = '';
cortex.userInfo = [];
cortex.licenseInfo = [];
end
