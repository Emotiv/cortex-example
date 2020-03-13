
## Prequisitions
- Android device (Android version >= 7)
- EMOTIV App for Android
- Emotiv headset (if you don't have, you should buy one [here](https://www.emotiv.com)) (question: any headsets can work???)
- Emotiv account (if you don't have, you can register [here](https://www.emotiv.com))

## Install EMOTIV App for Android device
[What is EMOTIV App (for desktop)?](https://emotiv.gitbook.io/cortex-manual/)

EMOTIV App for Android is required for any other apps which want to access EEG raw data and/or processed data from Emotiv headsets. You will need it to:
* log in Emotiv system
* manage headset connections
* manage 3rd app permissions
* It also provides a secure web socket connection (at wss://localhost:6868) for your app to read data from or make training action, etc. See more details in [Cortex API Documentation](https://emotiv.gitbook.io/cortex-api/).

### Unreleased version (only for Emotiv-partner developers)

- You will receive an invitation email to a testing program for an unreleased version of the EMOTIV App.
*Note: this is only available for our partner developers, not for everyone.*
- You can click on the link provided in the email, follow the guide to install EMOTIV App on your Android device.
	- You cannot install if you have any version of EMOTIV App installed on your device, so make sure you uninstall it (for all users) before installing a new version.
	- If you send the link to others, they may not access to our unreleased version as long as they are not in our list.

### Released version

Not available yet.

## Try EMOTIV App
- Open EMOTIV App, make sure you accept all permission request pop ups.
- Log in using your Emotiv account. You are on the page of "Available Devices".
- Turn on your Emotiv headset, wait for the headset to show up.
		- *Note: If you don't see the headset after 30s, you can try to turn it off then on again.*
- After the headset is shown up on the list, click to connect.
- You can navigate to other pages using the top left menu icon.
- If you see anything inconvenience, you can find `Send Feedback` in the menu.

## Try Cortex V2 Example
- Cortex V2 Example demonstrates how to work with Cortex API. To get started, you may want to look at the API documentation: https://emotiv.gitbook.io/cortex-api/.
*Note: API documentation is only for desktop version but should be almost the same on mobile. Exceptions will be noted below.*
- Open project under Android/CortexV2Example by Android Studio. You can see CortexV2Example is a set of modules, each module contains example code for a specific feature:
		- cortexclient: (add description)
		- authorize: (add description)
		- record-marker: (add description)
		- headset-session:
		- eeg
		- motion
		- bandpower: (add description)
		- facial-expression
		- facial-expression-training
		- mental-command
		- performance-metrics
- Choose the module you want to build and run on Android device. Make sure you accept all permission requests.
- The module app has a list of buttons demonstrating an API call. Try click on each button, you can see a pair of request/response json strings printed out in Android Studio `logcat`.

## Cortex API for Android
Although [Cortex API documentation](https://emotiv.gitbook.io/cortex-api/) is only for desktop, you can base on that to develop on Android since most of APIs are the same, except for these below APIs:
(coming soon: a table of APIs with some explanation)

## (Anything else)