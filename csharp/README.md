# Cortex C# Examples
These examples show how to call the Cortex APIs from C# which describe at [Cortex Document](https://app.gitbook.com/@emotiv/s/cortex-api/)

## Getting Started
These instructions will get you a copy of the project up and running on your local machine for development.
### Prerequisites
* You might have [Visual Studio](https://www.visualstudio.com/) with C# supported (msvc14 or higher recommended).
* The Cortex have to be running on your machine such as service. You can get the Cortex from (https://www.emotiv.com/developer/).
* Register a Application at https://www.emotiv.com/my-account/cortex-apps/ and get a pair of client id and client secret. You must connect to your Emotiv account on emotiv.com and create a Cortex app. If you don't have a EmotivID, you can [register here](https://id.emotivcloud.com/eoidc/account/registration/).
* We have updated our Terms of Use, Privacy Policy and EULA to comply with GDPR. Please login via EMOTIV Launcher to read and accept our latest policies in order to proceed using the following examples.  

### How to compile
<!-- how to compile  -->
1. Open CortexExamples.sln by Visual Studio IDE
2. Use Nuget Package Manager to install _Newtonsoft.Json, SuperSocket.ClientEngine.Core, WebSocket4Net_ for CortexAccess project
3. Login via EMOTIV Launcher and put  your client id and client secret to Config.cs. More detail please see below.
4. You can compile and run the examples directly from the IDE.

### Code structure
<!-- Code structure :overview about projects, classes in CortexAccess project and other examples-->
This section describe structure overview, core classes and examples. The C# Cortex examples contain 2 parts. Firstly, CortexAcess project is core and responsible for processing all requests and responses such as a middle layer between user and cortex. Secondly, Example projects, each example as a project which reference to CortexAccess project.
<!-- Structure overview -->
#### Structure overview
* CortexClient: Responsible for sending requests to Cortex and handle responses, warning, data from Cortex.
* Config: Contain configurations. User must fill clientId, client Secret of App. To get EEG and Performance metric data, an appropriate license is required.
* Authorizer: Responsible for getUserLogin, requestAccess, authorize for App.
* HeadsetFinder: Reponsible for finding headsets, connect headset.
* SessionCreator: Responsible for createSession, updateSession for work-flow.
* Training: Responsible for create/load/unload/ query Profiles and training.
* Examples: We have examples to demo subscribe data, record, inject marker, training.

#### Examples
**1. EEGLogger**
* This example opens a session with the first Emotiv headset. Then subscribe and save eeg data to EEGLogger.csv file until Esc key pressed. 
* The basic work-flow: Login via EMOTIV Launcher -> requestAccess-> Authorize (an appropriate license is required) -> find and connect headset -> Create Session -> Subscribe EEG data.
* Notes: 
  - 1) Need put an appropriate license in Program.cs
  - 2) Press Esc to flush data to output file and exit.

**2. MotionLogger**
* This example opens a session with the first Emotiv headset. Then subscribe and save motion data to MotionLogger.csv file until Esc key pressed.
* The basic work-flow: Login via EMOTIV Launcher -> requestAccess-> Authorize() -> find and connect headset -> Create Session -> Subscribe Motion data.
* Notes: 
  - 1) Press Esc to flush data to output file and exit.

**3. BandPowerLogger**
* This example opens a session with the first Emotiv headset. Then subscribe and save motion data to BandPowerLogger.csv file until Esc key pressed.
* The basic work-flow: Login via EMOTIV Launcher -> requestAccess-> Authorize() -> find and connect headset -> Create Session -> Subscribe Band Power data.
* Notes: 
  - 1) Press Esc to flush data to output file and exit.

**4. MentalCommandTraining**
* This example opens a session with the first Emotiv headset. Then User can create/load/unload profile then train actions following console guide.
* The example demo for train: neutral, push, pull actions but you can add more actions as getDetectionInfo Output.
* The basic work-flow: Login -> Login via EMOTIV Launcher -> requestAccess-> Authorize() -> find and connect headset -> Create Session -> Subscribe _sys_ data -> Load/Create profile -> Start training actions -> Accept/Reject training.

**5. FacialExpressionTraining**
* This example opens a session with the first Emotiv headset. Then User can create/load/unload profile then train actions following guideline on console.
* The example demo for train: neutral, smile, frown, clench actions but you can add more actions as getDetectionInfo Output.
* The basic work-flow: Login -> Login via EMOTIV Launcher -> requestAccess-> Authorize() -> find and connect headset -> Create Session -> Subscribe _sys_ data -> Load/Create profile -> Start training actions -> Accept/Reject training.

**6. InjectMarkers**
* This example opens a session with the first Emotiv headset. Then createRecord and inject markers to data stream.
* Following guideline shown on console. Press a certain key to set a label of marker into injectmarker function. The program ignores Tab, Enter, Spacebar and Backspace Key.
* The basic work-flow: Login via EMOTIV Launcher -> requestAccess-> Authorize() -> find and connect headset -> Create Session -> CreateRecord -> InjectMarker.

**7. RecordData**
* This example opens a session with the first Emotiv headset. Then create/stop/update/delete record.
* The basic work-flow: Login via EMOTIV Launcher -> requestAccess-> Authorize() -> find and connect headset -> Create Session -> CreateRecord -> Stop/Update/Query/Delete Record.

**8. PMLogger**
* This example opens a session with the first Emotiv headset. Then subscribe and save pm data to PMLogger.csv file until Esc key pressed. 
* The basic work-flow: Login via EMOTIV Launcher -> requestAccess-> Authorize (an appropriate license is required) -> find and connect headset -> Create Session -> Subscribe PM data.
* The performance metric data frequency depend on scope of license. For low performance metric : 1 sample/ 10 seconds ; for high performance metric 2 samples/ seconds.
* Notes: 
  - 1) Need put an appropriate license in Program.cs
  - 2) Press Esc to flush data to output file and exit.
  - 3) Each performance metric is a decimal number between 0 and 1. Zero means "low power", 1 means "high power". If the detection cannot run because of a bad contact quality then the value can also be **null**
### Notes
* You must login and logout via EMOTIV Launcher.
* You must use EMOTIV Launcher to grant AccessRight for the App one time for one emotiv user.
* You need a valid license to subscribe EEG data, Performance metrics data.
* The Examples are only demo for some Apis not all apis and to be continue updating.


* TODO: Basic UI for examples.

### References
1. https://www.newtonsoft.com/json
2. http://www.supersocket.net/
3. http://websocket4net.codeplex.com/
4. https://emotiv.github.io/cortex-docs/
5. C# Coding Standards for .NET - Lance Hunt
