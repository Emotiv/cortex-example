
# Emotiv Cortex C# Example Suite

This repository provides C# console applications that demonstrate how to use the [Emotiv Cortex API](https://emotiv.gitbook.io/cortex-api) to connect to Emotiv headsets, stream data, manage records, and perform training tasks.

## 1. Setup & Prerequisites

### Requirements
- **Visual Studio 2019 or newer** (Community/Professional/Enterprise)
- **.NET Framework 4.7.2 or newer**
- **C# compiler**: Use the default provided by Visual Studio
- **NuGet Packages** (install via NuGet Package Manager):
  - `Newtonsoft.Json`
  - `SuperSocket.ClientEngine.Core`
  - `WebSocket4Net`
- **EMOTIV Launcher** (required for login and granting application access). Installing EMOTIV Launcher will also run the Emotiv Cortex Service locally. [Download here](https://www.emotiv.com/products/emotiv-launcher)


### Setup Steps
1. Open `CortexExamples.sln` in Visual Studio
2. Use NuGet Package Manager to install required packages for the `CortexAccess` project
3. Enter your client ID and client secret in `Config.cs`
4. Build and run any example project from Visual Studio

## 2. Core Classes in CortexAccess

### Main Classes
- **CtxClient.cs**: Handles connection to the Emotiv Cortex WebSocket service, builds requests, sends them, and processes responses/events.
- **DataStreamExample.cs**: The easiest entry point for subscribing to data streams. This class manages the entire workflow: opening the WebSocket, authorization, session creation, and data subscription. Simply call `Start()` and it will sequence all required steps. You can customize this class for advanced workflows (e.g., create a record before subscribing, load a training profile before subscribing to 'com' data, or connect to a specific headset by setting `_wantedHeadsetId`).
- **RecordManager.cs**: Manages record-related tasks: create, inject marker, export, and delete records.
- **Training.cs**: Handles profile creation/loading and training for a specific headset (`_wantedHeadsetId`). But does not subscribe to 'com' or 'fac' data streams for mental command power.
- **Other utility classes**: Helpers for configuration, session management, headset finding, authorization, etc.

## 3. How to Use the Examples

All examples are console applications. Each example demonstrates a specific Cortex API workflow:

- **XYLogger (EEGLogger, BandPowerLogger, MotionLogger, PMLogger)**: Demonstrate how to subscribe to EEG, Band Power, Motion, or Performance Metrics data streams. Data is saved to a `.csv` file. Press `Esc` to stop and save.
- **RecordData**: Interactive demo for creating, stopping, querying, updating, deleting, and exporting records. Commands are listed in the console.
- **InjectMarker**: Shows how to create a record and inject markers. Follow the console instructions.
- **FacialExpressionTraining & MentalCommandTraining**: Demonstrate profile creation/loading and training. Follow console prompts for actions and keys.

## 4. Notes for Beginners

- You must log in via EMOTIV Launcher and grant AccessRight for your app (one time per user).
- A valid license is required for EEG and Performance Metrics data.
- By default, the examples use the first headset found. To use a different headset, set `_wantedHeadsetId` in the relevant class.
- From Cortex 3.7+, call `ScanHeadsets()` in `HeadsetFinder.cs` to start EMOTIV headset scanning.
- These examples are intended as starting points. You can extend or modify them for your own research or product needs.

## 5. References

1. https://www.newtonsoft.com/json
2. http://www.supersocket.net/
3. http://websocket4net.codeplex.com/
4. https://emotiv.github.io/cortex-docs/
5. C# Coding Standards for .NET - Lance Hunt
