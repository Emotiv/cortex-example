function [clientId, clientSecret] = cortexCredentials(newClientId, newClientSecret)
% cortexCredentials  Set (two arguments) or get the Cortex app credentials shared by all examples.

persistent storedId storedSecret

if nargin == 2
    storedId = newClientId;
    storedSecret = newClientSecret;
end

if isempty(storedId) || isempty(storedSecret)
    error('cortexCredentials:notSet', 'Credentials not set. Run octave/examples/setCredentials.m or call cortexCredentials(clientId, clientSecret).');
end

clientId = storedId;
clientSecret = storedSecret;
end
