# Emotiv Cortex Octave/MATLAB Examples

This folder contains a small MATLAB/GNU Octave client for the Emotiv Cortex API. The first example checks the connection, verifies that an Emotiv user is logged in, requests application access when needed, authorizes the application, and prints basic account and license information.

The examples are written for researchers and engineers who use MATLAB or GNU Octave for analysis. You do not need to understand the internal classes to run the example.

## What You Will Run

The local connection has three parts:

1. **EMOTIV Launcher**: Provides the local Cortex service and the logged-in Emotiv account.
2. **Python proxy**: Connects MATLAB/Octave to Cortex. It listens on `127.0.0.1:9999` and forwards messages to Cortex over WSS at `wss://localhost:6868`.
3. **MATLAB or GNU Octave**: Runs the example and communicates with the proxy.

The Python proxy is a required helper utility. It is not implemented by MATLAB or Octave.

## Folder Structure

- `api/`: Public functions (`cortexConnect`, `cortexSubscribe`, `cortexGetData`, `cortexDisconnect`) that hide the internal MATLAB classes.
- `internal/`: Internal SDK classes. Not a public interface; they may change.
- `proxy/`: The Python TCP-to-WSS proxy.
- `config/`: Local configuration. `config/config.json` holds your Cortex application credentials. This file is ignored by Git and must not be committed or shared.
- `examples/`: Beginner-friendly scripts that show complete workflows.

The public workflow uses `cortexConnect` and `cortexDisconnect`. The underlying `CortexManager` and `CortexAuth` classes are kept inside `internal/` so future examples can extend the SDK without making the user-facing scripts more complicated.

## Requirements

### EMOTIV software and account

- EMOTIV Launcher installed and running.
- An Emotiv account signed in to EMOTIV Launcher.
- A compatible EMOTIV headset, or a virtual headset configured in EMOTIV Launcher.
- A Cortex application with a Client ID and Client Secret.

