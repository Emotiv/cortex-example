classdef CortexProfile < handle
    % CortexProfile  Queries, creates, loads, saves and unloads training profiles for the active headset.

    properties (Access = private)
        client
        context
    end

    methods
        function obj = CortexProfile(client, context)
            obj.client = client;
            obj.context = context;
        end

        function profiles = query(obj)
            params = struct('cortexToken', obj.context.token);
            response = obj.client.sendRequest('queryProfile', params);
            profiles = CortexProfile.toCell(response.result);
        end

        function profile = findProfile(obj, profileName)
            profile = [];
            profiles = obj.query();
            for index = 1:numel(profiles)
                item = profiles{index};
                if isstruct(item) && isfield(item, 'name') && strcmp(item.name, profileName)
                    profile = item;
                    return;
                end
            end
        end

        function result = loadOrCreate(obj, profileName)
            CortexProfile.validateName(profileName);
            obj.requireHeadset();

            if obj.context.isProfileLoaded
                if strcmp(obj.context.profileName, profileName)
                    result = CortexProfile.makeResult(profileName, '', true, false, true);
                    return;
                end
                error('CortexProfile:otherLoaded', ...
                    'Profile %s is already loaded. Unload it before loading %s.', ...
                    obj.context.profileName, profileName);
            end

            existing = obj.findProfile(profileName);
            created = isempty(existing);
            uuid = '';
            if created
                fprintf('[CortexProfile] Profile %s not found; creating it...\n', profileName);
                createResult = obj.setup(profileName, 'create');
                uuid = CortexProfile.textField(createResult, 'uuid');
            else
                uuid = CortexProfile.textField(existing, 'uuid');
            end

            fprintf('[CortexProfile] Loading profile %s...\n', profileName);
            loadResult = obj.setup(profileName, 'load');
            if isempty(uuid), uuid = CortexProfile.textField(loadResult, 'uuid'); end

            obj.context.profileName = profileName;
            obj.context.isProfileLoaded = true;
            result = CortexProfile.makeResult(profileName, uuid, ~created, created, true);
        end

        function result = save(obj)
            obj.requireLoaded();
            name = obj.context.profileName;
            setupResult = obj.setup(name, 'save');
            result = CortexProfile.makeResult(name, CortexProfile.textField(setupResult, 'uuid'), ...
                true, false, true);
        end

        function result = unload(obj)
            obj.requireLoaded();
            name = obj.context.profileName;
            setupResult = obj.setup(name, 'unload');
            obj.context.profileName = '';
            obj.context.isProfileLoaded = false;
            result = CortexProfile.makeResult(name, CortexProfile.textField(setupResult, 'uuid'), ...
                true, false, false);
        end
    end

    methods (Access = private)
        function result = setup(obj, profileName, status)
            params = struct('cortexToken', obj.context.token, 'headset', obj.context.headsetId, ...
                'profile', profileName, 'status', status);
            response = obj.client.sendRequest('setupProfile', params);
            result = response.result;
        end

        function requireHeadset(obj)
            if isempty(obj.context.headsetId)
                error('CortexProfile:noSession', 'Create a headset session before using profiles.');
            end
        end

        function requireLoaded(obj)
            if ~obj.context.isProfileLoaded
                error('CortexProfile:notLoaded', 'No profile is loaded.');
            end
        end
    end

    methods (Static, Access = private)
        function validateName(profileName)
            if ~ischar(profileName) || isempty(profileName)
                error('CortexProfile:name', 'profileName must be a non-empty character vector.');
            end
        end

        function result = makeResult(name, uuid, existed, created, isLoaded)
            result = struct('name', name, 'uuid', uuid, 'existed', existed, ...
                'created', created, 'isLoaded', isLoaded);
        end

        function text = textField(item, fieldName)
            text = '';
            if isstruct(item) && isfield(item, fieldName) && ischar(item.(fieldName))
                text = item.(fieldName);
            end
        end

        function items = toCell(value)
            % jsondecode returns a struct array or a cell array depending on the content.
            if isempty(value)
                items = {};
            elseif iscell(value)
                items = value(:)';
            elseif isstruct(value)
                items = num2cell(value(:))';
            else
                items = {};
            end
        end
    end
end
