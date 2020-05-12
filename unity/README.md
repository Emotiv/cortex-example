# Emotiv Unity Example

These examples demontrate an UnityApp work with Emotiv CortexService(shortly called Cortex) whose APIs are described at [Cortex Document](https://app.gitbook.com/@emotiv/s/cortex-api/)

## Prerequisites

* You must install Unity. You can get it for free at [www.unity3d.com](https://unity3d.com/get-unity/download).
* You might update submodule to get latest version of emotiv unity plugin.
```
       git submodule update --init
```
* The Cortex have to be running on your machine such as service. You can get the Cortex from (https://www.emotiv.com/developer/).
* Register a Application at https://www.emotiv.com/my-account/cortex-apps/ and get a pair of client id and client secret. You must connect to your Emotiv account on emotiv.com and create a Cortex app. If you don't have a EmotivID, you can [register here](https://id.emotivcloud.com/eoidc/account/registration/).
* We have updated our Terms of Use, Privacy Policy and EULA to comply with GDPR. Please login via EMOTIV App to read and accept our latest policies in order to proceed using the following examples.

## How to compile
<!-- how to compile  -->
1. Open EmotivUnityPlugin.unity by Unity Editor
3. Login via EMOTIV App and put  your client id and client secret to AppConfig.cs. 
4. You can run the examples directly from the Unity Editor.


## Code structure
The example will use emotiv unity plugin to work with Cortex.

There are some main controller scripts and group of scripts are:

**ConnectToCortex.cs**: To configure App and start connecting to Cortex.

**1_Cortex**: Contain commponent scripts to control from Cortex connecting to authorizing procedure.

**ConnectHeadset**: Contain commponent scripts to create headset element and list headsets information.

**ConnectionIndicator**: Contain commponent scripts to show battery indicator after headset connected and subscribed device information successfully.

**ContactQuality**: Contain commponent scripts to show contact quality of headset sensors.

**DataSubscriber.cs**: The script to show subscribe and unsubscribe data. The header and data of corresponding streams will be displayed and updated. Note that the MARKERS channel of EEG data will not been displayed.

**Emotiv-Unity-Plugin**: The plugin help the application to work with Cortex. Please refer to [emotiv-unity-plugin](https://github.com/Emotiv/unity-plugin)

## How to use
Please follow the below steps:
1. Put clientId, clientSecret of your App to AppConfig.cs. You also can configure application name, application version and TmpAppDataDir to create logs folder.
2. You make sure have logined via EmotivApp.
3. Click run the Example from editor. Firstly, the example will connect to Cortex then process authorizing procedure. 
You might need to grant access right for the example via EmotivApp at the first time. Afterthat, the example will authorize to get the cortex token to work with Cortex. The token will be saved for next using.
After authorizing successfully, the example will find headsets and list all to screen. 
4. Click on a headet button to create a working session with the given headset and subscribe device information . It is default data stream types.
5. You will move to sceen show your contact quality. Please make sure the headset is at good contact quality. Click Done button when you finish
6. Now, you can subscribe or unsubscribe more data such as : EEG, Motion and Performance metrics.
7. When you run the application at standalone mode, the log files will be located at "%LocalAppData%/${TmpAppDataDir}/logs/" on Windows and "~/Library/Application Support/${TmpAppDataDir}/logs"

## Change logs

[May 8, 2020]

The example support the following features:
- Subscribe data streams: EEG, Motion, Performance metric, Device information
- Support for EPOC, EPOC+, EPOCX, INSIGHT headset only. The others will be supported later.

## Authors

* **TungNguyen**

See also the list of [contributors](https://github.com/Emotiv/unity-plugin/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/Emotiv/cortex-v2-example/blob/master/LICENSE) file for details
