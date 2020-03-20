package com.emotiv.cortexv2examplev.authorize;

import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView;
import com.emotiv.cortexv2example.controller.CortexClientController;
import com.emotiv.cortexv2example.interfaces.CortexClientInterface;
import com.emotiv.cortexv2example.websocket.CortexClient;
import com.emotiv.cortexv2example.websocket.WebSocketManager;

public class MainActivity extends AppCompatActivity implements CortexClientInterface, View.OnClickListener {

    WebSocketManager webSocketManager;
    Button btnGetUserLogin, btnHasAccessRight, btnRequestAccess, btnAuthorize, btnGetUserInformation, btnGetLicenseInfo;
    TextView tvEmotivID;
    ProgressBar progressBar;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        initView();
        connectToServer();
    }

    private void initView() {
        progressBar = (ProgressBar) findViewById(R.id.progressBar);
        btnGetUserLogin = (Button) findViewById(R.id.btnGetUserLogin);
        btnGetUserLogin.setOnClickListener(this);
        btnHasAccessRight = (Button) findViewById(R.id.btnHasAccessRight);
        btnHasAccessRight.setOnClickListener(this);
        btnRequestAccess = (Button) findViewById(R.id.btnRequestAccess);
        btnRequestAccess.setOnClickListener(this);
        btnAuthorize = (Button) findViewById(R.id.btnAuthorize);
        btnAuthorize.setOnClickListener(this);
        btnGetUserInformation = (Button) findViewById(R.id.btnGetUserInformation);
        btnGetUserInformation.setOnClickListener(this);
        btnGetLicenseInfo = (Button) findViewById(R.id.btnGetLicenseInfo);
        btnGetLicenseInfo.setOnClickListener(this);
        tvEmotivID = (TextView) findViewById(R.id.tvEmotivID);
    }

    private void connectToServer() {
        webSocketManager = WebSocketManager.getInstance();
        webSocketManager.connect();
    }

    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.btnGetUserLogin:
                CortexClient.getInstance().getUserLogin();
                break;
            case R.id.btnHasAccessRight:
                CortexClient.getInstance().hasAppAccess();
                break;
            case R.id.btnRequestAccess:
                CortexClient.getInstance().requestAppAccess();
                break;
            case R.id.btnAuthorize:
                CortexClient.getInstance().authorize();
                break;
            case R.id.btnGetUserInformation:
                CortexClient.getInstance().geUserInformation();
                break;
            case R.id.btnGetLicenseInfo:
                CortexClient.getInstance().getLicenseInformation();
                break;
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        CortexClientController.getInstance().setCortexClientInterface(this);
    }

    @Override
    public void onError(int errorCode, String errorMessage) {

    }

    @Override
    public void onSocketConnected() {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                progressBar.setVisibility(View.GONE);
                btnGetUserLogin.setEnabled(true);
                btnHasAccessRight.setEnabled(true);
                btnRequestAccess.setEnabled(true);
                btnAuthorize.setEnabled(true);
                btnGetUserInformation.setEnabled(true);
                btnGetLicenseInfo.setEnabled(true);
            }
        });
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
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                tvEmotivID.setText("EmotivID signed in: " + CortexClientController.getInstance().getDataStream().getAccessObject().getEmotivID());
            }
        });
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
}