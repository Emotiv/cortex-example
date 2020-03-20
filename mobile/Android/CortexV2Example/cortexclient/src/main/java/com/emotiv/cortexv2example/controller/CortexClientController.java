package com.emotiv.cortexv2example.controller;

import android.content.Context;
import android.util.Log;
import com.emotiv.cortexv2example.application.MyApplication;
import com.emotiv.cortexv2example.datastream.DataStream;
import com.emotiv.cortexv2example.interfaces.CortexClientInterface;
import com.emotiv.cortexv2example.utils.Constant;
import com.emotiv.cortexv2example.interfaces.WebSocketInterface;
import com.emotiv.cortexv2example.websocket.WebSocketManager;
import org.json.JSONObject;

public class CortexClientController implements WebSocketInterface {

    private final String TAG = CortexClientController.class.getName();
    private static WebSocketManager mWebSocketManager;
    private static CortexClientController mCortexClientController;
    private CortexClientInterface mCortexClientInterface;
    DataStream dataStream;

    public CortexClientController(Context context) {
        dataStream = DataStream.getInstance();
        mWebSocketManager = WebSocketManager.getInstance();
        mWebSocketManager.setWebSocketInterface(this);
    }

    public static CortexClientController getInstance() {
        if (mCortexClientController == null) {
            mCortexClientController = new CortexClientController(MyApplication.getCurrentContext());
        }
        return mCortexClientController;
    }

    public void setCortexClientInterface(CortexClientInterface cortexClientInterface) {
        this.mCortexClientInterface = cortexClientInterface;
    }

    public DataStream getDataStream() {
        return dataStream;
    }

    // handle message recieved from Cortex
    private void handleResponse(String msg) {
        try {
            JSONObject jsonObj = new JSONObject(msg);
            int requestID = jsonObj.getInt("id");
            if (dataStream.parseStreamData(msg)) {
                dataStream.parseStreamData(msg, requestID);

                if (mWebSocketManager.getCurrentStresmID() == Constant.ACCESS_STREAM) {
                    switch (requestID) {
                        case Constant.GET_USER_LOGGED_IN_REQUEST_ID:
                            mCortexClientInterface.onGetUserLoginOk();
                            break;
                        case Constant.HAS_APP_ACCESS_REQUEST_ID:
                            mCortexClientInterface.onHasAccessRightOk();
                            break;
                        case Constant.REQUEST_APP_ACCESS_REQUEST_ID:
                            mCortexClientInterface.onRequestAccessOk();
                            break;
                        case Constant.AUTHORIZE_REQUEST_ID:
                            mCortexClientInterface.onAuthorizeOk();
                            break;
                        case Constant.GET_USER_INFORMATION_REQUEST_ID:
                            mCortexClientInterface.onGetUserInformationOk();
                            break;
                        case Constant.GET_LICENSE_INFORMATION_REQUEST_ID:
                            mCortexClientInterface.onGetLicenseInfoOk();
                            break;
                    }
                } else if (mWebSocketManager.getCurrentStresmID() == Constant.HEADSET_STREAM) {
                    switch (requestID) {
                        case Constant.QUERY_HEADSET_REQUEST_ID:
                            mCortexClientInterface.onQueryHeadsetOk();
                            break;
                        case Constant.CONTROL_DEVICE_REQUEST_ID:
                            mCortexClientInterface.onControlDeviceOk();
                            break;
                    }
                } else if (mWebSocketManager.getCurrentStresmID() == Constant.SESSION_STREAM) {
                    switch (requestID) {
                        case Constant.CREATE_SESSION_REQUEST_ID:
                            mCortexClientInterface.onCreateSessionOk();
                            break;
                        case Constant.UPDATE_SESSION_REQUEST_ID:
                            mCortexClientInterface.onUpdateSessionOk();
                            break;
                    }
                } else if (mWebSocketManager.getCurrentStresmID() == Constant.SUBSCRIBE_STREAM) {
                    switch (requestID) {
                        case Constant.SUBSCRIBE_DATA_REQUEST_ID:
                            mCortexClientInterface.onSubscribeDataOk();
                            break;
                        case Constant.UNSUBSCRIBE_DATA_REQUEST_ID:
                            mCortexClientInterface.onUnSubscribeDataOk();
                            break;
                    }
                } else if (mWebSocketManager.getCurrentStresmID() == Constant.TRAINING_PROFILE_STREAM) {
                    switch (requestID) {
                        case Constant.CREATE_PROFILE_REQUEST_ID:
                            mCortexClientInterface.onCreateProfileOk();
                            break;
                        case Constant.LOAD_PROFILE_REQUEST_ID:
                            mCortexClientInterface.onLoadProfileOk();
                            break;
                        case Constant.TRAINING_PROFILE_MC_REQUEST_ID:
                        case Constant.TRAINING_PROFILE_FE_REQUEST_ID:
                            mCortexClientInterface.onSetupTrainingProfileOk();
                            break;
                        case Constant.SAVE_TRAINING_PROFILE_REQUEST_ID:
                            mCortexClientInterface.onSaveTrainingProfileOk();
                            break;
                    }
                } else if (mWebSocketManager.getCurrentStresmID() == Constant.RECORD_STREAM) {
                    switch (requestID) {
                        case Constant.CREATE_RECORD_REQUEST_ID:
                            mCortexClientInterface.onCreateRecordOk();
                            break;
                        case Constant.STOP_RECORD_REQUEST_ID:
                            mCortexClientInterface.onStopRecordOk();
                            break;
                        case Constant.INJECT_MARKER_REQUEST_ID:
                            mCortexClientInterface.onInjectMarkerOk();
                            break;
                    }
                }

            } else {
                if (mCortexClientInterface != null) {
                    Log.e(TAG, "Error code: " + dataStream.getErrorCode() + " Error msg: " + dataStream.getErrorString());
                    mCortexClientInterface.onError(dataStream.getErrorCode(), dataStream.getErrorString());
                }
            }
        } catch (Exception e) { e.printStackTrace(); }
    }

    // handle warning message recieved from Cortex
    private void handleWarning(String msg) {
        WarningController warningController = WarningController.getInstance();
        warningController.parseWarningMessage(msg);
    }

    // handle training/data message recieved from Cortex
    private void handleData(String msg) {
        SysController sysController = SysController.getInstance();
        sysController.parseSysMessage(msg);
    }

    @Override
    public void onWebSocketError(String error) {}

    @Override
    public void onWebSocketClosed() {}

    @Override
    public void onRecievedMessage(String msg) {
        Log.i(TAG, "new message: " + msg);
        handleResponse(msg);
    }

    @Override
    public void onRecievedWarningMessage(String msg) {
        Log.i(TAG, "new warning: " + msg);
        handleWarning(msg);
    }

    @Override
    public void onRecievedNewData(String msg) {
        Log.i(TAG, "new data: " + msg);
        handleData(msg);
    }

    @Override
    public void onWebSocketOpened() {
        mCortexClientInterface.onSocketConnected();
    }
}