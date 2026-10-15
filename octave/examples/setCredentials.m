% setCredentials  Load the Cortex client id/secret from config/config.json for every example.

configFile = fullfile(fileparts(mfilename('fullpath')), '..', 'config', 'config.json');

if exist(configFile, 'file')
    config = jsondecode(fileread(configFile));
    clientId = config.clientId;
    clientSecret = config.clientSecret;
else
    fprintf('[setCredentials] %s not found.\n', configFile);
    fprintf('[setCredentials] Create config/config.json with clientId and clientSecret from your Cortex app (https://www.emotiv.com/my-account/cortex-apps/).\n');
    clientId = input('Client ID: ', 's');
    clientSecret = input('Client secret: ', 's');
end

cortexCredentials(clientId, clientSecret);

