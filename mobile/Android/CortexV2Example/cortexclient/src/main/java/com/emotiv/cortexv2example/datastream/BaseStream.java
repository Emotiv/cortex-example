package com.emotiv.cortexv2example.datastream;

import org.json.JSONObject;

public class BaseStream {
    int errorCode = -1;
    String errorString = "";

    public int getErrorCode() {
        return errorCode;
    }

    public String getErrorString() {
        return errorString;
    }

    public boolean parseStreamData(String jsonString) {
        try {
            JSONObject jsonObj = new JSONObject(jsonString);
            if (jsonObj.has("result")) {
                return true;
            }
        } catch (Exception e) { e.printStackTrace(); }

        parseErrorData(jsonString);
        return false;
    }

    public void parseErrorData(String jsonString) {
        try {
            JSONObject jsonObj = new JSONObject(jsonString);
            JSONObject errorObj = jsonObj.getJSONObject("error");
            errorCode = errorObj.getInt("code");
            errorString = errorObj.getString("message");
        } catch (Exception e) { e.printStackTrace(); }
    }
}
