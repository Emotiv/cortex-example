package com.emotiv.cortexv2example.interfaces;

public abstract interface CortexClientInterface {
    // received an error message in response to a RPC request
    void onError(int errorCode, String errorMessage);
    void onSocketConnected();
    void onGetUserLoginOk();
    void onHasAccessRightOk();
    void onRequestAccessOk();
    void onAuthorizeOk();
    void onGetUserInformationOk();
    void onGetLicenseInfoOk();
    void onQueryHeadsetOk();
    void onControlDeviceOk();
    void onCreateSessionOk();
    void onUpdateSessionOk();
    void onSubscribeDataOk();
    void onUnSubscribeDataOk();
    void onCreateRecordOk();
    void onStopRecordOk();
    void onInjectMarkerOk();
    void onCreateProfileOk();
    void onLoadProfileOk();
    void onSetupTrainingProfileOk();
    void onSaveTrainingProfileOk();
}
