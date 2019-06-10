# Nodejs cortex code example
This guide explain how to use Emotiv cortex api and explain node js example code.

## Installation
* [Install Nodejs](https://nodejs.org/en/)

* [Install EmotivApps](https://emotiv.com)

* Install ws package with npm ```npm install ws```

## Prepare
### Create emotiv id
Go to [emotiv website](https://emotiv.com) for create new emotivid

### License
Login to [emotiv website](https://emotiv.com) with above emotiv id then buy a license for coding third party app.

### Create client id and client secret
Go to [emotiv website](https://emotiv.com) to create a new pair of client id and client secret for your app.


## Cortex class explained
Cortex is class wrap around Emotiv cortex service api. In this class not yet contain all api function but essential functions for user could subcribe data stream from headset and doing mental command.

```async / await``` and ```Promise``` is used for function that need to run in sync mode.

### Check login and access request
Check user login status and access request are handled with function ```checkGrantAccessAndQuerySessionInfo```
Incase user not yet login or access request not yet granted, message will show up.

```javascript
/**
* - check if user logined
* - check if app is granted for access
* - query session info to prepare for sub and train
*/
async checkGrantAccessAndQuerySessionInfo(){
}
```

### Get session info
Session info is handled with function
```javascript
/**
* - query headset infor
* - connect to headset
* - authentication and get back auth token
* - create session and get back session id
*/
async querySessionInfo(){
}
```

### Subcribe data stream
Have 6 kind of data streams ('fac', 'pow', 'eeg', 'mot', 'met', 'com'). Subcribe data streams is handled by function ```sub```. Each time call user could sub one or many difference stream.
```javascript
/**
 * 
 * - check login and grant access
 * - subcribe for stream
 * - logout data stream to console or file
 */
 sub(streams){}
```

### Training profile
Training mental command for specific profile is handled with function ```train```.
Each time call function will train list of command, number of repeat train is free to specify but it is recommended to train atlease 5 times for each command.

```javascript
/**
* - check login and grant access
* - create profile if not yet exist
* - load profile
* - sub stream 'sys' for training
* - train for actions, each action in number of time
* 
*/
train(profileName, trainingActions, numberOfTrain){
}
```


### Live mode
After traing mental command successfully, user could test that command again in live mode with function ```live```.
```javascript
/**
 * 
 * - load profile which trained before
 * - sub 'com' stream (mental command)
 * - user think specific thing which used while training, for example 'push' action
 * - 'push' command should show up on mental command stream
 */
live(profileName) {
}
```

## How to use Cortex class

### Create cortex instance
To initialize a Cortex instance, you will need websocket url where cortex service running and information related to specific user : license, client id, client secret.

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

### Subscribe data stream
```javascript
// subcribe headset data stream
// user could sub one or many stream at once
// streams = ['fac', 'pow', 'eeg', 'mot', 'met', 'com']
let streams = ['eeg']
c.sub(streams)
```

### Training mental command and live mode
User need to train 'neutral' first. Then train for wanted action, for example 'push' action.
```javascript
// train is do with a specific profile
// if profile not yet exist, it will be created
let profileName = 'test'
// number of repeat train for each action
// user have 8 seconds for each time of training
let numberOfTrain = 5
// always train 'neutral' first then train other action
let trainingActions = ['neutral', 'push']
// c.train(profileName, trainingActions, numberOfTrain)
```

Finally test mental command in live mode with profile already trained
``` javascript
// load profile which already trained then test your mental command
c.live(profileName)
```

### Running steps

* Start CortexUI
* Login on CortexUI with emotivid and password manually
* Connect headset with pc or mac
* Wear headset and make sure have a good contact quality, contact quality could be viewed visually on CortexUI
* Run example first time to request access ```node cortex_code_example.js```
* Approve access on CortexUI manually (Action of approve need to do only onece)
* Rerun example to sub data or training