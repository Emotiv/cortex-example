# Cortex C++ Examples

These examples show how to call the Cortex API from C++, using the Qt framework.

These examples only work with Cortex 2.0, they are not compatible with older versions of Cortex.

## Prerequisites

You must [download and install](https://www.emotiv.com/developer/) the Cortex service. Please note that currently, the Cortex service is only available for Windows and macOS.

You must install the Qt framework. You can get it for free at [www.qt.io](https://www.qt.io).  
These examples were compiled and tested with Qt 5.12

On macOS, you also need to install Xcode, or the "Command Line Tools for Xcode".

We have updated our Terms of Use, Privacy Policy and EULA to comply with GDPR. Please login via the EMOTIV Launcher to read and accept our latest policies in order to proceed using the following examples.

## How to compile

The easiest way to compile these sources is to use the Qt Creator IDE. It is included in the Qt framework.

From Qt Creator, open the **cortex-v2-examples.pro** project file. Then you can compile and run the examples directly from the IDE.

On Windows, you may need to add the appropriate Qt bin folder to your PATH environment variable.  
For example, if you installed Qt 5.9.3 in C:\Qt and you use Visual Studio 2015, you should add "C:\Qt\5.9.3\msvc2015\bin" to your PATH.

## Configure and run

To run these examples successfully, you need to edit the file **cortexclient/Config.h**.

At the top of this file, you need to set the client id and client secret of your Cortex app.

To get a client id and a client secret, you must connect to your Emotiv account on [emotiv.com](https://www.emotiv.com/my-account/cortex-apps/) and create a Cortex app. If you don't have a EmotivID, you can [register here](https://id.emotivcloud.com/eoidc/account/registration/).

If you use an Epoc Flex headset, you need to edit its channel mapping in **cortexclient/Config.h**.

Before you run these examples, you need to login with your Emotiv id in the EMOTIV Launcher.

The first time you run these examples, you also need to authorize them in the EMOTIV Launcher.

## Code structure

There is a static library and a few executables. Each is located in its own folder.

### cortexclient

This library contains most of the source code.

The most important class is **CortexClient**, this is the one you should read first in order to understand these examples. It connects to the Cortex service using a web socket, and provides methods to call most of the Cortex APIs.

Then, you should look at **HeadsetFinder** and **SessionCreator**. These 2 classes contain the logic to list the headsets connected to your device, and then open a session with a headset.

The **DataStreamExample** class contains the logic to subscribe to any data stream of a headset.

The **Training** class contains the logic to train the facial expression or mental command.

The **ProfileInfo** class read a training profile and display information about the trained actions.

### motion

This example opens a session with the first Emotiv headset it can find, and displays its motion data stream for 30 seconds.

### facial-expression-training

This example opens a session with the first Emotiv headset it can find. It creates and loads a training profile named "cortex-v2-example". Then it asks you to train 3 facial expressions: neutral, surprise and frown.

If your training is successful, then you can test your skills with the **facial-expression** example.

### facial-expression

This example opens a session with the first Emotiv headset it can find. It loads a training profile named "cortex-v2-example". Then it displays the facial expressions data stream for 30 seconds.

### mental-command-training

This example opens a session with the first Emotiv headset it can find. It creates and loads a training profile named "cortex-v2-example". Then it asks you to train 3 mental commands: neutral, push and pull.

If your training is successful, then you can test your mental command skills with the **mental-command** example.

### mental-command

This example opens a session with the first Emotiv headset it can find. It loads a training profile named "cortex-v2-example". Then it displays the mental command data stream for 30 seconds.

### performance-metric

This example opens a session with the first Emotiv headset it can find, and displays its performance metric data stream for 30 seconds.

### eeg

**WARNING: running this example will debit a session from your Emotiv license.**

This example opens a session with the first Emotiv headset it can find, and displays its raw EEG data stream for 30 seconds.

You must purchase an appropriate license from Emotiv in order to subscribe to the EEG data stream.

If Cortex cannot find your license when you run this example, please try to edit **eeg/main.cpp** and manually set your license id.

### marker

**WARNING: running this example will debit a session from your Emotiv license.**

This example opens a session with the first Emotiv headset it can find, and inject a few markers in the session. The session ends after 30 seconds.

If Cortex cannot find your license when you run this example, please try to edit **marker/main.cpp** and manually set your license id.

## Signals and slots in Qt

If you are not familiar with Qt programing, you should probably read this article about [signals and slots](http://doc.qt.io/qt-5/signalsandslots.html) first.  
In a nutshell, signals and slots in Qt are an implementation of the [observer design pattern](https://en.wikipedia.org/wiki/Observer_pattern).
* The subjects emit signals.
* The observers implement slots.
* A signal can be connected to one or more slots.
* Slots and signals can be connected and disconnected at runtime.
