import websocket #'pip install websocket-client' for install
from datetime import datetime
import json
import ssl
import time
import sys


# define request id
QUERY_HEADSET_ID            = 1
CONNECT_HEADSET_ID          = 2
REQUEST_ACCESS_ID           = 3
AUTHORIZE_ID                = 4
CREATE_SESSION_ID           = 5
SUB_REQUEST_ID              = 6
SETUP_PROFILE_ID            = 7
QUERY_PROFILE_ID            = 8
TRAINING_ID                 = 9
DISCONNECT_HEADSET_ID       = 10
CREATE_RECORD_REQUEST_ID    = 11
STOP_RECORD_REQUEST_ID      = 12
EXPORT_RECORD_ID            = 13
INJECT_MARKER_REQUEST_ID    = 14

 
class Cortex():
    def __init__(self, user, debug_mode=False):
        url = "wss://localhost:6868"
        self.ws = websocket.create_connection(url, 
                                            sslopt={"cert_reqs": ssl.CERT_NONE})
        self.user = user
        self.debug = debug_mode

    def query_headset(self):
        print('query headset --------------------------------')        
        query_headset_request = {
            "jsonrpc": "2.0", 
            "id": QUERY_HEADSET_ID,
            "method": "queryHeadsets",
            "params": {}
        }

        self.ws.send(json.dumps(query_headset_request, indent=4))
        result = self.ws.recv()
        result_dic = json.loads(result)

        self.headset_id = result_dic['result'][0]['id']
        if self.debug:
            # print('query headset result', json.dumps(result_dic, indent=4))            
            print(self.headset_id)

    def connect_headset(self):
        print('connect headset --------------------------------')        
        connect_headset_request = {
            "jsonrpc": "2.0", 
            "id": CONNECT_HEADSET_ID,
            "method": "controlDevice",
            "params": {
                "command": "connect",
                "headset": self.headset_id
            }
        }

        self.ws.send(json.dumps(connect_headset_request, indent=4))
        result = self.ws.recv()
        result_dic = json.loads(result)

        if self.debug:
            print('connect headset result', json.dumps(result_dic, indent=4))


    def request_access(self):
        print('request access --------------------------------')
        request_access_request = {
            "jsonrpc": "2.0", 
            "method": "requestAccess",
            "params": {
                "clientId": self.user['client_id'], 
                "clientSecret": self.user['client_secret']
            },
            "id": REQUEST_ACCESS_ID
        }

        self.ws.send(json.dumps(request_access_request, indent=4))
        result = self.ws.recv()
        result_dic = json.loads(result)

        if self.debug:
            print(json.dumps(result_dic, indent=4))


    def authorize(self):
        print('authorize --------------------------------')
        authorize_request = {
            "jsonrpc": "2.0",
            "method": "authorize", 
            "params": { 
                "clientId": self.user['client_id'], 
                "clientSecret": self.user['client_secret'], 
                "license": self.user['license'],
                "debit": self.user['debit']
            },
            "id": AUTHORIZE_ID
        }

        if self.debug:
            print('auth request \n', json.dumps(authorize_request, indent=4))

        self.ws.send(json.dumps(authorize_request))
        
        while True:
            result = self.ws.recv()
            result_dic = json.loads(result)
            if 'id' in result_dic:
                if result_dic['id'] == AUTHORIZE_ID:
                    if self.debug:
                        print('auth result \n', json.dumps(result_dic, indent=4))
                    self.auth = result_dic['result']['cortexToken']
                    break


    def create_session(self, auth, headset_id):
        print('create session --------------------------------')
        create_session_request = { 
            "jsonrpc": "2.0",
            "id": CREATE_SESSION_ID,
            "method": "createSession",
            "params": {
                "cortexToken": self.auth,
                "headset": self.headset_id,
                "status": "active"
            }
        }
        
        if self.debug:
            print('create session request \n', json.dumps(create_session_request, indent=4))

        self.ws.send(json.dumps(create_session_request))
        result = self.ws.recv()
        result_dic = json.loads(result)

        if self.debug:
            print('create session result \n', json.dumps(result_dic, indent=4))

        self.session_id = result_dic['result']['id']


    def close_session(self):
        print('close session --------------------------------')
        close_session_request = { 
            "jsonrpc": "2.0",
            "id": CREATE_SESSION_ID,
            "method": "updateSession",
            "params": {
                "cortexToken": self.auth,
                "session": self.session_id,
                "status": "close"
            }
        }

        self.ws.send(json.dumps(close_session_request))
        result = self.ws.recv()
        result_dic = json.loads(result)

        if self.debug:
            print('close session result \n', json.dumps(result_dic, indent=4))


    def get_cortex_info(self):
        print('get cortex version --------------------------------')
        get_cortex_info_request = {
            "jsonrpc": "2.0",
            "method": "getCortexInfo",
            "id":100
        }

        self.ws.send(json.dumps(get_cortex_info_request))        
        result = self.ws.recv()
        if self.debug:
            print(json.dumps(json.loads(result), indent=4))

    def do_prepare_steps(self):
        self.query_headset()
        self.connect_headset()
        self.request_access()
        self.authorize()
        self.create_session(self.auth, self.headset_id)


    def disconnect_headset(self):
        print('disconnect headset --------------------------------')
        disconnect_headset_request = {
            "jsonrpc": "2.0", 
            "id": DISCONNECT_HEADSET_ID,
            "method": "controlDevice",
            "params": {
                "command": "disconnect",
                "headset": self.headset_id
            }
        }

        self.ws.send(json.dumps(disconnect_headset_request))

        # wait until disconnect completed
        while True:
            time.sleep(1)
            result = self.ws.recv()
            result_dic = json.loads(result)
            
            if self.debug:
                print('disconnect headset result', json.dumps(result_dic, indent=4))

            if 'warning' in result_dic:
                if result_dic['warning']['code'] == 1:
                    break


    def sub_request(self, stream):
        print('subscribe request --------------------------------')
        sub_request_json = {
            "jsonrpc": "2.0", 
            "method": "subscribe", 
            "params": { 
                "cortexToken": self.auth,
                "session": self.session_id,
                "streams": stream
            }, 
            "id": SUB_REQUEST_ID
        }

        self.ws.send(json.dumps(sub_request_json))
        
        if 'sys' in stream:
            new_data = self.ws.recv()
            print(json.dumps(new_data, indent=4))
            print('\n')
        else:
            while True:
                new_data = self.ws.recv()        
                print(new_data)


    def query_profile(self):
        print('query profile --------------------------------')
        query_profile_json = {
            "jsonrpc": "2.0",
            "method": "queryProfile",
            "params": {
              "cortexToken": self.auth,
            },
            "id": QUERY_PROFILE_ID
        }

        if self.debug:
            print('query profile request \n', json.dumps(query_profile_json, indent=4))
            print('\n')

        self.ws.send(json.dumps(query_profile_json))

        result = self.ws.recv()
        result_dic = json.loads(result)

        print('query profile result\n',result_dic)
        print('\n')

        profiles = []
        for p in result_dic['result']:
            profiles.append(p['name'])

        print('extract profiles name only')        
        print(profiles)
        print('\n')

        return profiles


    def setup_profile(self, profile_name, status):
        print('setup profile --------------------------------')
        setup_profile_json = {
            "jsonrpc": "2.0",
            "method": "setupProfile",
            "params": {
              "cortexToken": self.auth,
              "headset": self.headset_id,
              "profile": profile_name,
              "status": status
            },
            "id": SETUP_PROFILE_ID
        }
        
        if self.debug:
            print('setup profile json:\n', json.dumps(setup_profile_json, indent=4))
            print('\n')

        self.ws.send(json.dumps(setup_profile_json))

        result = self.ws.recv()
        result_dic = json.loads(result)

        if self.debug:
            print('result \n', json.dumps(result_dic, indent=4))
            print('\n')


    def train_request(self,action, status):
        print('train request --------------------------------')
        train_request_json = {
            "jsonrpc": "2.0", 
            "method": "training", 
            "params": {
              "cortexToken": self.auth,
              "detection": "mentalCommand",
              "session": self.session_id,
              "action": action,
              "status": status
            }, 
            "id": TRAINING_ID
        }

        # print('training request:\n', json.dumps(train_request_json, indent=4))
        # print('\n')

        self.ws.send(json.dumps(train_request_json))
        result = self.ws.recv()
        result_dic = json.loads(result)

        if result_dic['id']==TRAINING_ID:
            print(json.dumps(result_dic, indent=4))
            print('\n YOU HAVE 8 SECONDS FOR TRAIN ACTION {} \n'.format(action.upper()))
            

        if status == 'start':
            while True:
                result = self.ws.recv()
                result_dic = json.loads(result)
                print(json.dumps(result_dic))
                if result_dic['sys'][1]=='MC_Succeeded':
                    print('training result')
                    print(json.dumps(result_dic, indent=4))
                    print('\n')
                    return(result_dic)
                    break

        if status == 'accept':
            while True:
                result = self.ws.recv()
                result_dic = json.loads(result)
                if result_dic['sys'][1]=='MC_Completed':
                    print('accept training result')
                    print(json.dumps(result_dic, indent=4))
                    print('\n')
                    return(result_dic)
                    break


    def create_record(self,
                    record_name,
                    record_description):
        print('create record --------------------------------')
        create_record_request = {
            "jsonrpc": "2.0", 
            "method": "createRecord",
            "params": {
                "cortexToken": self.auth,
                "session": self.session_id,
                "title": record_name,
                "description": record_description
            }, 

            "id": CREATE_RECORD_REQUEST_ID
        }

        self.ws.send(json.dumps(create_record_request))
        result = self.ws.recv()
        result_dic = json.loads(result)

        if self.debug:
            print('start record request \n',
                    json.dumps(create_record_request, indent=4))
            print('start record result \n',
                    json.dumps(result_dic, indent=4))

        self.record_id = result_dic['result']['record']['uuid']



    def stop_record(self):
        print('stop record --------------------------------')
        stop_record_request = {
            "jsonrpc": "2.0", 
            "method": "stopRecord",
            "params": {
                "cortexToken": self.auth,
                "session": self.session_id
            }, 

            "id": STOP_RECORD_REQUEST_ID
        }
        
        self.ws.send(json.dumps(stop_record_request))
        result = self.ws.recv()
        result_dic = json.loads(result)

        if self.debug:
            print('stop request \n',
                json.dumps(stop_record_request, indent=4))
            print('stop result \n',
                json.dumps(result_dic, indent=4))


    def export_record(self, 
                    folder, 
                    export_types, 
                    export_format,
                    export_version,
                    record_ids):
        print('export record --------------------------------')
        export_record_request = {
            "jsonrpc": "2.0",
            "id":EXPORT_RECORD_ID,
            "method": "exportRecord", 
            "params": {
                "cortexToken": self.auth, 
                "folder": folder,
                "format": export_format,
                "streamTypes": export_types,
                "recordIds": record_ids
            }
        }

        # "version": export_version,
        if export_format == 'CSV':
            export_record_request['params']['version'] = export_version

        if self.debug:
            print('export record request \n',
                json.dumps(export_record_request, indent=4))
        
        self.ws.send(json.dumps(export_record_request))

        # wait until export record completed
        while True:
            time.sleep(1)
            result = self.ws.recv()
            result_dic = json.loads(result)

            if self.debug:            
                print('export record result \n',
                    json.dumps(result_dic, indent=4))

            if 'result' in result_dic:
                if len(result_dic['result']['success']) > 0:
                    break

    def inject_marker_request(self, marker):
        print('inject marker --------------------------------')
        inject_marker_request = {
            "jsonrpc": "2.0",
            "id": INJECT_MARKER_REQUEST_ID,
            "method": "injectMarker", 
            "params": {
                "cortexToken": self.auth, 
                "session": self.session_id,
                "label": marker['label'],
                "value": marker['value'], 
                "port": marker['port'],
                "time": marker['time']
            }
        }

        self.ws.send(json.dumps(inject_marker_request))
        result = self.ws.recv()
        result_dic = json.loads(result)

        if self.debug:
            print('inject marker request \n', json.dumps(inject_marker_request, indent=4))
            print('inject marker result \n',
                json.dumps(result_dic, indent=4))

# -------------------------------------------------------------------
# -------------------------------------------------------------------
# -------------------------------------------------------------------