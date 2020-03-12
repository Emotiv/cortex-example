package com.emotiv.cortexv2example.objects;

public class SessionObject {
    public static SessionObject sessionObject;
    private String currentActivedSession = "";
    private String currentSessonStatus = "";

    public SessionObject() {

    }

    public static SessionObject getInstance() {
        if (sessionObject == null) {
            sessionObject = new SessionObject();
        }
        return sessionObject;
    }

    public String getCurrentActivedSession() {
        return currentActivedSession;
    }

    public void setCurrentActivedSession(String currentActivedSession) {
        this.currentActivedSession = currentActivedSession;
    }

    public String getCurrentSessonStatus() {
        return currentSessonStatus;
    }

    public void setCurrentSessonStatus(String currentSessonStatus) {
        this.currentSessonStatus = currentSessonStatus;
    }

    public void clearData() {
        setCurrentActivedSession("");
        setCurrentSessonStatus("");
    }

    public boolean isValidSession() {
        return !getInstance().getCurrentActivedSession().equals("") && getInstance().getCurrentSessonStatus().equals("activated");
    }
}
