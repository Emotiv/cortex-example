############
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see <http://www.gnu.org/licenses/>
#############

from datetime import datetime
import os.path
import websockets
import ssl
import json
import logging

# Set up logging for websockets library
wslogger = logging.getLogger('websockets')
wslogger.setLevel(logging.INFO)
wslogger.addHandler(logging.StreamHandler())

logger = logging.getLogger('cortex')
logger.setLevel(logging.DEBUG)
logger.addHandler(logging.StreamHandler())


class CortexApiException(Exception):
    pass


class Cortex(object):
    CORTEX_URL = "wss://localhost:6868"

    def __init__(self, client_id_file_path):
        self.parse_client_id_file(client_id_file_path)
        self.websocket = None
        self.auth_token = None
        self.packet_count = 0
        self.id_sequence = 0

    def parse_client_id_file(self, client_id_file_path):
        '''
        Parse a client_id file for client_id and client secret.

        Parameter:
            client_id_file_path: absolute or relative path to a client_id file

        We expect the client_id file to have the format:
        ```
        # optional comments start with hash
        client_id Jj2RihpwD6U3827GZ7J104URd1O9c0ZqBZut9E0y
        client_secret abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMN
        ```
        '''
        self.client_id = None
        self.client_secret = None
        if not os.path.exists(client_id_file_path):
            raise OSError("no such file: {}".format(client_id_file_path))
        with open(client_id_file_path, 'r') as client_id_file:
            for line in client_id_file:
                if line.startswith('#'):
                    continue
                (key, val) = line.split(' ')
                if key == 'client_id':
                    self.client_id = val.strip()
                elif key == 'client_secret':
                    self.client_secret = val.strip()
                else:
                    raise ValueError(
                        f'Found invalid key "{key}" while parsing '
                        f'client_id file {client_id_file_path}')

        if not self.client_id or not self.client_secret:
            raise ValueError(
                f"Did not find expected keys in client_id file "
                f"{client_id_file_path}")

    def to_epoch(self, time=None):
        '''
        Convert a python datetime to a unix epoch time.

        Parameters:
            time: input time; defaults to datetime.now()
        '''
        if not time:
            time = datetime.now()
        return int(datetime.timestamp(time) * 1000)

    def gen_request(self, method, auth, **kwargs):
        '''
        Generate a JSON request formatted for Cortex.

        Parameters:
            method: method name as a string
            auth: boolean indicating whether or not authentication is required
                for this method (may generate an additional call to
                authorize())
            **kwargs: all other keyword arguments become parameters in the
                request.
        '''
        self.id_sequence += 1
        params = {key: value for (key, value) in kwargs.items()}
        if auth and self.auth_token:
            params['cortexToken'] = self.auth_token
        request = json.dumps(
            {'jsonrpc': "2.0",
             'method': method,
             'params': params,
             'id': self.id_sequence
             })
        logger.debug(f"Sending request:\n{request}")
        return request

    async def init_connection(self):
        ''' Open a websocket and connect to cortex.  '''
        # Cortex is running locally; data is encrypted, but the certificate is
        # self-signed.
        ssl_context = ssl.create_default_context()
        ssl_context.check_hostname = False
        ssl_context.verify_mode = ssl.CERT_NONE

        self.websocket = await websockets.connect(
            self.CORTEX_URL, ssl=ssl_context)

    async def send_command(self, method, auth=True, callback=None, **kwargs):
        '''
        Send a command to cortex.

        Parameters:
            method: the cortex method to call as a string
            auth: boolean to indicate whether or not authentication is
                required for this method (may generate an additional call to
                authorize())
            callback: function to be called with the response data; defaults
                to returning the response data
            **kwargs: all other keyword arguments become parameters in the
                request to cortex.
        Returns: response as dictionary
        '''
        if not self.websocket:
            await self.init_connection()
        if auth and not self.auth_token:
            await self.authorize()
        msg = self.gen_request(method, auth, **kwargs)
        await self.websocket.send(msg)
        logger.debug("sent; awaiting response")
        resp = await self.websocket.recv()
        if 'error' in resp:
            logger.warn(f"Got error in {method} with params {kwargs}:\n{resp}")
            raise CortexApiException(resp)
        resp = json.loads(resp)
        if callback:
            callback(resp)
        return resp

    async def get_data(self):
        '''
        Get data from cortex.  Useful after calling the 'subscribe' method.
        The self.packet_count attribute can be used to limit data collection.

        '''
        resp = await self.websocket.recv()
        logger.debug(f"get_data got {resp}")
        self.packet_count += 1
        return resp

    def close(self):
        ''' Close the cortex connection '''
        self.websocket.close()

    ##
    # Here down are cortex specific commands
    # Each of them is documented thoroughly in the API documentation:
    # https://emotiv.gitbook.io/cortex-api
    ##
    async def inspectApi(self):
        ''' Return a list of available cortex methods '''
        resp = await self.send_command('inspectApi', auth=False)
        logger.debug(f"InspectApi resp:\n{resp}")

    async def authorize(self, license_id=None, debit=None):
        '''
        Generate an authorization token, required for most actions.
        Requires a valid license file, that the user be logged in via
        the Emotiv App, and that the user has granted access to this app.

        Optionally, a license_id can be specified to allow sharing a
        device-locked license across multiple users.

        Parameters:
            license_id (optional): a specific license to be used with the app.
                Can specify another user's license.
            debit (optional): number of sessions to debit from the license
        '''
        params = {'clientId': self.client_id,
                  'clientSecret': self.client_secret}
        if license_id:
            params['license'] = license_id
        if debit:
            params['debit'] = debit

        resp = await self.send_command('authorize', auth=False, **params)
        logger.debug(f"{__name__} resp:\n{resp}")
        self.auth_token = resp['result']['cortexToken']

    async def get_cortex_info(self):
        resp = await self.send_command('getCortexInfo', auth=False)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def get_license_info(self):
        resp = await self.send_command('getLicenseInfo')
        logger.debug(f"{__name__} resp:\n{resp}")

    async def query_headsets(self):
        resp = await self.send_command('queryHeadsets', auth=False)
        self.headsets = [h['id'] for h in resp['result']]
        logger.debug(f"{__name__} found headsets {self.headsets}")
        logger.debug(f"{__name__} resp:\n{resp}")

    async def get_user_login(self):
        return await self.send_command('getUserLogin', auth=False,
                                       callback=self.get_user_login_cb)

    def get_user_login_cb(self, resp):
        ''' Example of using the callback functionality of send_command '''
        resp = resp['result'][0]
        if 'loggedInOSUId' not in resp:
            logger.debug(resp)
            raise CortexApiException(
                f"No user logged in! Please log in with the Emotiv App")
        if (resp['currentOSUId'] != resp['loggedInOSUId']):
            logger.debug(resp)
            raise CortexApiException(
                f"Cortex is already in use by {resp.loggedInOSUsername}")
        logger.debug(f"{__name__} resp:\n{resp}")

    async def has_access_right(self):
        params = {'clientId': self.client_id,
                  'clientSecret': self.client_secret}
        resp = await self.send_command('requestAccess', auth=False, **params)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def request_access(self):
        params = {'clientId': self.client_id,
                  'clientSecret': self.client_secret}
        resp = await self.send_command('requestAccess', auth=False, **params)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def control_device(self, command, headset_id=None,
                             flex_mapping=None):
        if not headset_id:
            headset_id = self.headsets[0]
        params = {'headset': headset_id,
                  'command': command}
        if flex_mapping:
            params['mappings'] = flex_mapping
        resp = await self.send_command('controlDevice', **params)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def create_session(self, activate, headset_id=None):
        status = 'active' if activate else 'open'
        if not headset_id:
            headset_id = self.headsets[0]
        params = {'cortexToken': self.auth_token,
                  'headset': headset_id,
                  'status': status}
        resp = await self.send_command('createSession', **params)
        self.session_id = resp['result']['id']
        logger.debug(f"{__name__} resp:\n{resp}")

    async def close_session(self):
        params = {'cortexToken': self.auth_token,
                  'session': self.session_id,
                  'status': 'close'}
        resp = await self.send_command('updateSession', **params)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def subscribe(self, stream_list):
        ''' Options for streams to subscribe to include:
            eeg: EEG data
            mot: motion data
            dev: device data (battery, signal strength, etc)
            pow: EEG band power data
            met: performance metric data
            com: mental commands data
            fac: facial expression data
            sys: system events (training data, etc)
        '''

        params = {'cortexToken': self.auth_token,
                  'session': self.session_id,
                  'streams': stream_list}
        resp = await self.send_command('subscribe', **params)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def unsubscribe(self, stream_list):
        params = {'cortexToken': self.auth_token,
                  'session': self.session_id,
                  'streams': stream_list}
        resp = await self.send_command('unsubscribe', **params)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def query_profile(self):
        params = {'cortexToken': self.auth_token}
        resp = await self.send_command('queryProfile', **params)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def setup_profile(self):
        params = {'cortexToken': self.auth_token,
                  'profile': None,
                  'status': 'create'}
        resp = await self.send_command('setupProfile', **params)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def load_profile(self, headset_id=None):
        if not headset_id:
            headset_id = self.headsets[0]
        params = {'cortexToken': self.auth_token,
                  'headset': headset_id,
                  'profile': None,
                  'status': 'load'}
        resp = await self.send_command('setupProfile', **params)
        logger.debug(f"{__name__} resp:\n{resp}")

    async def save_profile(self, headset_id=None):
        if not headset_id:
            headset_id = self.headsets[0]
        params = {'cortexToken': self.auth_token,
                  'headset': headset_id,
                  'profile': None,
                  'status': 'save'}
        resp = await self.send_command('setupProfile', **params)
        logger.debug(f"{__name__} resp:\n{resp}")
        return resp

    async def get_detection_info(self):
        params = {'detection': None}
        resp = await self.send_command('getDetectionInfo', **params)
        logger.debug(f"{__name__} resp:\n{resp}")
        return resp

    async def training(self):
        params = {'cortexToken': self.auth_token,
                  'session': self.session_id,
                  'detection': None,
                  'action': None,
                  'status': None}
        resp = await self.send_command('training', **params)
        logger.debug(f"{__name__} resp:\n{resp}")
        return resp

    async def create_record(self, title=None):
        if not title:
            title = f'record {self.id_sequence}'
        params = {'cortexToken': self.auth_token,
                  'session': self.session_id,
                  'title': title}
        resp = await self.send_command('createRecord', **params)
        logger.debug(f"{__name__} resp:\n{resp}")
        return resp

    async def stop_record(self):
        params = {'cortexToken': self.auth_token,
                  'session': self.session_id}
        resp = await self.send_command('stopRecord', **params)
        logger.debug(f"{__name__} resp:\n{resp}")
        return resp

    async def inject_marker(self, label='', value=0, port='',
                            time=None):
        if not time:
            time = datetime.now()
        params = {'cortexToken': self.auth_token,
                  'session': self.session_id,
                  'label': label,
                  'value': value,
                  'port': port,
                  'time': time}
        resp = await self.send_command('injectMarker', **params)
        logger.debug(f"{__name__} resp:\n{resp}")
        return resp

    async def update_marker(self):
        params = {'cortexToken': self.auth_token,
                  'session': self.session_id,
                  'markerId': None,
                  'time': None}
        resp = await self.send_command('updateMarker', **params)
        logger.debug(f"{__name__} resp:\n{resp}")
        return resp
