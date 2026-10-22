function cortex = cortexConnect()
% cortexConnect  Connect to Cortex over WSS and authorize this app.
%   cortex = cortexConnect() loads clientId, clientSecret and the optional port (default 6868)
%   from config/config.json
%   and returns a struct with the fields:
%     isConnected  true when the Cortex link is open and the app is authorized
%     token        cortexToken to use in later Cortex requests ('' on failure)
%     userInfo     user details returned by Cortex ([] on failure)
%     licenseInfo  license details returned by Cortex ([] on failure)
%     manager      internal handle used by cortexDisconnect; do not modify
%     isSubscribed, headsetId, sessionId, streams   filled in by headset connection/subscription
%     isRecording, recordId   filled in by cortexRecord

functionsDir = fileparts(mfilename('fullpath'));
addpath(fullfile(functionsDir, '..', 'internal'));

[clientId, clientSecret, port] = cortexCredentials();

cortex = struct('isConnected', false, 'token', '', 'userInfo', [], 'licenseInfo', [], 'manager', [], ...
    'isSubscribed', false, 'isRecording', false, 'recordId', '', ...
    'headsetId', '', 'sessionId', '', 'streams', {{}});
manager = CortexManager(clientId, clientSecret, 'localhost', port);
cortex.manager = manager;

try
    fprintf('[cortexConnect] Connecting to Cortex at wss://%s:%d...\n', manager.host, manager.port);
    try
        auth = manager.getAuth();
    catch connectFailure
        fprintf(['Could not connect to Cortex at wss://%s:%d. ' ...
            'Make sure EMOTIV Launcher is running.\n'], manager.host, manager.port);
        rethrow(connectFailure);
    end

    fprintf('[cortexConnect] Checking user login and access right...\n');
    auth.init();
    auth.getUserInformation();
    auth.getLicenseInfo();

    context = manager.getContext();
    cortex.token = context.token;
    cortex.userInfo = context.userInfo;
    cortex.licenseInfo = context.licenseInfo;
    cortex.isConnected = true;
    fprintf('[cortexConnect] Connected and authorized.\n');
catch failure
    fprintf('[cortexConnect] ERROR %s: %s\n', failure.identifier, failure.message);
end
end
