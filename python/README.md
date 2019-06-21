# Cortex: Python example

These examples show how to call the Cortex API from Python, using the websockets
library.

## Prerequisites

The example code uses features that are only available in Python >= 3.6

In addition, the example code only works with Cortex 2.0, it is not compatible
with older versions of Cortex.

## Websockets

The example uses the Python websockets library to connect to Cortex.  The
websockets library (not to be confused with the older websocket library).

The websockets library utilizes `await` and `async` which were introduced in
Python 3.6.  

## Before you start

We generally recommend using a virtual environment.

Once you've set up your environment (virtual or otherwise) install the
requirements with `pip`:
    pip install -r requirements.txt

To run the existing example you will need to do a few things.

1. You will need an EMOTIV headset.  You can purchase a headset in our [online
   store](https://www.emotiv.com/)
2. Next, [download and install](https://www.emotiv.com/developer/) the Cortex
   service.  Please note that currently, the Cortex service is only available
   for Windows and macOS.
3. We have updated our Terms of Use, Privacy Policy and EULA to comply with
   GDPR. Please login via the EMOTIV App to read and accept our latest policies
   in order to proceed using the following examples.
4. Next, to get a client id and a client secret, you must connect to your
   Emotiv account on
   [emotiv.com](https://www.emotiv.com/my-account/cortex-apps/) and create a
   Cortex app. If you don't have a EmotivID, you can [register
   here](https://id.emotivcloud.com/eoidc/account/registration/).
5. Then, if you have not already, you will need to login with your Emotiv id in
   the EMOTIV App.
6. Finally, the first time you run these examples, you also need to authorize
   them in the EMOTIV App.

This code is purely an example of how to work with Cortex.  We strongly
recommend adjusting the code to your purposes.

## Basics

With the prerequisites complete, you can 
create a credentials file with your client ID and secret, and then run the
example file
```
cat << EOF > cortex_creds
client_id 123456789abcdefghijklmnopqrstuvwxyzABCDE
client_secret 123456789abcdefghijklmnopqrstuvwxyzABCDExxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
EOF
python ./example.py
```

This will use your client_id and client_secret to connect to Cortex, spit out
some information about the API, users, and licenses.  From there it will check
authorization, and query the connected headset(s).  Finally it will create a
session and a record, subscribe to some streams, and collect a few frames of
data.

## Details

The ``example.py`` file shows one way to tie together several Cortex calls.

Inside of lib/cortex.py you can see how to set up the websockets library to talk
to Cortex, and how to send commands and receive data. 

The canonical documentation for Cortex is available
[here](https://emotiv.gitbook.io/cortex-api/). It includes full descriptions of
the various methods available.
