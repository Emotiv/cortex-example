% authorizeAndShowAccount  Connect to Cortex, authorize this app, show user and license information, and disconnect.

% 1. Initialization & Configuration: make the SDK functions visible.
currentDir = fileparts(mfilename('fullpath'));
addpath(fullfile(currentDir, '../functions'));

% 2. Connection / Authentication: needs EMOTIV Launcher running with a logged-in user.
%    Credentials are loaded from config/config.json.
cortex = cortexConnect();

% 3. Action: show the result of the authentication.
if cortex.isConnected
    fprintf('[authorizeAndShowAccount] Authorized. cortexToken length: %d\n', numel(cortex.token));
    fprintf('[authorizeAndShowAccount] User: %s\n', cortex.userInfo.username);
    fprintf('[authorizeAndShowAccount] License: %s\n', cortex.licenseInfo.licenseName);
else
    fprintf('[authorizeAndShowAccount] Authentication failed. See the messages above.\n');
end

% 4. Safe Teardown: always release the socket, even after a failed connection.
cortex = cortexDisconnect(cortex);
fprintf('[authorizeAndShowAccount] Done.\n');
