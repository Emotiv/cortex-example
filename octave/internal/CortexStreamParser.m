classdef CortexStreamParser < handle
    % CortexStreamParser  Turns decoded Cortex subscribe/stream messages into labels and numeric rows.
    %   It maps each subscription id (sid) to its stream name; storage is left to CortexDataBuffer.

    properties (Access = private)
        sids = {}
        streams = {}
    end

    methods
        function register(obj, sid, streamName)
            obj.sids{end + 1} = sid;
            obj.streams{end + 1} = streamName;
        end

        function reset(obj)
            obj.sids = {};
            obj.streams = {};
        end

        function [stream, row, time, mcAction] = parse(obj, msg)
            % stream is '' when the message does not belong to a registered stream.
            % mcAction is the mental command action of a com sample ('' for other streams).
            stream = '';
            row = [];
            time = NaN;
            mcAction = '';
            if isfield(msg, 'sid') && ischar(msg.sid)
                % Streams of one session can share the same sid, so match the stream by its field name too.
                candidates = find(strcmp(obj.sids, msg.sid));
                for index = candidates
                    if isfield(msg, obj.streams{index})
                        stream = obj.streams{index};
                        break;
                    end
                end
            end
            if isempty(stream) || ~isfield(msg, stream)
                stream = '';
                return;
            end

            if isfield(msg, 'time') && isnumeric(msg.time) && ~isempty(msg.time)
                time = double(msg.time);
            end
            row = CortexStreamParser.flattenSample(msg.(stream));
            names = CortexStreamParser.stringsOf(msg.(stream));
            if ~isempty(names), mcAction = names{1}; end
        end
    end

    methods (Static)
        function names = stringsOf(values)
            if ischar(values)
                names = {values};
            elseif iscell(values)
                names = values(cellfun(@ischar, values));
                names = names(:)';
            else
                names = {};
            end
        end

        function labels = flattenLabels(cols)
            % The dev stream nests the contact-quality labels in a sub-array.
            if ischar(cols)
                labels = {cols};
            elseif iscell(cols)
                labels = {};
                for index = 1:numel(cols)
                    labels = [labels, CortexStreamParser.flattenLabels(cols{index})]; %#ok<AGROW>
                end
            else
                labels = {};
            end
        end

        function row = flattenSample(values)
            % Empty entries (null, no markers) become NaN so every row keeps the same width.
            % jsondecode returns numeric, logical or cell values depending on the tool and content.
            if isnumeric(values) || islogical(values)
                if isempty(values)
                    row = NaN;
                else
                    row = double(values(:))';
                end
            elseif iscell(values)
                row = [];
                for index = 1:numel(values)
                    row = [row, CortexStreamParser.flattenSample(values{index})]; %#ok<AGROW>
                end
                if isempty(row), row = NaN; end
            else
                row = NaN;
            end
        end
    end
end
