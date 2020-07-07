# Python Example

## Requirement
- This example works with Python >= 3.7
- Install websocket client via  `pip install websocket-client`

## Cortex Library
- [`cortex.py`](./cortex.py) - the wrapper lib around EMOTIV Cortex API.

## Susbcribe Data
- [`sub_data.py`](./sub_data.py) shows data streaming from Cortex: EEG, motion, band power and Performance Metrics.
- For more details https://emotiv.gitbook.io/cortex-api/data-subscription

## BCI
- [`train.py`](./train.py) shows Mental Command training and live mode.
- [`facial_expression.py`](./facial_expression.py) shows facial expression training and live mode.
- For more details https://emotiv.gitbook.io/cortex-api/bci

## Advanced BCI
- [`train_advance.py`](./train_advance.py) shows the ability to get active actions, brain map, and training threshold.
- [`live_advance.py`](./live_advance.py) shows the ability to get and set action sensitivity in live mode.
- For more details https://emotiv.gitbook.io/cortex-api/advanced-bci

## Create record and export to file
- [`record.py`](./record.py) shows how to create record and export data to CSV or EDF format.
- For more details https://emotiv.gitbook.io/cortex-api/records

## Inject marker while recording
- [`marker.py`](./marker.py) shows how to inject marker during a recording.
- For more details https://emotiv.gitbook.io/cortex-api/markers

Note : for async code example, please access folder [`async`](./async)


