package com.emotiv.cortexv2example.headsetandsession;

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
import com.emotiv.cortexv2example.utils.Utilities;
import com.emotiv.cortexv2example.websocket.CortexClient;
import com.emotiv.cortexv2example.websocket.WebSocketManager;
import java.util.ArrayList;

public class MainActivity extends AppCompatActivity implements CortexClientInterface, View.OnClickListener {

    WebSocketManager webSocketManager;
    ListView headsetListView;
    private HeadsetListAdapter headsetListAdapter;
    Button btnQueryHeadsets, btnCreateSession, btnUpdateSession;
    boolean hasAppAcess = false;
    String cortexToken = "";
    ProgressBar progressBar;
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
        btnQueryHeadsets = (Button) findViewById(R.id.btnQueryHeadsets);
        btnQueryHeadsets.setOnClickListener(this);
        btnCreateSession = (Button) findViewById(R.id.btnCreateSession);
        btnCreateSession.setOnClickListener(this);
        btnUpdateSession = (Button) findViewById(R.id.btnUpdateSession);
        btnUpdateSession.setOnClickListener(this);
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
            case R.id.btnUpdateSession:
                CortexClient.getInstance().updateSession(cortexToken, "close", SessionObject.getInstance().getCurrentActivedSession());
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
                btnUpdateSession.setEnabled(true);
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
