package com.emotiv.cortexv2example.utils;

import android.Manifest;
import android.app.Activity;
import android.content.pm.PackageManager;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import java.util.ArrayList;
import java.util.List;

public class Utilities {

    /* permissions */
    public static final String [] appPermissions = {
            Manifest.permission.WRITE_EXTERNAL_STORAGE,
            Manifest.permission.READ_EXTERNAL_STORAGE
    };

    public static boolean checkAndRequestPermissions(Activity activity) {
        List<String> listPermissionsNeed = new ArrayList<>();
        for (String perm : appPermissions) {
            if (ContextCompat.checkSelfPermission(activity.getApplicationContext(), perm) != PackageManager.PERMISSION_GRANTED) {
                listPermissionsNeed.add(perm);
            }
        }

        if (!listPermissionsNeed.isEmpty()) {
            ActivityCompat.requestPermissions(activity, listPermissionsNeed.toArray(new String[listPermissionsNeed.size()]), 1001);
            return false;
        }
        return true;
    }
}
