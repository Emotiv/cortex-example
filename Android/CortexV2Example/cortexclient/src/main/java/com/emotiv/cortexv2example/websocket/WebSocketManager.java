package com.emotiv.cortexv2example.websocket;

import android.content.Context;
import android.os.StrictMode;
import android.util.Log;
import com.emotiv.cortexv2example.application.MyApplication;
import com.emotiv.cortexv2example.interfaces.WebSocketInterface;
import com.emotiv.cortexv2example.utils.Constant;
import org.java_websocket.client.DefaultSSLWebSocketClientFactory;
import org.java_websocket.client.WebSocketClient;
import org.java_websocket.drafts.Draft_17;
import org.java_websocket.handshake.ServerHandshake;
import org.json.JSONObject;
import java.net.URI;
import javax.net.ssl.SSLContext;

public class WebSocketManager {
    private final String TAG = WebSocketManager.class.getName();
    private Context mContext;
    private String mUri;
    private static WebSocketManager mWebSocketManager;
    private WebSocketInterface mWebSocketInterface;
    private WebSocketClient mWebSocketClient;
    private int currentStresmID = -1;
    private int currentRequestID = -1;

    public WebSocketManager(Context context, String uri) {
        this.mContext = context;
        this.mUri = uri;
        StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().permitAll().build();
        StrictMode.setThreadPolicy(policy);
        try {
            initWebSocket(this.mUri);
        } catch (Exception e) { e.printStackTrace(); }
    }

    public static WebSocketManager getInstance() {
        if (mWebSocketManager == null){
            mWebSocketManager = new WebSocketManager(MyApplication.getCurrentContext(), Constant.SOCKET_URL);
        }
        return mWebSocketManager;
    }

    public void setWebSocketInterface(WebSocketInterface webSocketInterface) {
        this.mWebSocketInterface = webSocketInterface;
    }

    private void initWebSocket(String uri) throws Exception {
        mWebSocketClient = new WebSocketClient(new URI(uri), new Draft_17()) {
            @Override
            public void onOpen(ServerHandshake serverHandshake) {
                if (mWebSocketInterface != null) {
                    Log.i(TAG, "WebSocket Opened");
                    mWebSocketInterface.onWebSocketOpened();
                }
            }

            @Override
            public void onMessage(String s) {
                if (mWebSocketInterface != null) {
                    updateNewMessage(s);
                }
            }

            @Override
            public void onClose(int i, String s, boolean b) {
                if (mWebSocketInterface != null) {
                    Log.i(TAG, "WebSocket Closed");
                    mWebSocketInterface.onWebSocketClosed();
                }
            }

            @Override
            public void onError(Exception e) {
                if (mWebSocketInterface != null) {
                    Log.e(TAG, "WebSocket Error " + e.getMessage());
                    mWebSocketInterface.onWebSocketError(e.getMessage());
                }
            }
        };

        SSLContext sslContext = SSLContext.getDefault();
        mWebSocketClient.setWebSocketFactory(new DefaultSSLWebSocketClientFactory(sslContext));
    }

    public int getCurrentStresmID() {
        return currentStresmID;
    }

    public int getCurrentRequestID() {
        return currentRequestID;
    }

    public boolean isConnected() {
        return mWebSocketClient.getConnection().isOpen();
    }

    public void connect() {
        if (mWebSocketClient != null) {
            mWebSocketClient.connect();
        }
    }

    private void closeConnection() {
        if (mWebSocketClient != null) {
            mWebSocketClient.close();
        }
    }

    // a generic method to send a RPC request to Cortex
    public void sendRequest(int streamID, int requestID, String method, JSONObject params, boolean hasParam) {
        try {
            if (isConnected() && mWebSocketClient != null) {
                currentStresmID = streamID;
                currentRequestID = requestID;
                JSONObject contentRequest = createHeaderForRequest(method, requestID);
                if (hasParam && params != null) {
                    contentRequest.put("params", params);
                }
                mWebSocketClient.send(contentRequest.toString());
                Log.i(TAG, "method: " + method + " | request: " + contentRequest.toString());
            }
        } catch (Exception e) { e.printStackTrace(); }
    }

    // method to create header for a RPC request
    private JSONObject createHeaderForRequest(String method, int requestID) {
        JSONObject headerRequest = new JSONObject();
        try {
            headerRequest.put("id", requestID);
            headerRequest.put("jsonrpc", "2.0");
            headerRequest.put("method", method);
            return headerRequest;
        } catch (Exception e) { e.printStackTrace(); }
        return null;
    }

    private void updateNewMessage(String s) {
        try {
            JSONObject jsonObj = new JSONObject(s);
            // received warning message in response to a RPC request
            if (jsonObj.has("warning")) {
                mWebSocketInterface.onRecievedWarningMessage(s);
            }
            // received data from a data stream
            else if (jsonObj.has("sid")) {
                mWebSocketInterface.onRecievedNewData(s);
            }
            // received message in response to a RPC request
            else if (jsonObj.has("id")) {
                mWebSocketInterface.onRecievedMessage(s);
            }
        } catch (Exception e) { e.printStackTrace(); }
    }
}