from cortex import Cortex
import time
import threading

class Marker():
    def __init__(self, app_client_id, app_client_secret, **kwargs):
        self.c = Cortex(app_client_id, app_client_secret, debug_mode=True, **kwargs)
        self.c.bind(create_session_done=self.on_create_session_done)
        self.c.bind(create_record_done=self.on_create_record_done)
        self.c.bind(stop_record_done=self.on_stop_record_done)
        self.c.bind(warn_cortex_stop_all_sub=self.on_warn_cortex_stop_all_sub)
        self.c.bind(inject_marker_done=self.on_inject_marker_done)
        self.c.bind(export_record_done=self.on_export_record_done)
        self.c.bind(inform_error=self.on_inform_error)

    def start(self, number_markers=10, headsetId=''):
        """
        To start data recording and inject marker process as below workflow
        (1) check access right -> authorize -> connect headset->create session
        (2) start record --> inject marker --> stop record --> disconnect headset --> export record
        Parameters
        ----------
        number_markers: int, required
            number of markers

        headsetId: string , optional
             id of wanted headet which you want to work with it.
             If the headsetId is empty, the first headset in list will be set as wanted headset
        Returns
        -------
        None
        """
        self.number_markers = number_markers
        self.marker_idx = 0

        if headsetId != '':
            self.c.set_wanted_headset(headsetId)

        self.c.open()

    def create_record(self, record_title, **kwargs):
        """
        To create a record
        Parameters
        ----------
        record_title : string, required
             title  of record
        other optional params: Please reference to https://emotiv.gitbook.io/cortex-api/records/createrecord
        Returns
        -------
        None
        """
        self.c.create_record(record_title, **kwargs)

    def stop_record(self):
        self.c.stop_record()


    def export_record(self, folder, stream_types, export_format, record_ids,
                      version, **kwargs):
        """
        To export records
        Parameters
        ----------
        More detail at https://emotiv.gitbook.io/cortex-api/records/exportrecord
        Returns
        -------
        None
        """
        self.c.export_record(folder, stream_types, export_format, record_ids, version, **kwargs)


    def add_markers(self):
        print('add_markers: ' + str(self.number_markers) + ' markers will be injected each second automatically.')
        for m in range(self.number_markers):
            marker_time = time.time()*1000
            print('add marker at : ', marker_time)
            
            # marker_value = "test marker value"
            marker_label = self.marker_label +"_" +  str(m)

            self.inject_marker(marker_time, self.marker_value, marker_label, port='python_app')

            # add marker each 3 seconds
            time.sleep(3)

    def inject_marker(self, time, value, label, **kwargs):
        """
        To create an "instance" marker to the current record of a session
        Parameters
        ----------
        Please reference to https://emotiv.gitbook.io/cortex-api/markers/injectmarker
        Returns
        -------
        None
        """
        self.c.inject_marker_request(time, value, label, **kwargs)

    def update_marker(self, markerId, time, **kwargs):
        """
        To update a marker that was previously created by inject_marker
        Parameters
        ----------
        Please reference to https://emotiv.gitbook.io/cortex-api/markers/updatemarker
        Returns
        -------
        None
        """
        self.c.update_marker_request(markerId, time, **kwargs)

    # callbacks functions
    def on_create_session_done(self, *args, **kwargs):
        print('on_create_session_done')

        # create a record
        self.create_record(self.record_title, description=self.record_description)

    def on_create_record_done(self, *args, **kwargs):
        
        data = kwargs.get('data')
        self.record_id = data['uuid']
        start_time = data['startDatetime']
        title = data['title']
        print('on_create_record_done: recordId: {0}, title: {1}, startTime: {2}'.format(self.record_id, title, start_time))

        # inject markers
        th = threading.Thread(target=self.add_markers)
        th.start()

    def on_stop_record_done(self, *args, **kwargs):
        
        data = kwargs.get('data')
        record_id = data['uuid']
        start_time = data['startDatetime']
        end_time = data['endDatetime']
        title = data['title']
        print('on_stop_record_done: recordId: {0}, title: {1}, startTime: {2}, endTime: {3}'.format(record_id, title, start_time, end_time))

        # disconnect headset to export record
        print('on_stop_record_done: Disconnect the headset to export record')
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
            self.stop_record()

    def on_warn_cortex_stop_all_sub(self, *args, **kwargs):
        print('on_warn_cortex_stop_all_sub')
        # cortex has closed session. Wait some seconds before exporting record
        time.sleep(3)

        self.export_record(self.record_export_folder, self.record_export_data_types,
                           self.record_export_format, [self.record_id], self.record_export_version)

    def on_export_record_done(self, *args, **kwargs):
        print('on_export_record_done')
        data = kwargs.get('data')
        print(data)
        self.c.close()

    def on_inform_error(self, *args, **kwargs):
        error_data = kwargs.get('error_data')
        print(error_data)
        

# -----------------------------------------------------------
# 
# GETTING STARTED
#   - Please reference to https://emotiv.gitbook.io/cortex-api/ first.
#   - Connect your headset with dongle or bluetooth. You can see the headset via Emotiv Launcher
#   - Please make sure the your_app_client_id and your_app_client_secret are set before starting running.
#   - In the case you borrow license from others, you need to add license = "xxx-yyy-zzz" as init parameter
#   - Check the on_create_session_done() to see a record is created.
# RESULT
#   - record data then inject marker each 3 seconds
#   - export data file should contain marker added
# 
# -----------------------------------------------------------

def main():
    
    # Please fill your application clientId and clientSecret before running script
    your_app_client_id = ''
    your_app_client_secret = ''

    m = Marker(your_app_client_id, your_app_client_secret)

    # input params for create_record. Please see on_create_session_done before running script
    m.record_title = '' # required param and can not be empty
    m.record_description = '' # optional param

    # marker input for inject marker. Please see add_markers()
    m.marker_value = "test value" # required param and can not be empty
    m.marker_label = "test label" #required param and can not be empty

    # input params for export_record. Please see on_warn_cortex_stop_all_sub()
    m.record_export_folder = '' # your place to export, you should have write permission, example on desktop
    m.record_export_data_types = ['EEG', 'MOTION', 'PM', 'BP']
    m.record_export_format = 'CSV'
    m.record_export_version = 'V2'

    marker_numbers = 10
    m.start(marker_numbers)

if __name__ =='__main__':
    main()

# ----------------------------------------------