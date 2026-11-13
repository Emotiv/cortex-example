# Emotiv ERP Setup: Emotiv Headset Configuration, Headset Clock Synchronization & Marker Workflow (`marker.py`)

This document provides a technical guide for setting up high-precision Event-Related Potential (**ERP**) experiments and injecting event markers (**Markers**) in Python using [marker.py](./marker.py) and [cortex.py](./cortex.py). It covers hardware and wireless optimization for **Flex2** and **EPOC X**, the end-to-end event-driven software workflow, headset clock synchronization (`syncWithHeadsetClock` / `headset_clock_adjustment`), and sub-millisecond marker timestamp calculation using `time.perf_counter()`.

---

## 1. Emotiv Headset & Connection Setup (Flex2 & EPOC X)

Before running an ERP experiment, proper physical setup of the headset and wireless link is essential to minimize packet loss, firmware buffer fluctuations, and timestamp drift:

- **Use an nRF Dongle:** Prefer using the nRF dongle (included with Flex2) rather than the PC's built-in Bluetooth. The nRF dongle provides significantly more stable wireless transmission than native PC Bluetooth adapters. For Mac computers equipped only with USB Type-C ports (no USB Type-A ports), use a short USB-C to USB-A adapter cable (approximately 10 cm in length) to keep the dongle away from chassis interference.
- **Minimum Bluetooth Requirement:** If an nRF dongle is not available, ensure the PC supports **Bluetooth 5.2 or higher**.
- **Optimize Flex2 Control Box Placement for Clear Line-of-Sight:** Position the **Flex2** control box so that there are **no physical obstructions—including the subject's own head—between the control box and the Bluetooth adapter / PC**. For example, if the subject is facing the PC screen and the control box is mounted at the back of the head, the subject's head acts as an obstacle between the box and the PC; in this scenario, mount the control box on **top of the head** instead (or, if mounted at the back of the head, ensure the back of the head / control box has a direct, unobstructed line of sight to the Bluetooth receiver).
- **Allow a 5–10 Minute Timestamp Stabilization Window:** After connecting the headset to the PC, the first **5 to 10 minutes** are critical for Cortex to calibrate and stabilize its internal timestamping algorithms. Keep the headset and PC in an unobstructed environment during this warm-up period to establish a robust timestamp baseline, which helps maintain accurate timing even if transient signal quality issues occur later during testing.
- **Reduce or Disable the Motion Sampling Rate to Prioritize EEG Data:** To further maximize wireless transmission performance in demanding environments and dedicate bandwidth entirely to EEG data, lower the Motion sampling rate to **64 Hz** or **32 Hz**, or turn it **Off** completely.

---

## 2. Software Workflow Overview

### 2.1. Text-Based Workflow Diagram (ASCII)

```text
[1. Initialize & Open Connection (c.open)]
   └── hasAccessRight -> requestAccess -> authorize -> queryHeadsets -> connect -> createSession
         │
         ▼ (emit: create_session_done)
[2. Synchronize with Headset Clock (syncWithHeadsetClock)]
   └── Send time.time() and time.perf_counter() to Cortex
         │
         ▼ (emit: sync_with_headset_clock_done)
   └── Store: self.headset_clock_adjustment = data.get('adjustment', 0)
         │
         ▼
[3. Start Data Recording (createRecord)]
         │
         ▼ (emit: create_record_done)
[4. Inject Markers in Worker Thread (injectMarker / updateMarker)]
   ├── Compute: marker_time = (time.perf_counter() + self.headset_clock_adjustment) * 1000
   ├── Call: inject_marker(marker_time, value, label, port='python_app')
   └── (Optional) Call: update_marker(marker_id, end_time) to close an Interval Marker
         │
         ▼ (emit: inject_marker_done -> when all markers are injected)
[5. Stop Data Recording (stopRecord)]
         │
         ▼ (Wait for Warning Code 30: warn_record_post_processing_done)
[6. Export Record (exportRecord) & Close Connection (c.close)]
```

---

## 3. Step-by-Step Software Workflow Breakdown

### Step 1: Initialization and Event Binding (`bind`)
In `Marker.__init__` ([marker.py](./marker.py)), the application initializes the `Cortex` client and binds callbacks for each stage of the workflow:
- `create_session_done`: Triggered when the session with the headset is established.
- `sync_with_headset_clock_done`: Triggered when the clock offset (`adjustment`) is returned by Cortex.
- `create_record_done`: Triggered when a new record starts capturing data.
- `inject_marker_done`: Triggered each time a marker is successfully injected into the active record.
- `stop_record_done`: Triggered when the active record is stopped.
- `warn_record_post_processing_done`: Triggered when Cortex finishes post-processing the stopped record (Warning code `30`), indicating the record is ready for export.
- `export_record_done`: Triggered when the record export finishes.

### Step 2: Headset Clock Synchronization (`syncWithHeadsetClock`)
As soon as `on_create_session_done` is invoked, the application calls `self.c.sync_with_headset_clock()` before starting a record. This aligns Python's high-resolution performance counter (`time.perf_counter()`) with Cortex's internal headset clock.

### Step 3: Record Creation (`createRecord`)
Once `headset_clock_adjustment` is received and stored in `on_sync_with_headset_clock_done`, the application calls `self.create_record(...)`.
> **Important:** Markers are always linked to an active record. You **must** start a record before injecting markers, and you cannot inject or update markers after the record has been stopped (`stopRecord`).

