from cortex import Cortex
import time

class Record():
    def __init__(self, app_client_id, app_client_secret, **kwargs):
        self.c = Cortex(app_client_id, app_client_secret, debug_mode=True, **kwargs)
        self.c.bind(create_session_done=self.on_create_session_done)
        self.c.bind(create_record_done=self.on_create_record_done)
        self.c.bind(stop_record_done=self.on_stop_record_done)
        self.c.bind(warn_cortex_stop_all_sub=self.on_warn_cortex_stop_all_sub)
        self.c.bind(export_record_done=self.on_export_record_done)
        self.c.bind(inform_error=self.on_inform_error)

    def start(self, record_duration_s=20, headsetId=''):
        """
        To start data recording and exporting process as below
        (1) check access right -> authorize -> connect headset->create session
        (2) start record --> stop record --> disconnect headset --> export record
        Parameters
        ----------
        record_duration_s: int, optional
            duration of record. default is 20 seconds

        headsetId: string , optional
             id of wanted headet which you want to work with it.
             If the headsetId is empty, the first headset in list will be set as wanted headset
        Returns
        -------
        None
        """
        self.record_duration_s = record_duration_s

        if headsetId != '':
            self.c.set_wanted_headset(headsetId)

        self.c.open()

    # custom exception hook
    def custom_hook(args):
        # report the failure
        print(f'Thread failed: {args.exc_value}')

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


    def export_record(self, folder, stream_types, format, record_ids,
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
        self.c.export_record(folder, stream_types, format, record_ids, version, **kwargs)

    def wait(self, record_duration_s):
        print('start recording -------------------------')
        length = 0
        while length < record_duration_s:
            print('recording at {0} s'.format(length))
            time.sleep(1)
            length+=1
        print('end recording -------------------------')

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

        # record duration is record_length_s
        self.wait(self.record_duration_s)

        # stop record
        self.stop_record()

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

    def on_warn_cortex_stop_all_sub(self, *args, **kwargs):
        print('on_warn_cortex_stop_all_sub')
        # cortex has closed session. Wait some seconds before exporting record
        time.sleep(3)

        #export record
        self.export_record(self.record_export_folder, self.record_export_data_types,
                           self.record_export_format, [self.record_id], self.record_export_version)

    def on_export_record_done(self, *args, **kwargs):
        print('on_export_record_done: the successful record exporting as below:')
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
#   - Check the on_create_session_done() to see how to create a record.
#   - Check the on_warn_cortex_stop_all_sub() to see how to export record
# RESULT
#   - record data 
#   - export recording data, the result should be csv or edf file at location you specified
#   - in that file will has data you specified like : eeg, motion, performance metric and band power
# 
# -----------------------------------------------------------


def main():
    
    # Please fill your application clientId and clientSecret before running script
    your_app_client_id = ''
    your_app_client_secret = ''

    r = Record(your_app_client_id, your_app_client_secret)


    # input params for create_record. Please see on_create_session_done before running script
    r.record_title = '' # required param and can not be empty
    r.record_description = '' # optional param

    # input params for export_record. Please see on_warn_cortex_stop_all_sub()
    r.record_export_folder = '' # your place to export, you should have write permission, example on desktop
    r.record_export_data_types = ['EEG', 'MOTION', 'PM', 'BP']
    r.record_export_format = 'CSV'
    r.record_export_version = 'V2'


    record_duration_s = 10 # duration for recording in this example. It is not input param of create_record
    r.start(record_duration_s)

if __name__ =='__main__':
    main()

# -----------------------------------------------------------
