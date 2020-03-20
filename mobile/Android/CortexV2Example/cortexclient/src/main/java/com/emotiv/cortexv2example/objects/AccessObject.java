package com.emotiv.cortexv2example.objects;

public class AccessObject {
    public static AccessObject accessObject;

    public AccessObject() {

    }

    public static AccessObject getInstance() {
        if (accessObject == null) {
            accessObject = new AccessObject();
        }
        return accessObject;
    }

    String emotivID = "";
    boolean isLogin = false;
    String firstName = "";
    String lastName = "";
    String email = "";
    String cortexAccessToken = "";
    boolean accessGranted = false;

    public String getEmotivID() {
        return emotivID;
    }

    public void setEmotivID(String emotivID) {
        this.emotivID = emotivID;
    }

    public boolean isLogin() {
        return isLogin;
    }

    public void setLogin(boolean login) {
        isLogin = login;
    }

    public String getFirstName() {
        return firstName;
    }

    public void setFirstName(String firstName) {
        this.firstName = firstName;
    }

    public String getLastName() {
        return lastName;
    }

    public void setLastName(String lastName) {
        this.lastName = lastName;
    }

    public String getCortexAccessToken() {
        return cortexAccessToken;
    }

    public void setCortexAccessToken(String cortexAccessToken) {
        this.cortexAccessToken = cortexAccessToken;
    }

    public boolean isAccessGranted() {
        return accessGranted;
    }

    public void setAccessGranted(boolean accessGranted) {
        this.accessGranted = accessGranted;
    }

    public String getEmail() {
        return email;
    }

    public void setEmail(String email) {
        this.email = email;
    }
}
