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


## How to use
There are 2 examples to demo how to work with Emotiv Cortex on Unity. The first one is EmotivUnityPlugin.unity which aim to demo data subscribing and marker injection. The second one is SimpleExample.unity which has simple UI but demo data subscribing, marker injection and training mentalcommand data.  

### Example 1: EmotivUnityPlugin.unity
1. Open **EmotivUnityPlugin.unity** scene.  
2. Set the clientId, clientSecret of your application in AppConfig.cs before running.  
3. Running the example. After authorzing process, you will see the Headset list screen. Hit **"Connect"** button to connect to your wanted headset. The unity-plugin will help to connect to headset, create a session and subscribe device information (dev).  
4. The next screens will show device fitting and contact quality for the headset. The headset should have good contact quality. Hit the **"Done"** button to go to Example Board screen.  
5. You can choose "Data Subscribers Example" or "Markers Example"  
	- **"Data Subscribers Example":** demo subscribe and unsubscribe EEG, Motion and Performance metrics data. But you can subscribe other streams with some change at DataSubscriber.cs  
	- **"Markers Example":** demo create record, inject marker and update the current marker. You must create a record first before injecting marker.

> **Please note that:**
>
> - The subscribed data will be saved to DataBuffer then will be pulled via Update() function at DataSubscriber.cs. Currently, the new data will be shown each 1 second.  
> - You can click to the Contact Quality indicator at the right top screen to back to Contact Quality screen to able to switch example.  
> - The **"Update Marker"** will make the instance marker to interval marker which have start and end time.  

### Example 2: SimpleExample.unity 
1. Open **SimpleExample.unity** scene. It is all in one example.
2. Set the clientId, clientSecret of your application in SimpleExample.cs before running.
3. Running the example. After authorizing process you able to create session with a headset. Enter a headset Id before clicking **"Create Session"** button to connect and create a session with the headset. If the text field is empty, the first headset in the headset list will be used.  
4. After create session successfully, you will be able to start a record, subscribe one ore more data streams and load a profile for training.
	- **Start and Stop Record:** Enter record title before starting a record, record description is optional field.
	- **Inject marker:** After start a record you can inject instance marker to the record. Please enter marker value and marker label before injecting marker.
	- **Subscribe and Unsubscribe data:** Select wanted data streams before subscribing data. The output data wil be shown at log box.
	- **Load profile and Training:**
		- Enter a profile name before loading profile. If the profile is not existed, it will be created then loaded.  
		- Please subscribe the "System Event" data stream before training to see the training event.
		- Select a mental command training at Dropdown then click "Start Training".
		- You might see the event "MC_Succeeded" after 8 seconds. You can accept or reject the training.
		- After traing please click "Save Profile" to save training data.
		- Please unload the trained data before closing.  
		
> **Please note that:**
> - From Emotiv Cortex 3.7, you need to call ScanHeadsets() at DataStreamManager.cs to start headset scanning. Otherwise your headsets might not appeared in the headset list return from queryHeadsets(). If IsHeadsetScanning = false, you need re-call the ScanHeadsets() if want to re-scan headsets again but should not call ScanHeadsets() when has a headset connected.
> - The example will use **EmotivUnityItf.cs** such as a interface to do all things.
> - The subscribed data will be saved to DataBuffer as default. But you have option use data directly without DataBuffer by set '_isDataBufferUsing = false'. Please check the ouput for subcribed data at functions such as OnDevDataReceived(), OnEEGDataReceived().etc.. in EmotivUnityItf.cs  
> - Please load a trained profile before subscribing **"Mental Command"** or **"Facial Expression"** data unless you only see the neutral action.


## Change log

[10 Nov 2023]
- Support new headset scanning flow from Emotiv Cortex 3.7

[15 May 2022]
- Add SimpleExample.unity to demo subscribe data, training , start record and inject marker same time.
- Remove some unused files and update Unity version to newer version (LTS 2021.3.2f1)

[15 Jan 2022]
- Support insight 2 for unity examples.

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
