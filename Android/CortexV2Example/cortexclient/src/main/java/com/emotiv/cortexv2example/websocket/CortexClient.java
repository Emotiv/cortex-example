package com.emotiv.cortexv2example.websocket;

import com.emotiv.cortexv2example.datastream.DataStream;
import com.emotiv.cortexv2example.utils.Constant;
import org.json.JSONArray;
import org.json.JSONObject;
import java.util.List;

public class CortexClient {

    public static CortexClient mCortexClient;
    private WebSocketManager mWebSocketManager;

    public CortexClient() {
        mWebSocketManager = WebSocketManager.getInstance();
    }

    public static CortexClient getInstance() {
        if (mCortexClient == null) {
            mCortexClient = new CortexClient();
        }

        return mCortexClient;
    }

    // to get User Logged In information
    public void getUserLogin() {
        mWebSocketManager.sendRequest(Constant.ACCESS_STREAM, Constant.GET_USER_LOGGED_IN_REQUEST_ID, "getUserLogin", null, false);
    }

    // to check Access Right of a App
    public void hasAppAccess() {
        try {
            JSONObject params = new JSONObject();
            params.put("clientId", Constant.CLIENT_ID);
            params.put("clientSecret", Constant.CLIENT_SECRET);
            mWebSocketManager.sendRequest(Constant.ACCESS_STREAM, Constant.HAS_APP_ACCESS_REQUEST_ID, "hasAccessRight", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // all apps need to send requestAccess to User work with Cortex via the App
    public void requestAppAccess() {
        try {
            JSONObject params = new JSONObject();
            params.put("clientId", Constant.CLIENT_ID);
            params.put("clientSecret", Constant.CLIENT_SECRET);
            mWebSocketManager.sendRequest(Constant.ACCESS_STREAM, Constant.REQUEST_APP_ACCESS_REQUEST_ID, "requestAccess", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // get an authorization token
    public void authorize() {
        try {
            JSONObject params = new JSONObject();
            params.put("clientId", Constant.CLIENT_ID);
            params.put("clientSecret", Constant.CLIENT_SECRET);
            params.put("debit", Constant.DEBIT_NUMBER);
            params.put("license", Constant.LICENSE_ID);
            mWebSocketManager.sendRequest(Constant.ACCESS_STREAM, Constant.AUTHORIZE_REQUEST_ID, "authorize", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // to get current user information. In addition, the eula acceptance information included
    public void geUserInformation() {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", DataStream.getCortexToken());
            mWebSocketManager.sendRequest(Constant.ACCESS_STREAM, Constant.GET_USER_INFORMATION_REQUEST_ID, "getUserInformation", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // to get current license information. If user is online, the latest information from cloud
    // will be return, otherwise return information from local database
    public void getLicenseInformation() {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", DataStream.getCortexToken());
            mWebSocketManager.sendRequest(Constant.ACCESS_STREAM, Constant.GET_LICENSE_INFORMATION_REQUEST_ID, "getLicenseInfo", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // list all the headsets connected to your computer
    public void queryHeadset() {
        mWebSocketManager.sendRequest(Constant.HEADSET_STREAM, Constant.QUERY_HEADSET_REQUEST_ID, "queryHeadsets", null, false);
    }

    // to connect/disconnect a headset.
    // in addition, refresh headset to new updated information is applied to headset
    public void controlDevice(String command, String headsetID) {
        try {
            JSONObject params = new JSONObject();
            params.put("command", command);
            params.put("headset", headsetID);
            mWebSocketManager.sendRequest(Constant.HEADSET_STREAM, Constant.CONTROL_DEVICE_REQUEST_ID, "controlDevice", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // open a session, so we can then get data from a headset
    // you need a license to activate the session
    public void createSession(String cortexToken, String status, String headsetID) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("status", status);
            params.put("headset", headsetID);
            mWebSocketManager.sendRequest(Constant.SESSION_STREAM, Constant.CREATE_SESSION_REQUEST_ID, "createSession", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // to update session: close or active
    public void updateSession(String cortexToken, String status, String sessionID) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("status", status);
            params.put("session", sessionID);
            mWebSocketManager.sendRequest(Constant.SESSION_STREAM, Constant.UPDATE_SESSION_REQUEST_ID, "updateSession", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // to create a record
    public void createRecord(String cortexToken, String sessionID, String title, String description) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("session", sessionID);
            params.put("title", title);
            params.put("description", description);
            mWebSocketManager.sendRequest(Constant.RECORD_STREAM, Constant.CREATE_RECORD_REQUEST_ID, "createRecord", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // to stop a recording
    public void stopRecord(String cortexToken, String sessionID) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("session", sessionID);
            mWebSocketManager.sendRequest(Constant.RECORD_STREAM, Constant.STOP_RECORD_REQUEST_ID, "stopRecord", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // insert a marker, to mark an event in a session
    // you can use injectMarker alone, to mark an instant event
    // or you can use injectMarker and later injectStopMarker, to mark a period of time
    public void injectMarker(String cortexToken, String sessionID, String label, String value, String port, long time) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("session", sessionID);
            params.put("label", label);
            params.put("value", value);
            params.put("port", port);
            params.put("time", time);
            mWebSocketManager.sendRequest(Constant.RECORD_STREAM, Constant.INJECT_MARKER_REQUEST_ID, "injectMarker", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // to subsribe data streams include: eeg, motion (mot), performance metric(met),power band(pow)
    // mentalCommand(com), facial expression(fac), training event(sys), contact quality(dev)
    public void subscribeData(String cortexToken, String sessionID, List<String> streamArray) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("session", sessionID);
            JSONArray arrayStream = new JSONArray();
            for (int i = 0 ; i < streamArray.size(); i++) {
                arrayStream.put(streamArray.get(i));
            }
            params.put("streams", arrayStream);
            mWebSocketManager.sendRequest(Constant.SUBSCRIBE_STREAM, Constant.SUBSCRIBE_DATA_REQUEST_ID, "subscribe", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // to unsubscribe data
    public void unSubscribeData(String cortexToken, String sessionID, List<String> streamArray) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("session", sessionID);
            JSONArray arrayStream = new JSONArray();
            for (int i = 0 ; i < streamArray.size(); i++) {
                arrayStream.put(streamArray.get(i));
            }
            params.put("streams", arrayStream);
            mWebSocketManager.sendRequest(Constant.SUBSCRIBE_STREAM, Constant.UNSUBSCRIBE_DATA_REQUEST_ID, "unsubscribe", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // to create, load, upload, rename or delete profile
    public void setupProfile(String cortexToken, String profileName, String headsetID, String status, int requestType) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("profile", profileName);
            params.put("headset", headsetID);
            params.put("status", status);
            mWebSocketManager.sendRequest(Constant.TRAINING_PROFILE_STREAM, requestType, "setupProfile", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // to get current profile
    public void getCurrentProfile(String cortexToken, String headsetID) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("headset", headsetID);
            mWebSocketManager.sendRequest(Constant.TRAINING_PROFILE_STREAM, Constant.GET_CURRENT_PROFILE_REQUEST_ID, "getCurrentProfile", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }

    // training profile management for facial expression and mental command
    public void training(String cortexToken, String detection, String sessionID, String action, String status, int requestType) {
        try {
            JSONObject params = new JSONObject();
            params.put("cortexToken", cortexToken);
            params.put("detection", detection);
            params.put("session", sessionID);
            params.put("action", action);
            params.put("status", status);
            mWebSocketManager.sendRequest(Constant.TRAINING_PROFILE_STREAM, requestType, "training", params, true);
        } catch (Exception e) { e.printStackTrace(); }
    }
}