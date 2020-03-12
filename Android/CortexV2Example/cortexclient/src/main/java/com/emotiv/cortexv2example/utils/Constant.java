package com.emotiv.cortexv2example.utils;

import android.Manifest;

public class Constant {
    // local cortex service
    public static String SOCKET_URL = "wss://localhost:6868";

    /*
     * To get a client id and a client secret, you must connect to your Emotiv
     * account on emotiv.com and create a Cortex app.
     * https://www.emotiv.com/my-account/cortex-apps/
     */
    public final static String CLIENT_ID = "YWqjZqG9NbZYwltaQwRHTOvWsfR6Pb5po4sakSMB";
    public final static String CLIENT_SECRET = "V71BeDjbX1oBmTv1hYzCm6d6CpiOX6MUkgelec22Plrt54io5yfNKpYStSvYEkFR9t5HiFKnIXuIFljLFIF3MpDr8l6IY5EGRXNJIRLWXMIHhBon2j2210j9uYsNtv9d";

    // The name of the training profile used for the facial expression and mental command
    public final static String TRAINING_PROFILE_NAME = "cortex-v2-example";

    // The name of a record
    public final static String RECORD_NAME = "record-cortex-v2-example";

    /* Stream IDs */
    public final static int ACCESS_STREAM = 100;
    public final static int HEADSET_STREAM = 200;
    public final static int SESSION_STREAM = 300;
    public final static int RECORD_STREAM = 400;
    public final static int SUBSCRIBE_STREAM = 500;
    public final static int TRAINING_PROFILE_STREAM = 600;

    /* Request IDs */
    public final static int GET_USER_LOGGED_IN_REQUEST_ID = 1;
    public final static int HAS_APP_ACCESS_REQUEST_ID = 2;
    public final static int REQUEST_APP_ACCESS_REQUEST_ID = 3;
    public final static int AUTHORIZE_REQUEST_ID = 4;
    public final static int GET_USER_INFORMATION_REQUEST_ID = 5;
    public final static int GET_LICENSE_INFORMATION_REQUEST_ID = 6;
    public final static int QUERY_HEADSET_REQUEST_ID = 7;
    public final static int CONTROL_DEVICE_REQUEST_ID = 8;
    public final static int CREATE_SESSION_REQUEST_ID = 9;
    public final static int UPDATE_SESSION_REQUEST_ID = 10;
    public final static int CREATE_RECORD_REQUEST_ID = 11;
    public final static int STOP_RECORD_REQUEST_ID = 12;
    public final static int INJECT_MARKER_REQUEST_ID = 13;
    public final static int SUBSCRIBE_DATA_REQUEST_ID = 14;
    public final static int UNSUBSCRIBE_DATA_REQUEST_ID = 15;
    public final static int CREATE_PROFILE_REQUEST_ID = 16;
    public final static int LOAD_PROFILE_REQUEST_ID = 17;
    public final static int GET_CURRENT_PROFILE_REQUEST_ID = 18;
    public final static int TRAINING_PROFILE_MC_REQUEST_ID = 19;
    public final static int TRAINING_PROFILE_FE_REQUEST_ID = 20;
    public final static int ACCEPT_TRAINING_PROFILE_REQUEST_ID = 21;
    public final static int SAVE_TRAINING_PROFILE_REQUEST_ID = 22;

    /* Warning code */
    public final static int APP_ACCESS_APPROVED = 9;
}
