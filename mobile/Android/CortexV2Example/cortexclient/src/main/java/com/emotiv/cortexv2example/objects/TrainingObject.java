package com.emotiv.cortexv2example.objects;

public class TrainingObject {
    public static TrainingObject trainingObject;
    private String currentProfile = "";
    private String trainingAction = "";
    private String currentDetection = "";
    private String currentTrainingEvent = "";

    public TrainingObject() {

    }

    public String getCurrentTrainingEvent() {
        return currentTrainingEvent;
    }

    public void setCurrentTrainingEvent(String curTrainingEvent) {
        this.currentTrainingEvent = curTrainingEvent;
    }

    public String getTrainingAction() {
        return trainingAction;
    }

    public void setTrainingAction(String trainingAction) {
        this.trainingAction = trainingAction;
    }

    public String getCurrentDetection() {
        return currentDetection;
    }

    public void setCurrentDetection(String currentDetection) {
        this.currentDetection = currentDetection;
    }

    public String getCurrentProfile() {
        return currentProfile;
    }

    public void setCurrentProfile(String currentProfile) {
        this.currentProfile = currentProfile;
    }

    public static TrainingObject getInstance() {
        if (trainingObject == null) {
            trainingObject = new TrainingObject();
        }
        return trainingObject;
    }
}