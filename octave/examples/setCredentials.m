% setCredentials  Load the Cortex client id/secret from config.json (next to this script) for every example.

configFile = fullfile(fileparts(mfilename('fullpath')), 'config.json');

if exist(configFile, 'file')
    config = jsondecode(fileread(configFile));
    clientId = config.clientId;
    clientSecret = config.clientSecret;
else
    fprintf('[setCredentials] %s not found.\n', configFile);
    fprintf('[setCredentials] Copy config.example.json to config.json and fill in the values from your Cortex app (https://www.emotiv.com/my-account/cortex-apps/).\n');
    clientId = input('Client ID: ', 's');
    clientSecret = input('Client secret: ', 's');
end

cortexCredentials(clientId, clientSecret);
