import cortex
from cortex import Cortex

class Train():
    """
    A class to use BCI API to control the training of the mental command detections.

    Attributes
    ----------
    c : Cortex
        Cortex communicate with Emotiv Cortex Service

    Methods
    -------
    start():
        to start a training process from starting a websocket

    subscribe_data(streams):
        To subscribe one or more data streams 
    unload_profile(profile_name):
        To unload an profile
    load_profile(profile_name):
        To load an  profile for training
    train_mc_action(status):
        To control training of mentalCommand action

    Callbacks functions
    -------------------
    on_create_session_done(*args, **kwargs):
        to handle create_session_done which inform when session created successfully
    on_query_profile_done(*args, **kwargs):
        to handle query_profile_done which inform when query_profile done
    on_load_unload_profile_done(*args, **kwargs):
        to handle load_unload_profile_done which inform when profile is loaded or unloaded successfully
    on_save_profile_done(*args, **kwargs):
        to handle save_profile_done which inform when profile is saved successfully
    on_new_data_labels(*args, **kwargs):
        to handle new_data_labels which inform when sys event is subscribed successfully
    on_new_sys_data(*args, **kwargs):
        to handle new_sys_data which inform when sys event is streamed
    """

    def __init__(self, app_client_id, app_client_secret, **kwargs):
        self.c = Cortex(app_client_id, app_client_secret, debug_mode=True, **kwargs)
        self.c.bind(create_session_done=self.on_create_session_done)
        self.c.bind(query_profile_done=self.on_query_profile_done)
        self.c.bind(load_unload_profile_done=self.on_load_unload_profile_done)
        self.c.bind(save_profile_done=self.on_save_profile_done)
        self.c.bind(new_data_labels=self.on_new_data_labels)
        self.c.bind(new_sys_data=self.on_new_sys_data)
        self.c.bind(inform_error=self.on_inform_error)

    def start(self, profile_name, actions, headsetId=''):
        """
        To start training process as below workflow
        (1) check access right -> authorize -> connect headset->create session
        (2) query profile -> get current profile -> load/create profile -> subscribe sys
        (3) start and accept MC action training in the action list one by one
        Parameters
        ----------
        profile_name : string, required
            name of profile
        actions : list, required
            list of actions which will be trained
        headsetId: string , optional
             id of wanted headet which you want to work with it.
             If the headsetId is empty, the first headset in list will be set as wanted headset
        Returns
        -------
        None
        """
        if profile_name == '':
            raise ValueError('Empty profile_name. The profile_name cannot be empty.')

        self.profile_name = profile_name
        self.actions = actions
        self.action_idx = 0

        self.c.set_wanted_profile(profile_name)

        if headsetId != '':
            self.c.set_wanted_headset(headsetId)

        self.c.open()

    def subscribe_data(self, streams):
        """
        To subscribe to one or more data streams
        'com': Mental command
        'fac' : Facial expression
        'sys': training event

        Parameters
        ----------
        streams : list, required
            list of streams. For example, ['sys']

        Returns
        -------
        None
        """
        self.c.sub_request(streams)

    def load_profile(self, profile_name):
        """
        To load an existed profile or create new profile for training

        Parameters
        ----------
        profile_name : str, required
            profile name

        Returns
        -------
        None
        """

        status = 'load'
        self.c.setup_profile(profile_name, status)

    def unload_profile(self, profile_name):
        """
        To unload an existed profile or create new profile for training

        Parameters
        ----------
        profile_name : str, required
            profile name

        Returns
        -------
        None
        """
        self.c.setup_profile(profile_name, 'unload')

    def save_profile(self, profile_name):
        """
        To save a profile

        Parameters
        ----------
        profile_name : str, required
            profile name

        Returns
        -------
        None
        """
        self.c.setup_profile(profile_name, 'save')

    def get_active_action(self, profile_name):
        self.c.get_mental_command_active_action(profile_name)

    def get_command_brain_map(self, profile_name):
        self.c.get_mental_command_brain_map(profile_name)

    def get_training_threshold(self):
        self.c.get_mental_command_training_threshold(profile_name)

    def train_mc_action(self, status):
        """
        To control the training of the mental command action.
        Make sure the headset is at good contact quality. You need to focus during 8 seconds for training an action.
        For simplicity, the example will train action by action in the actions list

        Parameters
        ----------
        status : string, required
            to control training: there are 5 types: start, accept, reject, erase, reset
        Returns
        -------
        None
        """
        if self.action_idx < len(self.actions):
            action = self.actions[self.action_idx]
            print('train_mc_action: -----------------------------------: '+ action + ":" + status)
            self.c.train_request(detection='mentalCommand',
                                 action=action,
                                 status=status)
        else:
            # save profile after training
            print('train_mc_action: -----------------------------------: Done') 
            self.c.setup_profile(self.profile_name, 'save')
            self.action_idx = 0 # reset action_idx

    # callbacks functions
    def on_create_session_done(self, *args, **kwargs):
        print('on_create_session_done')
        self.c.query_profile()

    def on_query_profile_done(self, *args, **kwargs):
        print('on_query_profile_done')
        self.profile_lists = kwargs.get('data')
        if self.profile_name in self.profile_lists:
            # the profile is existed
            self.c.get_current_profile()
        else:
            # create profile
            self.c.setup_profile(self.profile_name, 'create')

    def on_load_unload_profile_done(self, *args, **kwargs):
        is_loaded = kwargs.get('isLoaded')
        
        if is_loaded == True:
            # subscribe sys stream to receive Training Event
            self.subscribe_data(['sys'])
        else:
            print('The profile ' + self.profile_name + ' is unloaded')
            self.profile_name = ''
            # close socket
            self.c.close()

    def on_save_profile_done (self, *args, **kwargs):
        print('Save profile ' + self.profile_name + " successfully.")
        # You can test some advanced bci such as active actions, brain map, and training threshold. before unload profile
        self.unload_profile(self.profile_name)

    def on_new_sys_data (self, *args, **kwargs):
        data = kwargs.get('data')
        train_event = data[1]
        action = self.actions[self.action_idx]
        print('on_new_sys_data: ' + action +" : " + train_event)
        if train_event == 'MC_Succeeded':
            # train action successful. you can accept the training to complete or reject the training
            self.train_mc_action('accept')
        elif train_event == 'MC_Failed':
            self.train_mc_action("reject")
        elif train_event == 'MC_Completed' or train_event == 'MC_Rejected':
            # training complete. Move to next action
            self.action_idx = self.action_idx + 1
            self.train_mc_action('start')

    def on_new_data_labels(self, *args, **kwargs):
        data = kwargs.get('data')
        print('on_new_data_labels')
        print(data)
        if data['streamName'] == 'sys':
            # subscribe sys event successfully
            # start training
            print('on_new_data_labels: start training ')
            self.train_mc_action('start')

    def on_inform_error(self, *args, **kwargs):
        error_data = kwargs.get('error_data')
        error_code = error_data['code']
        error_message = error_data['message']

        print(error_data)

        if error_code == cortex.ERR_PROFILE_ACCESS_DENIED:
            # disconnect headset for next use
            print('Get error ' + error_message + ". Disconnect headset to fix this issue for next use.")
            self.c.disconnect_headset()

# -----------------------------------------------------------
# 
# GETTING STARTED
#   - Please reference to https://emotiv.gitbook.io/cortex-api/ first.
#   - Connect your headset with dongle or bluetooth. You can see the headset via Emotiv Launcher
#   - Please make sure the your_app_client_id and your_app_client_secret are set before starting running.
#   - The function on_create_session_done,  on_query_profile_done, on_load_unload_profile_done will help 
#          handle create and load an profile automatically . So you should not modify them
#   - The functions on_new_data_labels(), on_new_sys_data() will help to control  action by action training.
#          You can modify these functions to control the training such as: reject an training, use advanced bci api.
# RESULT
#   - train mental command action
# 
# -----------------------------------------------------------

def main():

    # Please fill your application clientId and clientSecret before running script
    your_app_client_id = ''
    your_app_client_secret = ''

    # Init Train
    t=Train(your_app_client_id, your_app_client_secret)

    profile_name = '' # set your profile name. If the profile is not exited it will be created.

    # list actions which you want to train
    actions = ['neutral', 'push', 'pull']
    t.start(profile_name, actions)

if __name__ =='__main__':
    main()
# -----------------------------------------------------------