package com.emotiv.cortexv2example.application;

import android.app.Application;
import android.content.Context;

public class MyApplication extends Application {

    private static Context mContext;

    @Override
    public void onCreate() {
        mContext = getApplicationContext();
        super.onCreate();
    }
    public static Context getCurrentContext() {
        return mContext;
    }
}