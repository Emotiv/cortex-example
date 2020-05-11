package com.emotiv.cortexv2example.eeg;

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
import com.emotiv.cortexv2example.websocket.CortexClient;
import com.emotiv.cortexv2example.websocket.WebSocketManager;
import java.util.ArrayList;
import java.util.List;

public class MainActivity extends AppCompatActivity implements CortexClientInterface, View.OnClickListener {

    boolean hasAppAcess = false;
    String cortexToken = "";
    ProgressBar progressBar;
    ListView headsetListView;
    private HeadsetListAdapter headsetListAdapter;
    Button btnQueryHeadsets, btnCreateSession, btnSubscribeEeg, btnUnSubscribeEeg;
    WebSocketManager webSocketManager;
    List<String> streamArray = new ArrayList<String>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        initView();
        connectToServer();
    }

    private void initView() {
        streamArray.add("eeg");
        progressBar = (ProgressBar) findViewById(R.id.progressBar);
        btnQueryHeadsets = (Button) findViewById(R.id.btnQueryHeadsets);
        btnQueryHeadsets.setOnClickListener(this);
        btnCreateSession = (Button) findViewById(R.id.btnCreateSession);
        btnCreateSession.setOnClickListener(this);
        btnSubscribeEeg = (Button) findViewById(R.id.btnSubscribeEeg);
        btnSubscribeEeg.setOnClickListener(this);
        btnUnSubscribeEeg = (Button) findViewById(R.id.btnUnSubscribeEeg);
        btnUnSubscribeEeg.setOnClickListener(this);

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
    }

    private void connectToServer() {
        webSocketManager = WebSocketManager.getInstance();
        webSocketManager.connect();
    }

    @Override
    protected void onResume() {
        super.onResume();
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
            case R.id.btnSubscribeEeg:
                CortexClient.getInstance().subscribeData(cortexToken, SessionObject.getInstance().getCurrentActivedSession(), streamArray);
                break;
            case R.id.btnUnSubscribeEeg:
                CortexClient.getInstance().unSubscribeData(cortexToken, SessionObject.getInstance().getCurrentActivedSession(), streamArray);
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
                btnSubscribeEeg.setEnabled(true);
                btnUnSubscribeEeg.setEnabled(true);
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
