from datetime import datetime
import json
import ssl
import time
# install with pip install websocket-client
import websocket

class Cortex():
	def __init__(self, url, user):
		self.ws = websocket.create_connection(url, sslopt={"cert_reqs": ssl.CERT_NONE})
		self.user = user

	def query_headset(self):
		QUERY_HEADSET_ID = 2		
		
		query_headset_request = {
			"jsonrpc": "2.0", 
			"id": QUERY_HEADSET_ID,
			"method": "queryHeadsets",
			"params": {}
		}

		self.ws.send(json.dumps(query_headset_request))
		result = self.ws.recv()
		result_dic = json.loads(result)
		# print('query headset result', json.dumps(result_dic, indent=4))
		self.headset_id = result_dic['result'][0]['id']
		print(self.headset_id)

	def connect_headset(self):
		CONNECT_HEADSET_ID = 111
		
		connect_headset_request = {
			"jsonrpc": "2.0", 
			"id": CONNECT_HEADSET_ID,
			"method": "controlDevice",
			"params": {
				"command": "connect",
	    		"headset": self.headset_id
			}
		}

		self.ws.send(json.dumps(connect_headset_request))
		result = self.ws.recv()
		result_dic = json.loads(result)
		print('connect headset result', json.dumps(result_dic, indent=4))


	def request_access(self):
		REQUEST_ACCESS_ID = 1
		request_access_request = {
			"jsonrpc": "2.0", 
			"method": "requestAccess",
			"params": {
				"clientId": self.user['client_id'], 
				"clientSecret": self.user['client_secret']
			},
			"id": REQUEST_ACCESS_ID
		}
		self.ws.send(json.dumps(request_access_request))
		result = self.ws.recv()
		result_dic = json.loads(result)
		print(json.dumps(result_dic, indent=4))


	def authorize(self):
		AUTHORIZE_ID = 4
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
		print('json.dumps(authorize_request)', json.dumps(authorize_request))
		self.ws.send(json.dumps(authorize_request))
		result = self.ws.recv()
		result_dic = json.loads(result)
		print('auth_result', json.dumps(result_dic, indent=4))
		self.auth = result_dic['result']['cortexToken']
		print(self.auth)


	def create_session(self, auth, headset_id):
		CREATE_SESSION_ID = 5
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
		self.ws.send(json.dumps(create_session_request))
		result = self.ws.recv()
		result_dic = json.loads(result)
		print('create session result ', json.dumps(result_dic, indent=4))
		self.session_id = result_dic['result']['id']
		# print(self.session_id)


	def close_session(self):
		CREATE_SESSION_ID = 117
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
		print('close session result ', json.dumps(result_dic, indent=4))



	def get_cortex_info(self):
		get_cortex_info_request = {
			"jsonrpc": "2.0",
			"method": "getCortexInfo",
			"id":100
		}
		self.ws.send(json.dumps(get_cortex_info_request))
		result = self.ws.recv()
		print(json.dumps(json.loads(result), indent=4))


	def grant_access_and_session_info(self):
		self.query_headset()
		self.connect_headset()
		self.request_access()
		self.authorize()
		self.create_session(self.auth, self.headset_id)

	def inject_marker_request(self, marker):
		INJECT_MARKER_REQUEST_ID = 13
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
		print('inject marker result', json.dumps(result_dic, indent=4))


	def query_record(self, cortexToken, record_id, record_name):
		QUERY_RECORD_ID = 100
		query_record_request = {
			"jsonrpc": "2.0",
			"method": "queryRecords",
			"params": {
				"cortexToken": cortexToken,
				 "query": {
				 	# "applicationId": "com.multikeo.aaaa",
					"uuid": record_id,
					# "keyword": record_name
				},
				"orderBy":[],
			},
			"id": QUERY_RECORD_ID
		}
		# print('query record', json.dumps(query_record_request))
		self.ws.send(json.dumps(query_record_request))
		result = self.ws.recv()
		result_dic = json.loads(result)
		print('query record result', json.dumps(result_dic, indent=4))

		uuid = result_dic['result']['records'][0]['uuid']
		# print('self.uuid', uuid)

		self.markers = self.get_record_info(uuid, cortexToken)['result'][0]['markers']
		

		# convet time from timestamp to epoc time
		for m in self.markers:
			m['epoch_time'] = 1000 * self.from_timestamp_to_epoch_time(m['endDatetime'])

		self.markers = sorted(self.markers, key=lambda k: k['epoch_time'])

		print('self.markers', json.dumps(self.markers, indent=4))

		self.markers_num = len(self.markers)


	def from_timestamp_to_epoch_time(self, timestamp):

		if timestamp == 'session not yet finish':
			return 'session not yet finish'
		# 2019-02-14T15:35:56.528+07:00
		has_timezone = False

		if '+' in timestamp:			
			timestamp = timestamp.replace("+07:00","")
			has_timezone = True

		timestamp = timestamp.replace('Z','')
		# print('timestamp', timestamp)
		utc_time = datetime.strptime(timestamp, "%Y-%m-%dT%H:%M:%S.%f")
		epoch_time = (utc_time - datetime(1970, 1, 1)).total_seconds()

		if has_timezone:
			return epoch_time - 7*3600
		else:
			return epoch_time


	def get_record_info(self, uuid, cortexToken):
		getRecordInfo = {
			"jsonrpc": "2.0",
			"id": 333,
			"method": "getRecordInfos",
			"params": {
				"cortexToken": cortexToken, 
				"recordIds":[uuid]
			}
		}
		print('getRecordInfo', getRecordInfo)
		self.ws.send(json.dumps(getRecordInfo))
		result = self.ws.recv()
		result_dic = json.loads(result)
		# print(json.dumps(result_dic, indent=4))
		return result_dic
	

	def create_record(self, 
					record_name, 
					record_description, 
					record_length_s,
					record_marker_numbers):

		self.grant_access_and_session_info()
		self.start_record(record_name, record_description)
		self.inject_markers(record_length_s, record_marker_numbers)
		self.stop_record(record_name, record_description)
		print('self.record_id', self.record_id)


	def inject_markers(self, record_length_s, record_marker_numbers):
		self.marker_added = []
		
		time_between_marker = record_length_s / record_marker_numbers
		
		for m in range(0, record_marker_numbers):
			mk_time = time.time()*1000
			print('moment send marker request', mk_time)
			marker = {
				"label":str(m),
				"value":"test_marker",
				"port":"python-app",
				"time":mk_time
			}
			self.marker_added.append(marker)
			self.inject_marker_request(marker)
			time.sleep(time_between_marker)

		self.marker_added = sorted(self.marker_added, key=lambda k: k['time'])
		print('self.marker_added', json.dumps(self.marker_added, indent=4))


	def disconnect_headset(self):
		DISCONNECT_HEADSET_ID = 112
		
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
			
			print('disconnect headset result', json.dumps(result_dic, indent=4))

			if 'warning' in result_dic:
				if result_dic['warning']['code'] == 1:
					break


	def export_record(self, 
					folder, 
					export_types, 
					export_format,
					export_version,
					record_ids):

		EXPORT_RECORD_ID = 113

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

		print('export record request ', json.dumps(export_record_request, indent=4))
		self.ws.send(json.dumps(export_record_request))

		# wait until export record completed
		while True:
			time.sleep(1)
			result = self.ws.recv()
			result_dic = json.loads(result)
			print('export record result', json.dumps(result_dic, indent=4))
			if 'result' in result_dic:
				if len(result_dic['result']['success']) > 0:
					break


	def create_and_export_record(self, 
								record_name, 
								record_description, 
								record_length_s,
								record_marker_numbers,
								record_export_folder,
								record_export_data_types,
								record_export_format,
								record_export_version):
		
		self.create_record(	record_name, 
							record_description, 
							record_length_s,
							record_marker_numbers)

		# need disconnect headset befor export
		self.disconnect_headset()


		self.export_record( record_export_folder,
							record_export_data_types,
							record_export_format,
							record_export_version,
							[self.record_id]
							)


	def start_record(self, record_name, description):
		CREATE_RECORD_REQUEST_ID = 11
		create_record_request = {
			"jsonrpc": "2.0", 
			"method": "createRecord",
			"params": {
				"cortexToken": self.auth,
				"session": self.session_id,
				"title": record_name,
				"description": description
			}, 

			"id": CREATE_RECORD_REQUEST_ID
		}
		print('start record request', json.dumps(create_record_request))
		self.ws.send(json.dumps(create_record_request))
		result = self.ws.recv()
		result_dic = json.loads(result)
		self.record_id = result_dic['result']['record']['uuid']


	def stop_record(self, record_name, description):
		STOP_RECORD_REQUEST_ID = 12
		stop_record_request = {
			"jsonrpc": "2.0", 
			"method": "stopRecord",
			"params": {
				"cortexToken": self.auth,
				"session": self.session_id
			}, 

			"id": STOP_RECORD_REQUEST_ID
		}
		print('stop request', json.dumps(stop_record_request))
		self.ws.send(json.dumps(stop_record_request))
		result = self.ws.recv()
		result_dic = json.loads(result)
		# print('stop result', json.dumps(result_dic, indent=4))
		# self.record_id = result_dic['result']['recordIds'][0]
		# print(self.record_id)



	def subRequest(self, stream):
		SUB_REQUEST_ID = 6 
		subRequest = { 
			"jsonrpc": "2.0", 
			"method": "subscribe", 
			"params": { 
				"cortexToken": self.auth,
				"session": self.session_id,
				"streams": stream
			}, 
			"id": SUB_REQUEST_ID
		}

		self.ws.send(json.dumps(subRequest))

		data = ""
		print('\n')
		print('subscribe result')
		for i in range(1, self.user['number_row_data']):
			new_data = self.ws.recv()
			data += new_data
			print(new_data)
		print('\n')

		return data


	def sub(self, stream):
		self.grant_access_and_session_info()
		self.subRequest(stream)



# ---------------------------------------------------
# NOTE : This code only run from version 2.1.1
# ---------------------------------------------------
url = "wss://localhost:6868"
user = {
	"license" : "your license",
	"client_id" : "your client id",
	"client_secret" : "your client ",
	"debit" : 100,
	"number_row_data" : 10
}


# init cortex instant
c = Cortex(url, user)

# record parameter
record_name = 'test_export'
record_description = 'test_export'
record_length_s = 60 * 1
record_marker_numbers = record_length_s

# export parameter
record_export_folder = 'C:\\export'
record_export_data_types = ['EEG', 'MOTION', 'PM', 'BP']
record_export_format = 'CSV'
record_export_version = 'V2'

# start record --> add marker --> stop marker --> disconnect headset --> export record
c.create_and_export_record(	record_name,
							record_description,
							record_length_s,
							record_marker_numbers,
							record_export_folder,
							record_export_data_types,
							record_export_format,
							record_export_version )
# ---------------------------------------------------