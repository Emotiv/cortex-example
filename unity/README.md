# Emotiv Unity Example

This example demonstrates how to work with the Emotiv Cortex Service (Cortex) in Unity.

## Prerequisites

* **Install Unity:** You can download it for free at [www.unity3d.com](https://unity3d.com/get-unity/download).
* **Get the Emotiv Unity Plugin:** Obtain the latest version of the [Emotiv Unity Plugin](https://github.com/Emotiv/unity-plugin) as a submodule:
    ```
    git submodule update --init
    ```
* **Install and Run EMOTIV Launcher with Cortex:** Download and run the EMOTIV Launcher with Cortex from [https://www.emotiv.com/developer/](https://www.emotiv.com/developer/).
* **Emotiv Account and Application:**
    * Log in to the Emotiv website with your valid EmotivID. If you don't have one, you can [register here](https://id.emotivcloud.com/eoidc/account/registration/).
    * Register an application at [https://www.emotiv.com/my-account/cortex-apps/](https://www.emotiv.com/my-account/cortex-apps/) to obtain your `clientId` and `clientSecret`.
* **Accept Updated Policies:** Please log in via the EMOTIV Launcher to read and accept our latest Terms of Use, Privacy Policy, and EULA to proceed with the following examples. This is required due to GDPR compliance.

## How to Use

1.  **Open the Example Scene:** Open the **SimpleExample.unity** scene. This scene contains a comprehensive demonstration.
2.  **Set Credentials:** In the **AppConfig.cs** script, locate and set the `clientId` and `clientSecret` with the values from your registered application before running the scene.
3.  **Query Headsets:** Run the example. Once authorization is complete, click the **"Query Headset"** button to list available headsets.
4.  **Create a Session:**
    * Enter the ID of the desired headset (e.g., "INSIGHT-A12345") in the text field next to the **"Create Session"** button.
    * If the text field is left empty, the first headset in the queried list will be used by default.
    * Click the **"Create Session"** button to connect to the headset and establish a session.
5.  **Interact with Data and Training:** After successfully creating a session, you can perform the following actions:
    * **Start and Stop Recording:** Enter a title for your recording in the designated field before clicking **"Start Record"**. An optional description can also be added. Use the **"Stop Record"** button to end the recording.
    * **Inject Marker:** While a recording is active, you can inject instance markers. Enter a **"marker value"** and a **"marker label"** and then click the **"Inject Marker"** button.
    * **Subscribe and Unsubscribe Data Streams:** Select the desired data streams from the available options before clicking the **"Subscribe Data"** button. The received data will be displayed in the log box. You can unsubscribe by clicking the **"Unsubscribe Data"** button.
    * **Load Profile and Training:**
        * Enter a **"profile name"** before clicking **"Load Profile"**. If a profile with that name does not exist, it will be created and then loaded.
        * **Important:** Subscribe to the **"System Event"** data stream to observe training events.
        * Select a mental command for training from the dropdown menu and click **"Start Training"**.
        * You might observe a **"MC\_Succeeded"** event after approximately 8 seconds, indicating a successful training attempt. You can then choose to accept or reject the training.
        * After training, click **"Save Profile"** to persist the trained data.
        * **Important:** Before closing the application, click **"Unload Profile"** to release the trained data.

> **Please Note:**
>
> * **Headset Scanning (Cortex 3.7+):** Starting from Emotiv Cortex 3.7, you need to call RefreshHeadset() to scan bluetooth devices. But the process is proceed automatically after authorization.
> * **Interface:** This example utilizes **EmotivUnityItf.cs** as an interface to interact with the Emotiv Cortex Service.
> * **Data Buffering:** By default, subscribed data is not saved to a data buffer and is only displayed in the message log box. You can enable saving data to a buffer by setting `IsDataBufferUsing = true` in the `AppConfig.cs` file.
> * **Mental Command and Facial Expression:** Ensure you load a trained profile before subscribing to **"Mental Command"** or **"Facial Expression"** data streams; otherwise, you will only observe neutral actions.

## Change Log

**[15 March 2024]**

* Stopped support for `EmotivUnityPlugin.unity` and removed obsolete files.
* Bug fixes.
* Internal support for working with the embedded Cortex library (internal use only).

**[10 Nov 2023]**

* Implemented support for the new headset scanning flow introduced in Emotiv Cortex 3.7.

**[15 May 2022]**

* Added `SimpleExample.unity` to demonstrate simultaneous data subscription, training, start recording, and marker injection.
* Removed some unused files and updated the Unity version to a newer LTS version (2021.3.2f1).

**[15 Jan 2022]**

* Added support for Insight 2 headset in the Unity examples.

**[10 July 2021]**

* Implemented support for injecting and updating markers within the EEG data stream.

**[19 Apr 2021]**

* Added support for the new `BateryPercent` channel in the "dev" stream, introduced in version 2.7.0.
* Fixed several bugs related to the data buffer and Cortex client within the `unity-plugin`.

**[12 May 2020]**

* Implemented subscription for the following data streams: EEG, Motion, Performance Metric, Device information.
* Added support for EPOC, EPOC+, EPOC X, and Insight headsets.

## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/Emotiv/cortex-v2-example/blob/master/LICENSE) file for details.