# Python Example

## Requirement
- This example works with Python >= 3.7
- Install websocket client via  `pip install websocket-client`

## Cortex library
- `cortex.py` - the wrapper lib around Emotiv cortex api.

## BCI
- `train.py` demo mental command training and live mode.
- `facial_expression.py` demo facial expression training and live.
- For more details https://emotiv.gitbook.io/cortex-api/bci

## BCI advance
- `train_advance.py` demo ability to get active action, brain map, and training threshold
- `live_advance.py` demo ability to get and set action sensitivity in live mode

## Susbcribe data
- `sub_data.py` demo data streaming from Cortex: EEG, motion, band power and Performance Metrics.
- For more details https://emotiv.gitbook.io/cortex-api/data-subscription

## Create record and export to file
- `record.py` demo create record and export data to csv or edf file.
- For more details https://emotiv.gitbook.io/cortex-api/records

## Inject marker while recording
- `marker.py` demo inject marker while do recording.
- For more details https://emotiv.gitbook.io/cortex-api/markers


