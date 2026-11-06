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
        headset = []
        record = []
        marker = []
    end

    methods
        function obj = CortexManager(clientId, clientSecret, host, port)
            if nargin < 2
                [clientId, clientSecret, configPort] = cortexCredentials();
            else
                configPort = 6868;
            end
            if nargin < 3, host = 'localhost'; end
            if nargin < 4, port = configPort; end
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

        function headset = getHeadset(obj)
            if isempty(obj.headset)
                obj.headset = CortexHeadset(obj.getClient(), obj.context);
            end
            headset = obj.headset;
        end

        function record = getRecord(obj)
            if isempty(obj.record)
                obj.record = CortexRecord(obj.getClient(), obj.context);
            end
            record = obj.record;
        end

        function marker = getMarker(obj)
            if isempty(obj.marker)
                obj.marker = CortexMarker(obj.getClient(), obj.context);
            end
            marker = obj.marker;
        end

        function close(obj)
            % Only closes what was actually created.
            if ~isempty(obj.headset)
                try
                    obj.headset.unsubscribe();
                catch
                end
                obj.headset = [];
            end
            if ~isempty(obj.auth)
                obj.auth.close();
            end
            obj.record = [];
            obj.marker = [];
        end
    end
end
