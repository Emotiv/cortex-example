% testAuth  Connect to Cortex, authorize this app, print the token, and disconnect.
%   Start the proxy first: python octave/core/cortex_proxy.py

% 1. Initialization & Configuration: make the SDK functions and classes visible.
currentDir = fileparts(mfilename('fullpath'));
addpath(fullfile(currentDir, '../core'));
addpath(fullfile(currentDir, '../api'));
addpath(currentDir);

% Client id/secret come from config.json (or an input() prompt if it is missing).
setCredentials;

% 2. Connection / Authentication: needs EMOTIV Launcher running with a logged-in user.
cortex = cortexConnect(clientId, clientSecret);

% 3. Action: show the result of the authentication.
if cortex.isConnected
    fprintf('[testAuth] Authorized. cortexToken: %s\n', cortex.token);
    fprintf('[testAuth] User: %s\n', cortex.userInfo.username);
    fprintf('[testAuth] License: %s\n', cortex.licenseInfo.licenseName);
else
    fprintf('[testAuth] Authentication failed. See the messages above.\n');
end

% 4. Safe Teardown: always release the socket, even after a failed connection.
cortex = cortexDisconnect(cortex);
fprintf('[testAuth] Done.\n');
