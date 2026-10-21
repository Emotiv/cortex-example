function [clientId, clientSecret, port] = cortexCredentials()
% cortexCredentials  Load clientId, clientSecret and the optional port from config/config.json.
%   port defaults to 6868 (production) when config.json has no "port" field.
%   Internal: the dev environment uses port 7070; set "port": 7070 in config.json.

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

port = 6868;
if isfield(config, 'port') && ~isempty(config.port)
    port = config.port;
    if ~isnumeric(port) || ~isscalar(port) || ~isfinite(port) || port ~= fix(port) || port < 1 || port > 65535
        error('cortexCredentials:invalidConfig', ...
            'octave/config/config.json "port" must be an integer between 1 and 65535.');
    end
end
end
