
# Emotiv Cortex API Python Examples

This repository provides a set of Python examples to help you get started with the [Emotiv Cortex API](https://emotiv.gitbook.io/cortex-api). Each script demonstrates a specific workflow, making it easier to understand and integrate Cortex API features into your own projects.


## Requirements

- Python 2.7+ or Python 3.4+
- Install dependencies:
  - `pip install websocket-client`
  - `pip install python-dispatch`


## Getting Started

Before running the examples, please ensure you have completed the following steps:

1. **Download and Install EMOTIV Launcher**: Download from [here](https://www.emotiv.com/products/emotiv-launcher). Log in with your Emotiv ID and accept the latest Terms of Use, Privacy Policy, and EULA in the Launcher.
2. **Accept Policies**: If prompted, accept any additional policies in the EMOTIV Launcher.
3. **Obtain an EMOTIV Headset or Create a Virtual Device**:
   - Purchase a headset from the [EMOTIV online store](https://www.emotiv.com/), **or**
   - Use a virtual headset in the EMOTIV Launcher by following [these instructions](https://emotiv.gitbook.io/emotiv-launcher/devices-setting-up-virtual-brainwear-r/creating-a-virtual-brainwear-device).
4. **Get Client ID & Secret**: Log in to your Emotiv account at [emotiv.com](https://www.emotiv.com/my-account/cortex-apps/) and create a Cortex app. [Register here](https://id.emotivcloud.com/eoidc/account/registration/) if you don't have an account. 
5. **Authorize Examples**: The first time you run these examples, you may need to grant permission for your application to work with Emotiv Cortex.

---

## Example Scripts Overview

### 1. `cortex.py` — Cortex API Wrapper
Central wrapper class for the Cortex API. Handles:
- Opening and managing the websocket connection
- Buidling JSON-RPC requests
- Handling responses, errors, and emitting events to corresponding classes
- Parsing and dispatching data to workflow scripts

### 2. `sub_data.py` — Subscribe to Data Streams
Demonstrates how to:
- Subscribe to data streams (EEG, motion, performance metrics, etc.)
- Print or process incoming data
See: [Data Subscription](https://emotiv.gitbook.io/cortex-api/data-subscription)

### 3. `record.py` — Record and Export Data
Demonstrates how to:
- Create a new record
- Stop a record
- Export recorded data to CSV or EDF
See: [Records](https://emotiv.gitbook.io/cortex-api/records)

### 4. `marker.py` — Inject Markers
Demonstrates how to:
- Inject markers into a record during data collection
- Export records with marker information
See: [Markers](https://emotiv.gitbook.io/cortex-api/markers)


### 5. `mental_command_train.py` — Mental Command Training
Demonstrates how to:
- Load or create a training profile
- Train mental command actions (e.g., neutral, push, pull)
See: [BCI](https://emotiv.gitbook.io/cortex-api/bci)


### 6. `facial_expression_train.py` — Facial Expression Training
Demonstrates how to:
- Load or create a training profile
- Train facial expression actions (e.g., neutral, surprise, smile)
See: [BCI](https://emotiv.gitbook.io/cortex-api/bci)

### 7. `live_advance.py` — Advanced Live Data & Sensitivity
Demonstrates how to:
- Load a trained profile
- Subscribe to the 'com' stream for live mental command data
- (Optionally) Subscribe to the 'fac' stream for live facial expression data
- Get and set sensitivity for mental command actions in live mode
See: [Advanced BCI](https://emotiv.gitbook.io/cortex-api/advanced-bci)

---

## Tips
- Each script is self-contained and demonstrates a specific workflow.
- Adjust the code as needed for your own applications.
- For more details, refer to the [official Cortex API documentation](https://emotiv.gitbook.io/cortex-api/).


