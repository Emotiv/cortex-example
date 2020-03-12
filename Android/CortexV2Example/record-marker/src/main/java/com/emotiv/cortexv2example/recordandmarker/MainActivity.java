package com.emotiv.cortexv2example.recordandmarker;

import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.Toast;

import com.emotiv.cortexv2example.adapter.HeadsetListAdapter;
import com.emotiv.cortexv2example.controller.CortexClientController;
import com.emotiv.cortexv2example.interfaces.CortexClientInterface;
import com.emotiv.cortexv2example.objects.HeadsetObject;
import com.emotiv.cortexv2example.objects.SessionObject;
import com.emotiv.cortexv2example.utils.Constant;
import com.emotiv.cortexv2example.utils.Utilities;
import com.emotiv.cortexv2example.websocket.CortexClient;
import com.emotiv.cortexv2example.websocket.WebSocketManager;
import java.util.ArrayList;

public class MainActivity extends AppCompatActivity implements CortexClientInterface, View.OnClickListener {

    WebSocketManager webSocketManager;
    boolean hasAppAcess = false;
    String cortexToken = "";
    ProgressBar progressBar;
    Button btnQueryHeadsets, btnCreateSession, btnCreateRecord, btnInjectMarker, btnStopRecord;
    ListView headsetListView;
    private HeadsetListAdapter headsetListAdapter;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        initView();
        connectToServer();
        Toast.makeText(MainActivity.this, "Please make sure you have already connected Emotiv headset from Emotiv App", Toast.LENGTH_LONG).show();
    }

    private void initView() {
        progressBar = (ProgressBar) findViewById(R.id.progressBar);
        btnQueryHeadsets = (Button) findViewById(R.id.btnQueryHeadsets);
        btnQueryHeadsets.setOnClickListener(this);
        btnCreateSession = (Button) findViewById(R.id.btnCreateSession);
        btnCreateSession.setOnClickListener(this);

        headsetListView = (ListView) findViewById(R.id.headsetListView);
        headsetListAdapter = new HeadsetListAdapter(MainActivity.this, new ArrayList<HeadsetObject>());
        headsetListView.setAdapter(headsetListAdapter);
        headsetListView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                if (headsetListAdapter.getItem(position).getHeadsetStatus() != "connected" && headsetListAdapter.getItem(position).getHeadsetStatus() != "connecting")
                    CortexClient.getInstance().controlDevice("connect", headsetListAdapter.getItem(position).getHeadsetName());
            }
        });

        btnCreateRecord = (Button) findViewById(R.id.btnCreateRecord);
        btnCreateRecord.setOnClickListener(this);
        btnInjectMarker = (Button) findViewById(R.id.btnInjectMarker);
        btnInjectMarker.setOnClickListener(this);
        btnStopRecord = (Button) findViewById(R.id.btnStopRecord);
        btnStopRecord.setOnClickListener(this);
    }

    private void connectToServer() {
        webSocketManager = WebSocketManager.getInstance();
        webSocketManager.connect();
    }

    @Override
    protected void onResume() {
        super.onResume();
        Utilities.checkAndRequestPermissions(this);
        CortexClientController.getInstance().setCortexClientInterface(this);
    }

    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.btnQueryHeadsets:
                CortexClient.getInstance().queryHeadset();
                break;
            case R.id.btnCreateSession:
                CortexClient.getInstance().createSession(cortexToken, "active", HeadsetObject.getInstance().getHeadsetName());
                break;
            case R.id.btnCreateRecord:
                CortexClient.getInstance().createRecord(cortexToken, SessionObject.getInstance().getCurrentActivedSession(), Constant.RECORD_NAME, "example for recording");
                break;
            case R.id.btnInjectMarker:
                CortexClient.getInstance().injectMarker(cortexToken, SessionObject.getInstance().getCurrentActivedSession(), "v2example", "v2", "USB", System.currentTimeMillis());
                break;
            case R.id.btnStopRecord:
                CortexClient.getInstance().stopRecord(cortexToken, SessionObject.getInstance().getCurrentActivedSession());
                break;
        }
    }

    @Override
    public void onError(int errorCode, String errorMessage) {

    }

    @Override
    public void onSocketConnected() {
        CortexClient.getInstance().getUserLogin();
    }

    @Override
    public void onGetUserLoginOk() {
        CortexClient.getInstance().hasAppAccess();
    }

    @Override
    public void onHasAccessRightOk() {
        hasAppAcess = CortexClientController.getInstance().getDataStream().getAccessObject().isAccessGranted();
        if (hasAppAcess) {
            CortexClient.getInstance().authorize();
        } else {
            CortexClient.getInstance().requestAppAccess();
        }
    }

    @Override
    public void onRequestAccessOk() {
        CortexClient.getInstance().authorize();
    }

    @Override
    public void onAuthorizeOk() {
        cortexToken = CortexClientController.getInstance().getDataStream().getAccessObject().getCortexAccessToken();

        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                progressBar.setVisibility(View.GONE);
                btnQueryHeadsets.setEnabled(true);
                btnCreateSession.setEnabled(true);
                btnCreateRecord.setEnabled(true);
                btnInjectMarker.setEnabled(true);
                btnStopRecord.setEnabled(true);
            }
        });
    }

    @Override
    public void onGetUserInformationOk() {

    }

    @Override
    public void onGetLicenseInfoOk() {

    }

    @Override
    public void onQueryHeadsetOk() {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                headsetListAdapter.clear();
                headsetListAdapter.addAll(HeadsetObject.getInstance().getHeadsetList());
                headsetListAdapter.notifyDataSetChanged();
            }
        });
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
