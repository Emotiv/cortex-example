
# Emotiv Unity Example

This example demonstrates how to work with the Emotiv Cortex Service (Cortex) and Emotiv Embedded Library in Unity, supporting Desktop (Windows/macOS), Android, and iOS platforms.

## Platform Support, Requirements & Prerequisites

- **Desktop (Windows/macOS):**
  - Works with Emotiv Cortex Service via EMOTIV Launcher.
  - Ensure `USE_EMBEDDED_LIB` is **not** defined in your Unity project.
  - **Embedded Library for Desktop is under development and not ready for production.**
- **Android:**
  - Supported via Emotiv Embedded Library.
  - You must request Bluetooth and Location permissions at runtime.
  - See [Unity Android Permissions](https://docs.unity3d.com/Manual/android-RequestingPermissions.html).
  
- **iOS:**
  - Supported via Emotiv Embedded Library.
  - After building, export to Xcode and ensure Bluetooth permissions are set in `Info.plist`.
  - The Emotiv Embedded Library `EmotivCortexLib.xcframework` must be included in your Xcode project.
  - **Xcode Version:** Use an Xcode version compatible with your Unity version. For example, Unity 6.0 (6000.0) generally requires Xcode 15+ or Xcode 16+ for iOS builds.

- For **desktop**, ensure the EMOTIV Launcher (Cortex Service) is installed and running. But the UniWebView submodule is not required for desktop, so you only need to pull the `unity-plugin` submodule as shown below:
    ```
      git submodule update --init
    ```
- For **mobile (Android/iOS)**, you do **not** need the EMOTIV Launcher, but you must pull the UniWebView submodule. The UniWebView submodule is a private repository used to open a webview for login on mobile. Please contact Emotiv to get access.
    - To pull all submodules (including UniWebView), use:
      ```
      git submodule update --init --recursive
      ```
    - UniWebView is a submodule inside the unity-plugin directory.


## How to Use

### 1. Open the Example Scene
Open the **SimpleExample.unity** scene. This scene demonstrates all major features and works on Desktop, Android, and iOS.

### 2. Set Credentials
In **AppConfig.cs**, set your `clientId` and `clientSecret` from your Emotiv Cortex App registration.

### 3. Platform-Specific Setup

#### Android Setup
- **Get the Embedded Library:** Contact Emotiv to obtain `EmotivCortexLib.aar` and place it in `./Assets/Plugins/Emotiv-Unity-Plugin/Src/AndroidPlugin/EmotivCortexLib/`.
- **Permissions:** Ensure your app requests Bluetooth and Location permissions at runtime. Unity 2021+ supports this via the `Android.Permission` API.
- **Update clientId:** Edit `mainTemplate.gradle` to set your `client_Id` for webview redirect URI.
- **Gradle Plugin:** The default `baseProjectTemplate.gradle` uses Gradle version 7.4.2, which is optimal for Unity 2021. If you are using a newer Unity version (such as Unity 2022 or later), you may need to update the Gradle plugin version to ensure compatibility with the Android build tools and SDKs required by your Unity version. On your first build in Android Studio, you might see a prompt to update the Gradle version—click **Yes** to proceed. Updating ensures your project builds successfully and takes advantage of the latest Android features and security updates.

#### iOS Setup
- **Get the Embedded Library:** Contact Emotiv to obtain `EmotivCortexLib.xcframework` and place it in `./Assets/Plugins/Emotiv-Unity-Plugin/Src/IosPlugin/EmotivCortexLib/`.
- **Build Settings:** In Unity, select iOS platform and click Build. Unity will generate an Xcode project (`.xcodeproj`).
- **Xcode Configuration:**
  - **Embed Framework:**
    - Open `.xcodeproj` in Xcode.
    - Go to Project > General > Frameworks, Libraries, and Embedded Content.
    - Add `EmotivCortexLib.xcframework` and set to Embed & Sign.
  - **Set Signing Team:**
    - Go to Signing & Capabilities.
    - Choose your Apple Developer Team before building.
  - **Bluetooth Permission (Info.plist):**
    - Add:
      ```xml
      <key>NSBluetoothAlwaysUsageDescription</key>
      <string>This app uses Bluetooth to discover, connect, and transfer data between devices.</string>
      ```
  - **Link Framework:**
    - Go to Build Phases > Link Binary with Libraries.
    - Add `EmotivCortexLib.xcframework` if not already listed.
- **Build and Run:** Build the app and run it on a physical iOS device.

- **Desktop:** Make sure EMOTIV Launcher is running and `USE_EMBEDDED_LIB` is **not** defined.

### 4. Run and Interact
1. Run the scene in the Unity Editor or on your target device.
2. Click **"Query Headset"** to list available headsets after authorization.
3. Enter a headset ID (or leave blank to use the first found) and click **"Create Session"**.
4. You can now:
   - **Subscribe/Unsubscribe Data Streams:** Select streams and click **"Subscribe Data"** or **"Unsubscribe Data"**.
   - **Start/Stop Recording:** Enter a title and click **"Start Record"**/**"Stop Record"**.
   - **Inject Marker:** Enter a label/value and click **"Inject Marker"** while recording.
> The UI and workflow are almost the same for Desktop, Android, and iOS.
> 
> **For Emotiv Embedded Library:**  
> - If you are not logged in with your EmotivID, the **Sign In** button will be active. Clicking **Sign In** opens a webview for authentication.  
> - After successful login, authorization is handled automatically.  
> - Once you see "Authorize done" in the message box on the UI, you can proceed to query headsets, create sessions, and use other features as described above.  
> - On mobile, ensure all required permissions are granted.

> **Please Note:**
>
> * **Headset Scanning (Cortex 3.7+):** Headset scanning is automatic after authorization. Manual `RefreshHeadset()` is not required in most cases.
> * **Interface:** The example uses **EmotivUnityItf.cs** for all Cortex API operations.
> * **Data Buffering:** To save data to a buffer for later access, set `IsDataBufferUsing = true` in `AppConfig.cs`.
> * **Mental Command/Facial Expression:** Always load a trained profile before subscribing to these streams, or only neutral actions will be received.
> * **Permissions:**
>   - **Android:** Request Bluetooth and Location permissions at runtime.
>   - **iOS:** Add Bluetooth permissions to `Info.plist` and ensure `.xcframework` is present in Xcode.
>   - **Desktop:** No special permissions, but EMOTIV Launcher must be running.

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
