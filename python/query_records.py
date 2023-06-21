from cortex import Cortex

class Records():
    """
    A class to query records, request to download undownloaded records, and export local records

    Attributes
    ----------
    c : Cortex
        Cortex communicate with Emotiv Cortex Service

    Methods
    -------
    start():
        start authorize process.
    query_records():
        To query records owned by the current user
    request_download_records()
        To request to download records if they are not at local machine
    export_record()
        To export records to CSV/ EDF files
    """
    def __init__(self, app_client_id, app_client_secret, **kwargs):
        """
        Constructs cortex client and bind a function to query records,  request to download and export records
        If you do not want to log request and response message , set debug_mode = False. The default is True
        """
        print("Query Records  __init__")
        self.c = Cortex(app_client_id, app_client_secret, debug_mode=True, **kwargs)
        self.c.bind(authorize_done=self.on_authorize_done)
        self.c.bind(query_records_done=self.on_query_records_done)
        self.c.bind(export_record_done=self.on_export_record_done)
        self.c.bind(request_download_records_done=self.on_request_download_records_done)
        self.c.bind(inform_error=self.on_inform_error)

    def start(self):
        """
        To open websocket and process authorizing step.
        After get authorize_done The program will query_records
        Returns
        -------
        None
        """
        self.c.open()

    def query_records(self):
        """
        To query records
        Parameters
        ----------
        orderBy : array of object, required : Specify how to sort the records.
        query: object, required: An object to filter the records.
               If you set an empty object, it will return all records created by your application
               If you want get records created by other applications, you need set licenseId as parameter of query object
        More detail at https://emotiv.gitbook.io/cortex-api/records/queryrecords
        Returns
        -------
        """

        license_id = '' #Set this parameter to filter the records by their license.
        application_id =  "", # filter by applicationId but must set licenseId as well. for example: com.emotiv.emotivpro
        query_obj = {}
        if license_id != '':
            query_obj["licenseId"] = license_id
            if application_id != '':
                query_obj["applicationId"] = application_id

        query_params = {
                        "orderBy": [{ "startDatetime": "DESC" }],
                        "query": query_obj,
                        "includeSyncStatusInfo":True
                        }
        self.c.query_records(query_params)
    
    def request_download_records(self, record_ids):
        """
        To request to download records
        Parameters
        ----------
        record_ids : list, required: list of wanted record ids
        More detail at https://emotiv.gitbook.io/cortex-api/records/requesttodownloadrecorddata
        Returns
        -------
        None
        """
        self.c.request_download_records(record_ids)

    def export_record(self, record_ids):
        """
        To export records
        By default, you can only export the records that were created by your application.
        If you want to export a record that was created by another applications 
        then you must provide the license id of those applications in the parameter licenseIds.
        Parameters
        record_ids: list, required: list of wanted export record ids
        ----------
        More detail at https://emotiv.gitbook.io/cortex-api/records/exportrecord
        Returns
        -------
        None
        """
        folder = '' # your place to export, you should have write permission, for example: 'C:\\Users\\NTT\\Desktop'
        stream_types = ['EEG', 'MOTION', 'PM', 'BP']
        export_format = 'CSV'
        version = 'V2'

        # license_ids: If set empty list, which means that you can only export the records created by your application.
        #             You can provide the license ID of other applications in order to export the records created by these applications.
        license_ids = []

        self.c.export_record(folder, stream_types, export_format, record_ids, version, licenseIds=license_ids)

    
    # callbacks functions
    def on_authorize_done(self, *args, **kwargs):
        print('on_authorize_done')
        # query records 
        self.query_records()

    # callbacks functions
    def on_query_records_done(self, *args, **kwargs):
        data = kwargs.get('data')
        count = kwargs.get('count')
        print('on_query_records_done: total records are {0}'.format(count))
        # print(data)
        not_downloaded_record_Ids = []
        export_record_Ids = []
        for item in data:
            uuid = item['uuid']
            sync_status = item["syncStatus"]["status"]
            application_id = item["applicationId"]
            print("recordId {0}, applicationId {1}, sync status {2}".format(uuid, application_id, sync_status))
            if (sync_status == "notDownloaded") :
                not_downloaded_record_Ids.append(uuid)
            elif (sync_status == "neverUploaded") or (sync_status == "downloaded"):
                export_record_Ids.append(uuid)

        # download records has not downloaded to local machine
        if len(not_downloaded_record_Ids) > 0:
            self.request_download_records(not_downloaded_record_Ids)
        elif len(export_record_Ids) > 0: # or export records are in local machine
            self.export_record(export_record_Ids)

    def on_export_record_done(self, *args, **kwargs):
        print('on_export_record_done: the successful record exporting as below:')
        data = kwargs.get('data')
        print(data)
        self.c.close()

    def on_request_download_records_done(self, *args, **kwargs):
        data = kwargs.get('data')
        success_records = []
        for item in data['success']:
            record_Id = item['recordId']
            print('The record '+ record_Id + ' is downloaded successfully.')
            success_records.append(record_Id)

        for item in data['failure']:
            record_Id = item['recordId']
            failed_msg = item['message']
            print('The record '+ record_Id + ' is downloaded unsuccessfully. Because: ' + failed_msg)

        self.c.close()
    
    def on_inform_error(self, *args, **kwargs):
        error_data = kwargs.get('error_data')
        print(error_data)

# -----------------------------------------------------------
# 
# GETTING STARTED
#   - Please reference to https://emotiv.gitbook.io/cortex-api/ first.
#   - Please make sure the your_app_client_id and your_app_client_secret are set before starting running.
# RESULT
#   - on_query_records_done: will show all records filtered by query condition in query_record
#   - on_export_record_done: will show the successful exported record
#   - on_request_download_records_done: will show all success and failure download case
# 
# -----------------------------------------------------------

def main():

    # Please fill your application clientId and clientSecret before running script
    your_app_client_id = ''
    your_app_client_secret = ''
    
    # Don't need to create session in this case
    r = Records(your_app_client_id, your_app_client_secret, auto_create_session= False)
    
    r.start()

if __name__ =='__main__':
    main()

# -----------------------------------------------------------