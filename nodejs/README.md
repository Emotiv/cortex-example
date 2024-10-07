# NodeJS Cortex code example
This guide explains how to use Emotiv Cortex API and NodeJS example code.

## Installation
* [Install NodeJS](https://nodejs.org/en/)

* [Install Emotiv Launcher](https://emotiv.com)

* Install ws package with npm ```npm install ws```

## Prepare
### Create EmotivID
Go to [Emotiv website](https://emotiv.com) to create new EmotivID.

### Purchase license
Purchase the Developer license from [Emotiv website](https://emotiv.com).

### Create client id and client secret
Login [Emotiv website](https://emotiv.com) under your account to create a new pair of client id and client secret for your app.


## Cortex class explained
The Cortex class is wrapped around Emotiv Cortex service API. The wrapper class does not contain all the API mapping but essential features for user to subcribe data stream from headset and basic BCI actions.

```async / await``` and ```Promise``` are used for functions that need to be run in sync mode.

### Login and access request
User login status and access request are handled with function ```checkGrantAccessAndQuerySessionInfo```.

```javascript
/**
* - check if user is logged in
* - check if app is granted for access
* - query session info to prepare for data subscription and BCI actions
*/
async checkGrantAccessAndQuerySessionInfo(){
}
```

### Session info
Session info is handled with function
```javascript
/**
* - query headset info
* - connect to headset
* - authentication and retrieve auth token
* - create session and retrieve session id
*/
async querySessionInfo(){
}
```

### Subcribe to data streams
There are 6 kind of data streams ('fac', 'pow', 'eeg', 'mot', 'met', 'com'). Subcription of data streams is handled by function ```sub```. User could subscribe one or many difference streams at the same time.
```javascript
/**
 * - check login and grant access
 * - subcribe for stream(s)
 * - log data stream to console or file
 */
 sub(streams){}
```

### Mental Command Training mode
Mental Command training is handled with function ```train```.
A single or list of actions can be specified, and it is recommended to train at leaset 5 times for each action to get enough data for accurate live detection.

```javascript
/**
* - check login and grant access
* - create profile if not yet existed
* - load profile
* - sub stream 'sys' for training
* - train for actions and the number of times that will be trained
*/
train(profileName, trainingActions, numberOfTrain){
}
```


### Mental Command Live mode
After training the Mental Command action successfully, user could test that command again in live mode with function ```live```.
```javascript
/**
 * - load profile which have been trained before
 * - sub 'com' stream (Mental Command)
 * - user performs the same action which used while training, for example 'push' action
 * - 'push' command should show up on Mental Command stream
 */
live(profileName) {
}
```

## How to use Cortex class

### Create cortex instance
To initialize a Cortex instance, you will need to connect to the Cortex service and provide specific parameters like license, client id, client secret.

```javascript
// use local cortex service
// put your license, client id, client secret to user object
let socketUrl = 'wss://localhost:6868'
let user = {
    "license":"your license in prepare step above",
    "clientId":"your client id in prepare step above",
    "clientSecret":"your client secret in prepare step above",
    "debit":5000
}

let c = new Cortex(user, socketUrl)
```

### Subscribe data stream(s)
```javascript
// subcribe headset data stream
// user could subcribe one or many streams at once
// streams = ['fac', 'pow', 'eeg', 'mot', 'met', 'com']
let streams = ['eeg']
c.sub(streams)
```

### Training Mental Command and live mode
User needs to train 'neutral' first before any other actions.
```javascript
// training data is saved with a specific profile
// if profile not yet exist, it will be created
let profileName = 'test'
// number of training for each action
// user have 8 seconds for each time of training
let numberOfTrain = 5
// always train 'neutral' first then train other action
let trainingActions = ['neutral', 'push']
// c.train(profileName, trainingActions, numberOfTrain)
```

Finally test Mental Command in live mode with profile that has been trained
``` javascript
// load the profile which has already been trained to test the Mental Command
c.live(profileName)
```

### Running steps

* Start Launcher
* Login on with EmotivID and crediential
* Connect headset with the computer
* Wear headset and make sure to get a good contact quality
* Run example first time to request access ```node cortex_code_example.js```
* Approve access on Launcher manually (which only needs to be done once)
* Re-run example to subcribe data streams or perform training
