classdef CortexContext < handle
    % CortexContext  Shared authentication data for one Cortex connection.

    properties
        token = ''
        userInfo = []
        licenseInfo = []
    end
end
