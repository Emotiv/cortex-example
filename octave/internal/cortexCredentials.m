function [clientId, clientSecret] = cortexCredentials()
% cortexCredentials  Load the Cortex app clientId and clientSecret from config/config.json.

configFile = fullfile(fileparts(mfilename('fullpath')), '..', 'config', 'config.json');
if ~exist(configFile, 'file')
    error('cortexCredentials:configNotFound', ...
        ['Config file not found: %s. ' ...
         'Copy octave/config/config.example.json to octave/config/config.json ' ...
         'and set your clientId and clientSecret.'], configFile);
end

config = jsondecode(fileread(configFile));
if ~isfield(config, 'clientId') || ~isfield(config, 'clientSecret') || ...
        isempty(config.clientId) || isempty(config.clientSecret)
    error('cortexCredentials:invalidConfig', ...
        'octave/config/config.json must contain non-empty clientId and clientSecret fields.');
end

clientId = config.clientId;
clientSecret = config.clientSecret;
end
