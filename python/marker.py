from cortex import Cortex
import time

class Marker():
    def __init__(self):
        self.c = Cortex(user, debug_mode=True)
        self.c.bind(create_session_done=self.on_create_session_done)
        self.c.bind(create_record_done=self.on_create_record_done)
        self.c.bind(stop_record_done=self.on_stop_record_done)
        self.c.bind(warn_cortex_close_session=self.on_warn_cortex_close_session)
        self.c.bind(inject_marker_done=self.on_inject_marker_done)
        self.c.bind(export_record_done=self.on_export_record_done)

    def start(self, record_name, record_description, number_markers, headsetId=''):
        """
        To start data recording and inject marker process
        Parameters
        ----------
        record_name : string, required
             name of record
        record_description : string, optional
             description of record
        number_markers: int, required
            number of markers

        headsetId: string , optional
             id of wanted headet which you want to work with it.
             If the headsetId is empty, the first headset in list will be set as wanted headset
        Returns
        -------
        None
        """
        self.record_name = record_name
        self.record_description = record_description
        self.number_markers = number_markers
        self.marker_idx = 0

        if headsetId != '':
            self.c.set_wanted_headset(headsetId)

        self.c.open()

    def add_markers(self, number_markers):
        print('add_markers')
        for m in range(number_markers):
            marker_time = time.time()*1000
            print('add marker at : ', marker_time)
            
            marker = {
                "label":str(m),
                "value":"test_marker",
                "port":"python-app",
                "time":marker_time
            }

            self.c.inject_marker_request(marker)

            # add marker each seconds
            time.sleep(3)

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

        # inject markers
        self.add_markers(self.number_markers)

    def on_stop_record_done(self, *args, **kwargs):
        
        data = kwargs.get('data')
        record_id = data['uuid']
        start_time = data['startDatetime']
        end_time = data['endDatetime']
        title = data['title']
        print('on_stop_record_done: recordId: {0}, title: {1}, startTime: {2}, endTime: {3}'.format(record_id, title, start_time, end_time))

        # disconnect headset to export record
        self.c.disconnect_headset()

    def on_inject_marker_done(self, *args, **kwargs):
        
        data = kwargs.get('data')
        marker_id = data['uuid']
        start_time = data['startDatetime']
        marker_type = data['type']
        print('on_inject_marker_done: markerId: {0}, type: {1}, startTime: {2}'.format(marker_id, marker_type, start_time))
        
        self.marker_idx = self.marker_idx + 1
        if self.marker_idx == self.number_markers:
            # stop record
            self.c.stop_record()

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
#   - replace client_id, client_secret to user dic
#   - specify infor for record and inject marker
#   - connect your headset with dongle or bluetooth, you should saw headset on Emotiv Launcher
#
# RESULT
#   - this demo add marker each 3 seconds
#   - export data file should contain marker added
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

m = Marker()

# record parameters
record_name = 'your record name'
record_description = 'your description for record'
marker_numbers = 10

# (1) check access right -> authorize -> connect headset->create session
# (2) start record --> inject marker --> stop record --> disconnect headset --> export record
m.start(record_name, record_description, marker_numbers)

# ----------------------------------------------