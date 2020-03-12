package com.emotiv.cortexv2example.mentalcommandtraining;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import androidx.appcompat.app.AppCompatActivity;
import com.emotiv.cortexv2example.controller.CortexClientController;
import com.emotiv.cortexv2example.controller.SysController;
import com.emotiv.cortexv2example.controller.WarningController;
import com.emotiv.cortexv2example.interfaces.CortexClientInterface;
import com.emotiv.cortexv2example.interfaces.SysInterface;
import com.emotiv.cortexv2example.interfaces.WarningInterface;
import com.emotiv.cortexv2example.objects.HeadsetObject;
import com.emotiv.cortexv2example.objects.SessionObject;
import com.emotiv.cortexv2example.utils.Constant;
import com.emotiv.cortexv2example.websocket.CortexClient;
import java.util.ArrayList;
import java.util.List;

public class TrainingActivity extends AppCompatActivity implements CortexClientInterface, SysInterface, WarningInterface, View.OnClickListener {

    private final String TAG = TrainingActivity.class.getName();
    List<String> streamArray = new ArrayList<String>();
    Button btnSubscribeTrainingMCEvent, btnCreateProfile, btnLoadProfile, btnTrainingMCNeutral, btnTrainingMCPush;
    String cortexToken = "";
    private String curDetection = "";
    private String curAction = "";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.training_mc_activity);
        initView();
    }

    private String getCortexToken() {
        Intent intent = getIntent();
        return intent.hasExtra("cortexToken") ? intent.getStringExtra("cortexToken") : "";
    }

    private void initView() {
        cortexToken = getCortexToken();
        streamArray.add("sys");
        btnSubscribeTrainingMCEvent = (Button) findViewById(R.id.btnSubscribeTrainingMCEvent);
        btnSubscribeTrainingMCEvent.setOnClickListener(this);
        btnCreateProfile = (Button) findViewById(R.id.btnCreateProfile);
        btnCreateProfile.setOnClickListener(this);
        btnLoadProfile = (Button) findViewById(R.id.btnLoadProfile);
        btnLoadProfile.setOnClickListener(this);
        btnTrainingMCNeutral = (Button) findViewById(R.id.btnTrainingMCNeutral);
        btnTrainingMCNeutral.setOnClickListener(this);
        btnTrainingMCPush = (Button) findViewById(R.id.btnTrainingMCPush);
        btnTrainingMCPush.setOnClickListener(this);
    }

    @Override
    protected void onResume() {
        super.onResume();
        CortexClientController.getInstance().setCortexClientInterface(this);
        SysController.getInstance().setSysInterface(this);
        WarningController.getInstance().setWarningInterface(this);
    }

    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.btnSubscribeTrainingMCEvent:
                CortexClient.getInstance().subscribeData(cortexToken, SessionObject.getInstance().getCurrentActivedSession(), streamArray);
                break;

            case R.id.btnCreateProfile:
                CortexClient.getInstance().setupProfile(cortexToken, Constant.TRAINING_PROFILE_NAME, HeadsetObject.getInstance().getHeadsetName(), "create", Constant.CREATE_PROFILE_REQUEST_ID);
                break;

            case R.id.btnLoadProfile:
                CortexClient.getInstance().setupProfile(cortexToken, Constant.TRAINING_PROFILE_NAME, HeadsetObject.getInstance().getHeadsetName(), "load", Constant.LOAD_PROFILE_REQUEST_ID);
                break;

            case R.id.btnTrainingMCNeutral:
                CortexClient.getInstance().training(cortexToken, "mentalCommand", SessionObject.getInstance().getCurrentActivedSession(), "neutral", "start", Constant.TRAINING_PROFILE_MC_REQUEST_ID);
                break;

            case R.id.btnTrainingMCPush:
                CortexClient.getInstance().training(cortexToken, "mentalCommand", SessionObject.getInstance().getCurrentActivedSession(), "push", "start", Constant.TRAINING_PROFILE_MC_REQUEST_ID);
                break;
        }
    }

    @Override
    public void onError(int errorCode, String errorMessage) {

    }

    @Override
    public void onSocketConnected() {

    }

    @Override
    public void onGetUserLoginOk() {

    }

    @Override
    public void onHasAccessRightOk() {

    }

    @Override
    public void onRequestAccessOk() {

    }

    @Override
    public void onAuthorizeOk() {

    }

    @Override
    public void onGetUserInformationOk() {

    }

    @Override
    public void onGetLicenseInfoOk() {

    }

    @Override
    public void onQueryHeadsetOk() {

    }

    @Override
    public void onControlDeviceOk() {

    }

    @Override
    public void onCreateSessionOk() {

    }

    @Override
    public void onUpdateSessionOk() {

    }

    @Override
    public void onCreateRecordOk() {

    }

    @Override
    public void onStopRecordOk() {

    }

    @Override
    public void onInjectMarkerOk() {

    }

    @Override
    public void onSubscribeDataOk() {

    }

    @Override
    public void onUnSubscribeDataOk() {

    }

    @Override
    public void onCreateProfileOk() {

    }

    @Override
    public void onLoadProfileOk() {

    }

    @Override
    public void onSetupTrainingProfileOk() {

    }

    @Override
    public void onSaveTrainingProfileOk() {

    }

    @Override
    public void onTrainingStarted(String detection, String action, String event) {
        curDetection = detection;
        curAction = action;
        Log.i(TAG, "sys event: " + detection + " | " + action + " | " + event);
    }

    @Override
    public void onTrainingFailed() {

    }

    @Override
    public void onTrainingSucceeded() {
        CortexClient.getInstance().training(cortexToken, curDetection, SessionObject.getInstance().getCurrentActivedSession(), curAction, "accept", Constant.ACCEPT_TRAINING_PROFILE_REQUEST_ID);
    }

    @Override
    public void onTrainingCompleted() {
        CortexClient.getInstance().setupProfile(cortexToken, Constant.TRAINING_PROFILE_NAME, HeadsetObject.getInstance().getHeadsetName(), "save", Constant.SAVE_TRAINING_PROFILE_REQUEST_ID);
    }

    @Override
    public void onTrainingRejected() {

    }

    @Override
    public void onTrainingReset() {

    }

    @Override
    public void onEegDataRecieved(String eEgData) {

    }

    @Override
    public void onMotDataRecieved(String motData) {

    }

    @Override
    public void onMetDataRecieved(String metData) {

    }

    @Override
    public void onDevDataRecieved(String devData) {

    }

    @Override
    public void onComDataRecieved(String comData) {

    }

    @Override
    public void onFacDataRecieved(String facData) {

    }

    @Override
    public void onNewWarning(int warningCode, String warningMessage) {

    }
}