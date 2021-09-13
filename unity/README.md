# Emotiv Unity Example

This example demonstrates how to work with Emotiv Cortex Service (aka Cortex) in Unity.

## Prerequisites

* Install Unity. You can get it for free at [www.unity3d.com](https://unity3d.com/get-unity/download).
* Get the latest version of [Emotiv Unity Plugin](https://github.com/Emotiv/unity-plugin) as a submodule.
```
       git submodule update --init
```
* Install and run the EMOTIV Launcher with Cortex from (https://www.emotiv.com/developer/).
* Login to the Emotiv website with a valid EmotivID, register an application at https://www.emotiv.com/my-account/cortex-apps/ to a pair of client id and client secret. If you don't have a EmotivID, you can [register here](https://id.emotivcloud.com/eoidc/account/registration/).
* We have updated our Terms of Use, Privacy Policy and EULA to comply with GDPR. Please login via EMOTIV Launcher to read and accept our latest policies in order to proceed with the following examples.

## How to compile
<!-- how to compile  -->
1. Open EmotivUnityPlugin.unity by Unity Editor.
1. Put your client id and client secret to AppConfig.cs.
1. You can run the examples directly from the Unity Editor.


## Code structure

There are some main controller scripts:

**ConnectToCortex.cs**: Initialize connection to Cortex.

**1_Cortex**: Contain commponent scripts to control authorization procedure.

**ConnectHeadset**: Contain commponent scripts to create headset element and list headset information.

**ConnectionIndicator**: Contain commponent scripts to show battery indicator, after headset is connected and device information is subscribed.

**ContactQuality**: Contain commponent scripts to show contact quality of headset sensors.

**DataSubscriber.cs**: The script to show subscribe and unsubscribe data. The header and data of corresponding streams will be displayed and updated. Note that the MARKERS channel of EEG data will not be shown.

**MarkersDemo.cs**: Show how to inject markers and update markers to the EEG data stream and show the output log for startRecord, stopRecord, injectMarker and updateMarker both result and error messages.

**Emotiv-Unity-Plugin**: The plugin that works behind the scene. Please refer to [Emotiv Unity Plugin](https://github.com/Emotiv/unity-plugin).

## How to use
1. Put clientId, clientSecret of your application in AppConfig.cs. To subscribe EEG data or inject markers to EEG data stream you need to put a PRO license at "AppLicenseId" in AppConfig.cs. In addition, You also can customize application name, version and TmpAppDataDir to create log directory.
1. Make sure you have logged in via EMOTIV Launcher, and the headset has been turned on.
1. Run the example from editor. The example will connect to Cortex for authorization. You might need to grant access right for the example via EMOTIV Launcher at the first time. After that, the example will get the token to work with Cortex. The token will be saved for subsequent use.
After authorizing successfully, the example will list available headsets. 
1. At headset list screen, hit "Connect" button to connect, create a working session with that headset, and subscribe device information (Contact Quality).
1. Please make sure the headset is at good contact quality and hit "Done" button to enter "Examples Board".
1. Some examples are listed on the board:
    1. Data Subscriber Example: Demo for EEG, Motion and Performance Metrics data subscribing.
    1. Markers Example: Show how to inject markers to the EEG data stream and update the current marker.
1. When you run the application at standalone mode, the log files will be located at "%LocalAppData%/${TmpAppDataDir}/logs/" on Windows or "~/Library/Application Support/${TmpAppDataDir}/logs" on macOS.

## Change log

[10 July 2021]
- Support inject marker and update marker to the EEG data stream.

[19 Apr 2021]
- Support new channel BateryPercent of "dev" stream which introduced from 2.7.0
- Fix some bugs related to data buffer and cortexclient at unity-plugin

[12 May 2020]
- Subscribe data streams: EEG, Motion, Performance Metric, Device information
- Supports EPOC, EPOC+, EPOC X and Insight.

## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/Emotiv/cortex-v2-example/blob/master/LICENSE) file for details