### Step 4: Marker Injection (`injectMarker` & `updateMarker`)
Cortex supports two types of markers:
1. **Instance Marker (Point-in-time marker):** Marks a single timestamp using `inject_marker`.
2. **Interval Marker (Time-range marker):** Marks a duration with a start time and an end time.
   - **Step 4a:** Call `inject_marker` at the beginning of the event $\rightarrow$ receive `marker_id` (`uuid`) in `on_inject_marker_done`.
   - **Step 4b:** When the event ends, call `update_marker(marker_id, end_time)` to set the end of the interval, converting the instance marker into an interval marker.

### Step 5: Stop Record and Wait for Post-Processing (Warning Code 30)
Once all markers have been injected, the application calls `self.stop_record()`.
- **Important:** Do not call `export_record` immediately inside `on_stop_record_done`. After a record stops, Cortex performs internal post-processing to finalize data streams and markers.
- When post-processing is complete, Cortex sends Warning code `30` (`RECORD_POST_PROCESSING_DONE`), which triggers `on_warn_record_post_processing_done`.

### Step 6: Export Record (`exportRecord`) and Close Connection
Inside `on_warn_record_post_processing_done`, the application calls `self.export_record(...)` to export the recorded data and markers to `CSV`, `EDF`, or `BDF`. Finally, `on_export_record_done` closes the WebSocket connection via `self.c.close()`.

---

## 4. `headset_clock_adjustment` & Marker Timestamp Calculation

### 4.1. Involved Functions and Timestamp Formula

#### How `headset_clock_adjustment` Is Obtained:
1. **`Cortex.sync_with_headset_clock(headset_id)`** (in [cortex.py](./cortex.py)):
   Captures both `time.time()` (in seconds) and `time.perf_counter()` (in seconds, high-resolution monotonic counter) right before sending the `"syncWithHeadsetClock"` JSON-RPC request to Cortex.
2. **`Cortex._handle_sync_with_headset_clock(result_dic)`** (in [cortex.py](./cortex.py)):
   Receives the response containing `"adjustment"` (in **seconds**) and emits the `'sync_with_headset_clock_done'` event.
3. **`Marker.on_sync_with_headset_clock_done(*args, **kwargs)`** (in [marker.py](./marker.py)):
   Stores `self.headset_clock_adjustment = data.get('adjustment', 0)`.

#### Marker Timestamp Calculation Formula:

```python
marker_time = (time.perf_counter() + self.headset_clock_adjustment) * 1000
```

| Component | Unit | Description |
| :--- | :---: | :--- |
| `time.perf_counter()` | Seconds (`float`) | High-resolution monotonic performance counter captured at the exact moment the event occurs (uses `QueryPerformanceCounter` on Windows with `100 ns` resolution, avoiding the `15.625 ms` tick quantization of `GetTickCount64`). |
| `self.headset_clock_adjustment` | Seconds (`float`) | Offset between your application's `time.perf_counter()` clock and the headset's monotonic clock in Cortex. |
| `time.perf_counter() + self.headset_clock_adjustment` | Seconds (`float`) | Event timestamp synchronized to Cortex's headset clock (in seconds). |
| `* 1000` | **Milliseconds (`float`)** | Converts seconds to **milliseconds (ms)**, as required by the `"time"` parameter in `injectMarker` and `updateMarker`. |

### 4.2. Three Mandatory Rules for `syncWithHeadsetClock`
1. **Call After Headset Connection:** Cortex initializes a dedicated clock for each headset when it connects via Bluetooth or USB dongle. You must call `syncWithHeadsetClock` **after** the headset is connected, never before.
2. **Provide a Valid `headset_id`:** Because each headset has its own clock, the target `headset` ID is required.
3. **Re-synchronize on Headset Reconnection:** If a headset disconnects and reconnects, Cortex re-initializes its clock, invalidating any previous `adjustment`. Always call `sync_with_headset_clock()` right before creating a record (`createRecord`), and use `time.perf_counter()` consistently for both synchronization and marker timestamp calculation.

---

## 5. Key API Calls Summary (`cortex.py` & `marker.py`)

For the complete working implementation, refer directly to [marker.py](./marker.py) and [cortex.py](./cortex.py). Below are the three core API calls involved in clock synchronization and marker injection:

```python
# 1. Synchronize with headset clock (called after createSession, before createRecord)
sync_request = {
    "jsonrpc": "2.0",
    "id": SYNC_WITH_HEADSET_CLOCK_ID,
    "method": "syncWithHeadsetClock",
    "params": {
        "headset": headset_id,
        "systemTime": time.time(),            # Current Unix epoch time (seconds)
        "monotonicTime": time.perf_counter()  # High-resolution monotonic time (seconds)
    }
}
# Response result provides: self.headset_clock_adjustment = result["adjustment"]

# 2. Compute synchronized marker timestamp (in milliseconds) at the moment of the event
marker_time = (time.perf_counter() + self.headset_clock_adjustment) * 1000

# 3. Inject an Instance Marker (or start an Interval Marker)
self.c.inject_marker_request(
    time=marker_time,
    value="1",
    label="stimulus_onset",
    port="python_app"
)
```

---

## 6. References
- [Emotiv Cortex API - Markers Overview](https://emotiv.gitbook.io/cortex-api/markers)
- [Emotiv Cortex API - syncWithHeadsetClock](https://emotiv.gitbook.io/cortex-api/headset/syncwithheadsetclock)
- [Emotiv Cortex API - injectMarker](https://emotiv.gitbook.io/cortex-api/markers/injectmarker)
- [Emotiv Cortex API - updateMarker](https://emotiv.gitbook.io/cortex-api/markers/updatemarker)
