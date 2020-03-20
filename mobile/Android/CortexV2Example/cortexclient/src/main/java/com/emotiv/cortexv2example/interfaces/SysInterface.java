package com.emotiv.cortexv2example.interfaces;

public abstract interface SysInterface {
    // received data from a data stream
    void onEegDataRecieved(String eEgData);
    void onMotDataRecieved(String motData);
    void onDevDataRecieved(String devData);
    void onMetDataRecieved(String metData);
    void onComDataRecieved(String comData);
    void onFacDataRecieved(String facData);
    void onTrainingStarted(String detection, String action, String event);
    void onTrainingFailed();
    void onTrainingSucceeded();
    void onTrainingCompleted();
    void onTrainingRejected();
    void onTrainingReset();
}
