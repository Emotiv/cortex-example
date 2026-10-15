function cortex = cortexConnect(clientId, clientSecret)
% cortexConnect  Connect to Cortex through the proxy and authorize this app.
%   cortex = cortexConnect(clientId, clientSecret) returns a struct with the fields:
%     isConnected  true when the proxy link is open and the app is authorized
%     token        cortexToken to use in later Cortex requests ('' on failure)
%     userInfo     user details returned by Cortex ([] on failure)
%     licenseInfo  license details returned by Cortex ([] on failure)
%     manager      internal handle used by cortexDisconnect; do not modify
%     isSubscribed, headsetId, sessionId, streams   filled in by cortexSubscribe

cortex = struct('isConnected', false, 'token', '', 'userInfo', [], 'licenseInfo', [], 'manager', [], ...
    'isSubscribed', false, 'headsetId', '', 'sessionId', '', 'streams', {{}});
manager = CortexManager(clientId, clientSecret);
cortex.manager = manager;

try
    fprintf('[cortexConnect] Connecting to the proxy on %s:%d...\n', manager.host, manager.port);
    try
        auth = manager.getAuth();
    catch connectFailure
        fprintf(['Could not connect to the proxy on localhost:%d. ' ...
            'Start the proxy first (see README Quickstart, step 3).\n'], manager.port);
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