Download EMOTIV Launcher from the [EMOTIV website](https://www.emotiv.com/products/emotiv-launcher). Follow the Launcher prompts to accept the current Terms of Use, Privacy Policy, and EULA.

### Python

- Python 3 is recommended.
- The proxy uses the `websocket-client` package.

```text
python -m pip install websocket-client
```

Check the installation with:

```text
python --version
python -c "import websocket; print('websocket-client is ready')"
```

### MATLAB or GNU Octave

Use a recent MATLAB release or GNU Octave version with:

- `jsonencode` and `jsondecode` support.
- Java networking support, including `java.net.Socket`.
- Permission to open a local TCP connection to `127.0.0.1:9999`.

The SDK does not require the MATLAB Instrument Control Toolbox, an Octave sockets package, or a separate JSON package.

## Register a Cortex Application

The Client ID and Client Secret identify your application. They are different from your Emotiv account password.

1. Sign in to your Emotiv account.
2. Open the Cortex application management page: [Emotiv Cortex Apps](https://www.emotiv.com/my-account/cortex-apps/).
3. Create a Cortex application, or open an existing application.
4. Copy its **Client ID** and **Client Secret**.
5. Keep the Client Secret private. Do not paste it into source code, commit it to Git, or send it in a public issue.

You can also follow the application-registration instructions in the repository's [Python examples README](../python/README.md) and the [official Cortex API documentation](https://emotiv.gitbook.io/cortex-api).

## Configure Your Credentials

The example reads credentials from `octave/config/config.json`.

1. Open `octave/config/config.json`.
2. Replace the placeholder values with the Client ID and Client Secret from your Cortex application:

```json
{
  "clientId": "YOUR_CLIENT_ID",
  "clientSecret": "YOUR_CLIENT_SECRET"
}
```

3. Save the file.

The script `examples/setCredentials.m` loads this file automatically. If the file is missing, it asks for the two values interactively and explains where to obtain them.

The repository ignores `config.json` because it contains a secret. If Git shows this file as tracked in your checkout, remove it from version control before committing changes; do not publish the secret.

## Quickstart

Run these steps in order.

### 1. Start EMOTIV Launcher

Open EMOTIV Launcher and sign in. Make sure Cortex is available before starting the proxy.

If you are testing without a physical headset, create or enable a virtual headset in EMOTIV Launcher according to the [virtual brainwear guide](https://emotiv.gitbook.io/emotiv-launcher/devices-setting-up-virtual-brainwear-r/creating-a-virtual-brainwear-device).

### 2. Install the Python dependency

Run this command in any terminal:

```text
python -m pip install websocket-client
```

### 3. Start the Cortex proxy

Open a terminal and run:

```text
cd octave/proxy
python cortex_proxy.py
```

Leave this terminal open. A successful startup reports that the proxy is listening on `127.0.0.1:9999`.

### 4. Configure `config.json`

Complete the credential setup described above before running the example.

### 5. Run the authentication example

Open MATLAB or GNU Octave from the repository root and run:

```matlab
run('octave/examples/testAuth.m')
```

The script adds `octave/internal`, `octave/api`, and `octave/examples` to the search path automatically. It then calls this simple workflow:

```matlab
cortex = cortexConnect(clientId, clientSecret);
% Use cortex.token, cortex.userInfo, and cortex.licenseInfo here.
cortex = cortexDisconnect(cortex);
```

A successful run prints an authorization message, the Cortex token length or token, the logged-in username, and the license name. The example closes the connection before it finishes.

### 6. Approve application access when prompted

The first authorization may require approval in EMOTIV Launcher. Keep the application open and approve access when prompted. The example waits for approval for up to 180 seconds.

## Understanding the Result

`cortexConnect` returns a plain MATLAB/Octave struct:

- `cortex.isConnected`: `true` after the proxy connection and authorization succeed.
- `cortex.token`: The Cortex token for later API requests.
- `cortex.userInfo`: Information returned by `getUserInformation`.
- `cortex.licenseInfo`: License information returned by `getLicenseInfo`.

The internal manager is stored in the struct only so that `cortexDisconnect(cortex)` can close the Java socket. Treat that field as internal and do not edit it.

## Subscribe and Plot

`examples/subscribeAndPlot.m` demonstrates subscribing to the `eeg` stream and plotting its electrode channels.

1. Open `examples/subscribeAndPlot.m` and set `headsetId` (shown in EMOTIV Launcher, for example `INSIGHT-A1B2C3D4`).
2. Run `run('octave/examples/subscribeAndPlot.m')`.

```matlab
cortex = cortexConnect(clientId, clientSecret);
cortex = cortexSubscribe(cortex, 'INSIGHT-A1B2C3D4', {'eeg'});
[labels, data, time] = cortexGetData(cortex, 'eeg', 256);  % newest 256 samples
cortex = cortexDisconnect(cortex);                         % unsubscribes and closes the socket
```

- To subscribe to other supported streams, pass their names instead, for example `{'mot','dev','eq','pow','met'}`. Call `cortexGetData(cortex, streamName)` for each stream you want to consume.
- `labels{j}` names column `j` of `data`; each row is a sample. Use the returned labels to select columns rather than hard-coding positions. For console output, loop over `labels` and print each `data(end,j)`; for plots, plot the desired numeric columns against `time`.
- `met` columns named `<metric>.isActive` are 1 when that metric is running. When it is 0 the metric value is `NaN` (Cortex sends `null`). Without a PRO license `met` arrives about every 10 seconds; with a PRO license about 2 samples per second. `pow` does not need a PRO license.
- `cortexSubscribe` scans for the headset, connects it if needed (waiting up to 5 seconds for Cortex warning 104), creates an active session and subscribes. Optional name/value pairs: `'autoRescan'` (default `true`), `'queryInterval'` (default `3` seconds), `'scanTimeout'` (default `15` seconds).
- `cortexGetData` collects the samples that have arrived and returns them from a buffer that keeps the latest 10000 samples per stream. `data` has one row per sample and one column per entry in `labels`; empty values are `NaN`.
- Call `cortexGetData` regularly. Samples are only read from the connection when it runs, and it can take up to about 1 second when no data is arriving.

## Troubleshooting

### Could not connect to the proxy on `localhost:9999`

- Confirm that the proxy terminal is still running.
- Start it from `octave/proxy` with `python cortex_proxy.py`.
- Check that another program is not already using port `9999`.
- Confirm that EMOTIV Launcher is running before the proxy starts.

### The proxy cannot connect to Cortex at `wss://localhost:6868`

- Open EMOTIV Launcher and sign in.
- Confirm that Cortex is available in the Launcher.
- Restart EMOTIV Launcher, then restart the proxy.
- Check that local security software is not blocking the connection.

### No user is logged in

Sign in to EMOTIV Launcher and run `testAuth.m` again.

### Application access is not granted

Approve the application-access request in EMOTIV Launcher. The example waits for up to 180 seconds. If the wait expires, run the example again after approving access.

### Invalid Client ID or Client Secret

Open the Cortex application page and copy the credentials again. Make sure `config.json` contains valid JSON and that the property names are exactly `clientId` and `clientSecret`.

### `jsondecode` or `jsonencode` is unavailable

Use a recent MATLAB or GNU Octave release with native JSON support. The SDK does not install or use a third-party JSON package.

### Headset not found or not connected

- Switch the headset on and make sure it is in range, or enable a virtual headset in EMOTIV Launcher.
- Check that `headsetId` matches the id shown in EMOTIV Launcher exactly.
- Increase `'scanTimeout'` in `cortexSubscribe` if the headset is slow to appear.
- If you see "warning 104 was not received", the headset did not finish connecting in 5 seconds; try again.

### A port conflict occurs

The current proxy and MATLAB/Octave client use port `9999`. Stop the process using that port before starting the proxy again. Changing the port requires updating both the proxy and the `CortexManager` defaults.

## Next Steps

Once authentication works, new examples can reuse the same connection pattern and build additional Cortex workflows in `api/` and `examples/`. Keep transport and authentication details in `internal/`, and keep researcher-facing scripts short and sequential.

For the full API reference, see the [official Emotiv Cortex API documentation](https://emotiv.gitbook.io/cortex-api).
