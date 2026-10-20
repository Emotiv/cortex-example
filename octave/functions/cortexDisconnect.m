function cortex = cortexDisconnect(cortex)
% cortexDisconnect  Close the proxy connection opened by cortexConnect.
%   cortex = cortexDisconnect(cortex) is safe to call even if cortexConnect failed.

if isstruct(cortex) && isfield(cortex, 'manager') && ~isempty(cortex.manager)
    fprintf('[cortexDisconnect] Closing proxy connection...\n');
    cortex.manager.close();
    cortex.manager = [];
end

cortex.isConnected = false;
cortex.isSubscribed = false;
cortex.headsetId = '';
cortex.sessionId = '';
cortex.streams = {};
cortex.token = '';
cortex.userInfo = [];
cortex.licenseInfo = [];
end
