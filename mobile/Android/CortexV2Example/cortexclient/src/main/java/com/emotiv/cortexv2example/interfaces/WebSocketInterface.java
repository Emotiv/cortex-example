package com.emotiv.cortexv2example.interfaces;

public abstract interface WebSocketInterface {
    void onWebSocketOpened();
    void onWebSocketClosed();
    void onWebSocketError(String error);
    void onRecievedMessage(String msg);
    void onRecievedWarningMessage(String msg);
    void onRecievedNewData(String msg);
}
