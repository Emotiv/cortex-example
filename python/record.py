from cortex import Cortex
import time

class Record():
    def __init__(self):
        self.c = Cortex(user, debug_mode=True)
        self.c.bind(create_session_done=self.on_create_session_done)
        self.c.bind(create_record_done=self.on_create_record_done)
        self.c.bind(stop_record_done=self.on_stop_record_done)
        self.c.bind(warn_cortex_close_session=self.on_warn_cortex_close_session)
        self.c.bind(export_record_done=self.on_export_record_done)

    def start(self, record_name, record_description, record_length_s, headsetId=''):
        """
        To start data recording and exporting process
        Parameters
        ----------
        record_name : string, required
             name of record
        record_description : string, optional
             description of record
        record_length_s: int, required
            duration of record

        headsetId: string , optional
             id of wanted headet which you want to work with it.
             If the headsetId is empty, the first headset in list will be set as wanted headset
        Returns
        -------
        None
        """
        self.record_name = record_name
        self.record_length_s = record_length_s
        self.record_description = record_description

        if headsetId != '':
            self.c.set_wanted_headset(headsetId)

        self.c.open()

    def wait(self, record_length_s):
        print('start recording -------------------------')
        length = 0
        while length < record_length_s:
            print('recording at {0} s'.format(length))
            time.sleep(1)
            length+=1
        print('end recording -------------------------')

    # callbacks functions
    def on_create_session_done(self, *args, **kwargs):
        print('on_create_session_done')

        # create a record 
        self.c.create_record(self.record_name, self.record_description)

    def on_create_record_done(self, *args, **kwargs):
        
        data = kwargs.get('data')
        self.record_id = data['uuid']
        start_time = data['startDatetime']
        title = data['title']
        print('on_create_record_done: recordId: {0}, title: {1}, startTime: {2}'.format(self.record_id, title, start_time))

        # record duration is record_length_s
        self.wait(self.record_length_s)

        # stop record
        self.c.stop_record()

    def on_stop_record_done(self, *args, **kwargs):
        
        data = kwargs.get('data')
        record_id = data['uuid']
        start_time = data['startDatetime']
        end_time = data['endDatetime']
        title = data['title']
        print('on_stop_record_done: recordId: {0}, title: {1}, startTime: {2}, endTime: {3}'.format(record_id, title, start_time, end_time))

        # disconnect headset to export record
        self.c.disconnect_headset()

    def on_warn_cortex_close_session(self, *args, **kwargs):

        # cortex has closed session. Wait some seconds before exporting record
        time.sleep(3)

        # record_export_folder = 'your place to export, you should have write permission, example on desktop'
        record_export_folder = 'G:/Emotiv'
        record_export_data_types = ['EEG', 'MOTION', 'PM', 'BP']
        record_export_format = 'CSV'
        record_export_version = 'V2'

        self.c.export_record(record_export_folder,
                             record_export_data_types,
                             record_export_format,
                             record_export_version,
                             [self.record_id])

    def on_export_record_done(self, *args, **kwargs):
        print('on_export_record_done')
        data = kwargs.get('data')
        print(data)

# -----------------------------------------------------------
# 
# SETTING
#   - replace  client_id, client_secret to user dic
#   - specify infor for record and export
#   - connect your headset with dongle or bluetooth, you should saw headset on Emotiv Launcher
#
# RESULT
#   - export result should be csv or edf file at location you specified
#   - in that file will has data you specified like : eeg, motion, performance metric and band power
# 
# -----------------------------------------------------------

"""
    client_id, client_secret: required params
        - To get a client id and a client secret, you must connect to your Emotiv account on emotiv.com and create a Cortex app
        - If your application require EEG access , you might register API access at https://www.emotiv.com/cortex-sdk-application-form
    license: optional param
        -In the case you borrow license from others, you need to add the license to the user dictionary such as "license" = "xxx-yyy-zzz"
"""
user = {
    "client_id" : "put application clientId",
    "client_secret" : "put application clientSecret"
}


r = Record()

# record parameters
record_name = 'your record name'
record_description = 'your description for record'
record_length_s = 30

# (1) check access right -> authorize -> connect headset->create session
# (2) start record --> stop record --> disconnect headset --> export record
r.start(record_name, record_description, record_length_s)
# -----------------------------------------------------------
