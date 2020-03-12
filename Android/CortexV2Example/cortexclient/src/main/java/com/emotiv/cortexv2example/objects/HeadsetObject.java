package com.emotiv.cortexv2example.objects;

import java.util.ArrayList;
import java.util.List;

public class HeadsetObject {
    public static HeadsetObject headsetObject;
    String headsetName = "";
    String headsetConnectedBy = "";
    String headsetStatus = "";
    boolean isConnected = false;
    List<HeadsetObject> headsetList = new ArrayList<HeadsetObject>();

    public HeadsetObject() {

    }

    public static HeadsetObject getInstance() {
        if (headsetObject == null) {
            headsetObject = new HeadsetObject();
        }
        return headsetObject;
    }

    public void clearData() {
        setConnected(false);
        setHeadsetConnectedBy("");
        setHeadsetName("");
        setHeadsetStatus("");
        getHeadsetList().clear();
    }


    public String getHeadsetName() {
        return headsetName;
    }

    public void setHeadsetName(String headsetName) {
        this.headsetName = headsetName;
    }

    public String getHeadsetConnectedBy() {
        return headsetConnectedBy;
    }

    public void setHeadsetConnectedBy(String headsetConnectedBy) {
        this.headsetConnectedBy = headsetConnectedBy;
    }

    public String getHeadsetStatus() {
        return headsetStatus;
    }

    public void setHeadsetStatus(String headsetStatus) {
        this.headsetStatus = headsetStatus;
    }

    public List<HeadsetObject> getHeadsetList() {
        return headsetList;
    }

    public void setHeadsetList(List<HeadsetObject> headsetList) {
        this.headsetList = headsetList;
    }

    public boolean isConnected() {
        return isConnected;
    }

    public void setConnected(boolean connected) {
        isConnected = connected;
    }
}
