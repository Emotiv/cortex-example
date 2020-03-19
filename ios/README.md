
## Prerequisites
- iOS device (iOS version 11.0 or higher)
- EMOTIV App for iOS
- Emotiv headset (if you don't have, you can purchase one [here](https://www.emotiv.com))
- Emotiv account (if you don't have, you can register [here](https://www.emotiv.com))

## EMOTIV App for iOS
[What is EMOTIV App (for desktop)?](https://emotiv.gitbook.io/cortex-manual/)

EMOTIV App for iOS is required for any other apps which want to access EEG raw data and/or processed data from Emotiv headsets. You will need it to:
* log in Emotiv system
* manage headset connections
* manage 3rd app permissions
* It also provides a secure web socket connection (at wss://localhost:6868) for your app to read data from, make training action, etc. See more details in [Cortex API Documentation](https://emotiv.gitbook.io/cortex-api/).

### Installing
##### Unreleased version (only for Emotiv-partner developers)

- You need to install [Testflight](https://apps.apple.com/us/app/testflight/id899247664) to able download EMOTIV App to your device.
- You will receive an invitation email to a testing program for an unreleased version of the EMOTIV App.
    - *Note: this is only available for our partner developers, not for everyone.*
- You can click on the link provided in the email, follow the guide to install EMOTIV App on your iOS device.
##### Released version

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
    - *Note: API documentation is only for desktop version but should be almost the same on mobile. Exceptions will be noted below.*
- Open project by Xcode. You can see CortexV2Example is a set of modules, each module contains example code for a specific feature:

Module| Related Cortex APIs
----------------|----------------
cortexclient |[Connecting to the Cortex API](https://emotiv.gitbook.io/cortex-api/connecting-to-the-cortex-api)
authorize |[Authentication](https://emotiv.gitbook.io/cortex-api/authentication)
headset-session | [Headsets](https://emotiv.gitbook.io/cortex-api/headset) & [Sessions](https://emotiv.gitbook.io/cortex-api/session)
record-marker | [Record](https://emotiv.gitbook.io/cortex-api/records) & [Markers](https://emotiv.gitbook.io/cortex-api/markers)
eeg / motion / bandpower / facial-expression mental-command / performance-metrics | [Data Subscription](https://emotiv.gitbook.io/cortex-api/data-subscription)
facial-expression-training / mental-command-training | [BCI](https://emotiv.gitbook.io/cortex-api/bci) & [Advanced BCI](https://emotiv.gitbook.io/cortex-api/advanced-bci)

- Choose the module you want to build and run on iOS device.
- The module app has a list of buttons demonstrating an API call. Try click on each button, you can see a pair of request/response json strings printed out in console log

## Cortex API for iOS
Although [Cortex API documentation](https://emotiv.gitbook.io/cortex-api/) is only for desktop, you can base on that to develop on iOS since most of APIs are the same, except for these below APIs:

APIs | Desktop version | Mobile version
--------|--------|--------
[controlDevice](https://emotiv.gitbook.io/cortex-api/headset/controldevice)|available values for `command` parameter: "connect", "disconnect", "refresh".|available values for `command` parameter: "refresh". <br>*You can only connect or disconnect headsets via EMOTIV App*.
[queryHeadsets](https://emotiv.gitbook.io/cortex-api/headset/queryheadsets)|return all discovered headsets.|return one and only connected headset.
[exportRecords](https://emotiv.gitbook.io/cortex-api/records/exportrecord)|Supported.|Not supported.


## Cortex security certificates
When open EMOTIV App, it will require you download and install Cortex's certificate. Please follow the instruction in the app and install the certificate correctly.
If you ignore this step example code will not work with Cortex API.
