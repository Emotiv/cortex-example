package com.emotiv.cortexv2example.controller;

import com.emotiv.cortexv2example.interfaces.SysInterface;
import org.json.JSONArray;
import org.json.JSONObject;

public class SysController {
    public static SysController sysController;
    private SysInterface mSysInterface;
    public SysController() {

    }

    public static SysController getInstance() {
        if (sysController == null) {
            sysController = new SysController();
        }
        return sysController;
    }

    public void setSysInterface(SysInterface sysInterface) {
        this.mSysInterface = sysInterface;
    }

    // handle the response to a RPC request
    public void parseSysMessage(String msg) {
        try {
            JSONObject sysJson = new JSONObject(msg);
            if (mSysInterface != null) {
                if (sysJson.has("sys")) {
                    JSONArray sysContentJson = sysJson.getJSONArray("sys");
                    CortexClientController.getInstance().getDataStream().getmTrainingObject().setCurrentDetection(sysContentJson.get(0).toString());
                    CortexClientController.getInstance().getDataStream().getmTrainingObject().setCurrentTrainingEvent(sysContentJson.get(1).toString());

                    if (sysContentJson.get(1).toString().contains("Started")) {
                        mSysInterface.onTrainingStarted(sysContentJson.get(0).toString(), CortexClientController.getInstance().getDataStream().getmTrainingObject().getTrainingAction(), sysContentJson.get(1).toString());
                    } else if (sysContentJson.get(1).toString().contains("Failed")) {
                        mSysInterface.onTrainingFailed();
                    } else if (sysContentJson.get(1).toString().contains("Succeeded")) {
                        mSysInterface.onTrainingSucceeded();
                    } else if (sysContentJson.get(1).toString().contains("Completed")) {
                        mSysInterface.onTrainingCompleted();
                    } else if (sysContentJson.get(1).toString().contains("Rejected")) {
                        mSysInterface.onTrainingRejected();
                    } else if (sysContentJson.get(1).toString().contains("Reset")) {
                        mSysInterface.onTrainingReset();
                    }

                } else if (sysJson.has("eeg")) {
                    mSysInterface.onEegDataRecieved(msg);
                } else if (sysJson.has("mot")) {
                    mSysInterface.onMotDataRecieved(msg);
                } else if (sysJson.has("dev")) {
                    mSysInterface.onDevDataRecieved(msg);
                } else if (sysJson.has("met")) {
                    mSysInterface.onMetDataRecieved(msg);
                } else if (sysJson.has("com")) {
                    mSysInterface.onComDataRecieved(msg);
                } else if (sysJson.has("fac")) {
                    mSysInterface.onFacDataRecieved(msg);
                }
            }
        } catch (Exception e) { e.printStackTrace(); }
    }

}
