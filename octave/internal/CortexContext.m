classdef CortexContext < handle
    % CortexContext  Shared authentication data for one Cortex connection.

    properties
        token = ''
        userInfo = []
        licenseInfo = []

        headsets = {}           % cell array of headset structs from the last queryHeadsets
        headsetId = ''
        sessionId = ''
        subscribedStreams = {}
        isRecording = false
        recordId = ''
        streamLabels = struct() % one field per stream, each a cell array of column labels
        buffer = []             % CortexDataBuffer holding the received samples
    end
end
