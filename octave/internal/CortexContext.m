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
        profileName = ''
        isProfileLoaded = false
        detectionInfo = struct() % per detection: cell arrays actions, controls and events, set on profile load
        trainingEvents = {}     % structs with detection, event and time from the sys stream
        streamLabels = struct() % one field per stream, each a cell array of column labels
        buffer = []             % CortexDataBuffer holding the received samples
    end
end
