package com.emotiv.cortexv2example.interfaces;

public abstract interface WarningInterface {
   // received a warning message in response to a RPC request
   void onNewWarning(int warningCode, String warningMessage);
}
