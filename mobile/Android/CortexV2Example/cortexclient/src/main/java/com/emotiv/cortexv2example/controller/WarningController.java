package com.emotiv.cortexv2example.controller;

import com.emotiv.cortexv2example.interfaces.WarningInterface;
import org.json.JSONObject;

public class WarningController {

    public static WarningController mEOWarningController;
    private int warningCode = -1;
    private String warningMessage = "";
    private WarningInterface warningInterface;

    public WarningController() {}

    public static WarningController getInstance() {
        if (mEOWarningController == null)
            mEOWarningController = new WarningController();
        return mEOWarningController;
    }

    public void setWarningInterface(WarningInterface warningInterface) {
        this.warningInterface = warningInterface;
    }

    public int getWarningCode() {
        return warningCode;
    }

    public void setWarningCode(int warningCode) {
        this.warningCode = warningCode;
    }

    public String getWarningMessage() {
        return warningMessage;
    }

    public void setWarningMessage(String warningMessage) {
        this.warningMessage = warningMessage;
    }

    // handle the response to a RPC request
    public void parseWarningMessage(String msg) {
        try {
            JSONObject warningJson = new JSONObject(msg);
            if (warningJson.has("warning")) {
                JSONObject warningContentJson = warningJson.getJSONObject("warning");
                setWarningCode(warningContentJson.getInt("code"));
                setWarningMessage(warningContentJson.getString("message"));
                if (warningInterface != null) {
                    warningInterface.onNewWarning(getWarningCode(), getWarningMessage());
                }
            }
        } catch (Exception e) { e.printStackTrace(); }
    }
}