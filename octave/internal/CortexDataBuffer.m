classdef CortexDataBuffer < handle
    % CortexDataBuffer  Fixed-size ring buffer that keeps the latest samples of each data stream.
    %   Rows are samples (oldest first when read) and columns follow the stream labels.

    properties (SetAccess = private)
        capacity
    end

    properties (Access = private)
        names = {}
        labels = {}
        samples = {}
        times = {}
        nextRow = []
        count = []
    end

    methods
        function obj = CortexDataBuffer(capacity)
            if nargin < 1, capacity = 10000; end
            if ~isnumeric(capacity) || ~isscalar(capacity) || capacity < 1
                error('CortexDataBuffer:capacity', 'Capacity must be a positive numeric scalar.');
            end
            obj.capacity = floor(capacity);
        end

        function configure(obj, stream, labels)
            % Resets one stream and stores its column labels.
            idx = obj.indexOf(stream);
            obj.labels{idx} = labels;
            obj.samples{idx} = [];
            obj.times{idx} = [];
            obj.nextRow(idx) = 1;
            obj.count(idx) = 0;
        end

        function append(obj, stream, row, time)
            idx = obj.indexOf(stream);
            width = numel(row);
            if isempty(obj.samples{idx})
                obj.samples{idx} = NaN(obj.capacity, width);
                obj.times{idx} = NaN(obj.capacity, 1);
            end

            columns = size(obj.samples{idx}, 2);
            if width < columns
                row(end + 1:columns) = NaN;
            elseif width > columns
                row = row(1:columns);
            end

            r = obj.nextRow(idx);
            obj.samples{idx}(r, :) = row;
            obj.times{idx}(r) = time;
            obj.nextRow(idx) = mod(r, obj.capacity) + 1;
            obj.count(idx) = min(obj.count(idx) + 1, obj.capacity);
        end

        function [labels, data, time] = read(obj, stream, lastN)
            % Returns the newest lastN samples (all buffered samples when lastN is omitted).
            labels = {};
            data = [];
            time = [];
            idx = find(strcmp(obj.names, stream), 1);
            if isempty(idx), return; end

            labels = obj.labels{idx};
            n = obj.count(idx);
            if nargin >= 3 && ~isempty(lastN)
                n = min(n, lastN);
            end
            if n == 0, return; end

            newest = mod(obj.nextRow(idx) - 2, obj.capacity) + 1;
            rows = mod((newest - n + 1:newest) - 1, obj.capacity) + 1;
            data = obj.samples{idx}(rows, :);
            time = obj.times{idx}(rows);
        end

        function n = numSamples(obj, stream)
            idx = find(strcmp(obj.names, stream), 1);
            if isempty(idx)
                n = 0;
            else
                n = obj.count(idx);
            end
        end
    end

    methods (Access = private)
        function idx = indexOf(obj, stream)
            idx = find(strcmp(obj.names, stream), 1);
            if isempty(idx)
                idx = numel(obj.names) + 1;
                obj.names{idx} = stream;
                obj.labels{idx} = {};
                obj.samples{idx} = [];
                obj.times{idx} = [];
                obj.nextRow(idx) = 1;
                obj.count(idx) = 0;
            end
        end
    end
end
