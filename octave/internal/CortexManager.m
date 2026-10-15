classdef CortexManager < handle
    % CortexManager  Owns the Cortex objects and creates each one lazily on first use.
    %   The TCP connection (CortexClient) is opened by CortexAuth, so nothing connects until getAuth().

    properties (SetAccess = private)
        host
        port
        clientId
        clientSecret
        context
    end

    properties (Access = private)
        auth = []
    end

    methods
        function obj = CortexManager(clientId, clientSecret, host, port)
            if nargin < 2
                [clientId, clientSecret] = cortexCredentials();
            end
            if nargin < 3, host = '127.0.0.1'; end
            if nargin < 4, port = 9999; end
            obj.clientId = clientId;
            obj.clientSecret = clientSecret;
            obj.host = host;
            obj.port = port;
            obj.context = CortexContext();
        end

        function auth = getAuth(obj)
            if isempty(obj.auth)
                obj.auth = CortexAuth(obj.clientId, obj.clientSecret, obj.host, obj.port, obj.context);
            end
            auth = obj.auth;
        end

        function context = getContext(obj)
            context = obj.context;
        end

        function client = getClient(obj)
            client = obj.getAuth().getClient();
        end

        % Future getHeadset/getRecord/getBCI follow the getAuth pattern: create lazily with getClient() and getAuth().

        function close(obj)
            % Only closes what was actually created.
            if ~isempty(obj.auth)
                obj.auth.close();
            end
        end
    end
end
