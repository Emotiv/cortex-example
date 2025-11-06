# Release Notes

## 30th October, 2025
- C#: Updated all C# projects to target .NET Framework 4.8 (CortexAccess, EEGLogger, BandPowerLogger, MotionLogger, PMLogger, FacialExpressionTraining, InjectMarker, RecordData, MentalCommandTraining).
- Updated App.config in each C# project to set `<supportedRuntime sku=".NETFramework,Version=v4.8" />` and adjusted bootstrapper metadata to .NET 4.8 where present.
- Documentation: `csharp/README.md` now lists .NET Framework 4.8 (or newer) as the requirement.
- Repo hygiene: Expanded top-level `.gitignore` to exclude common build artifacts and caches for Visual Studio/.NET, Unity, Node.js, Python, Qt/Qt Creator, and OS junk files.
- Compatibility note: No code-level breaking changes are expected with the move to .NET 4.8. Developer machines should have the .NET Framework 4.8 Developer Pack to build; target machines require the .NET Framework 4.8 runtime. Cortex service continues to require TLS 1.2+; ensure the Emotiv root CA is trusted locally if you encounter TLS trust issues on `wss://localhost:6868`.

## 30th November, 2022
- Add Emotiv's self-signed certificate rootCA.pem
- Update python example to use the Emotiv's self-signed certificate when open a secure Websocket connection to Emotiv Cortex Websocket
- Replace Newtonsoft.Json.12.0.1 csharp library by new version Newtonsoft.Json.13.0.1

## Before 30th November, 2022
- Introduce and update cpp-qt, python, csharp, unity, nodejs examples to show how to work with Emotiv Cortex.

