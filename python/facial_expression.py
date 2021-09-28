from cortex import Cortex

class Train():
	"""
    A class to use BCI API to control the training of the facial expression detections.

    Attributes
    ----------
    c : Cortex
        Cortex communicate with Emotiv Cortex Service

    Methods
    -------
    do_prepare_steps():
        Do prepare steps before training.
    subscribe_data():
        To subscribe to one or more data streams.
    load_profile(profile_name):
        To load an existed profile or create new profile for training
    unload_profile(profile_name):
        To unload an existed profile or create new profile for training
    train_fe(profile_name, training_action, number_of_train):
        To control the training of the facial expression action.
    live(profile_name):
        Load a trained profiles then subscribe facial expression data to enter live mode
    on_new_data(*args, **kwargs):
        To handle facial expression data emitted from Cortex
    """

	def __init__(self):
		"""
        Constructs cortex client and bind a function to handle subscribed data streams for the Train object
		If you do not want to log request and response message , set debug_mode = False. The default is True
        """
		self.c = Cortex(user, debug_mode=True)
		self.c.bind(new_fe_data=self.on_new_data)


	def do_prepare_steps(self):
		"""
        Do prepare steps before training.
        Step 1: Connect a headset. For simplicity, the first headset in the list will be connected in the example.
                If you use EPOC Flex headset, you should connect the headset with a proper mappings via EMOTIV Launcher first 
        Step 2: requestAccess: Request user approval for the current application for first time.
                       You need to open EMOTIV Launcher to approve the access
        Step 3: authorize: to generate a Cortex access token which is required parameter of many APIs
        Step 4: Create a working session with the connected headset
        Returns
        -------
        None
        """
		self.c.do_prepare_steps()

	def subscribe_data(self, streams):
		"""
        To subscribe to one or more data streams
        'com': Mental command
        'fac': Facial expression
        'sys': Training Event

        Parameters
        ----------
        streams : list, required
            list of streams. For example, ['fac']

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
		profiles = self.c.query_profile()

		if profile_name not in profiles:
			status = 'create'
			self.c.setup_profile(profile_name, status)

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
		profiles = self.c.query_profile()

		if profile_name in profiles:
			status = 'unload'
			self.c.setup_profile(profile_name, status)
		else:
			print("The profile " + profile_name + " is not existed.")

	def train_fe(self, profile_name, training_action, number_of_train):
		"""
        To control the training of the facial expression action.
        Make sure the headset is at good contact quality. You need to focus during 8 seconds for training an action.
        For simplicity, the training will be called to accepted automatically and then the training will be saved.

        Parameters
        ----------
        profile_name : string, required
            name of training profile
        training_action : string, required
            facial expression action, for example: neutral, surprise, smile
		number_of_train : int, required
            number of training for the action
        Returns
        -------
        None
        """

		print('begin train -----------------------------------')
		num_train = 0		
		while num_train < number_of_train:
			num_train = num_train + 1

			print('start training {0} time {1} ---------------'.format(training_action, num_train))
			print('\n')
			status='start'			
			self.c.train_request(detection='facialExpression',
								action=training_action,
								status=status)

			print('accept {0} time {1} ---------------'.format(training_action, num_train))
			print('\n')
			status='accept'
			self.c.train_request(detection='facialExpression',
								action=training_action, 
								status=status)
		
		print('save trained action')
		status = "save"
		self.c.setup_profile(profile_name, status)


	def live(self, profile_name):
		"""
        live facial expression actions

        Returns
        -------
        None
        """
		print('begin live mode ----------------------------------')
		# load profile
		status = 'load'
		self.c.setup_profile(profile_name, status)

		# sub 'fac' stream and view live mode
		stream = ['fac']
		self.c.sub_request(stream)

	def on_new_data(self, *args, **kwargs):
		"""
        To handle facial expression data emitted from Cortex

        Returns
        -------
        data: dictionary
             the format such as {'eyeAct': 'neutral', 'uAct': 'neutral', 'uPow': 0,'lAct': 'clench', 'lPow': 0.0576, 'time': 1590736942.8479}
        """
		data = kwargs.get('data')
		print('fe data: {}'.format(data))


# -----------------------------------------------------------
'''
SETTING
	- replace your license, client_id, client_secret to user dic
	- naming your profile
	- connect your headset with dongle or bluetooth, you should saw headset on EmotivApp.
	  make sure the headset at good contact quality.

TRAIN
	you need to folow steps:
		1) do_prepare_steps: for authorization, connect headset and create working session.
		2) subscribe 'sys' data for Training Event
		3) load a profile with the connected headset
		4) do training actions one by one. Begin with neutral action

LIVE
	you can run live mode with the trained profile. the data as below:

	{'eyeAct': 'neutral', 'uAct': 'neutral', 'uPow': 0,'lAct': 'clench', 'lPow': 0.0576, 'time': 1590736942.8479}
	{'eyeAct': 'neutral', 'uAct': 'neutral', 'uPow': 0,'lAct': 'smile', 'lPow': 0.0576, 'time': 1590736942.9729}
'''
# -----------------------------------------------------------

"""
	client_id, client_secret:
	To get a client id and a client secret, you must connect to your Emotiv account on emotiv.com and create a Cortex app
	For training purpose, you should set empty string for license
"""
user = {
	"license" : "your emotivpro license, which could use for third party app",
	"client_id" : "your client id",
	"client_secret" : "your client secret",
	"debit" : 100
}

t=Train()

# name of training profile
profile_name = 'your trained profile name'
# number of training time for one action
number_of_train = 2

# Do prepare steps
t.do_prepare_steps()

# subscribe sys stream to receive Training Event
t.subscribe_data(['sys'])

# load existed profile or create a new profile
t.load_profile(profile_name)


# Training neutral action
training_action = 'neutral'
t.train_fe(profile_name, training_action, number_of_train)

# # add active action

# Training push action
training_action = 'surprise'
t.train_fe(profile_name, training_action, number_of_train)

# unload profile
t.unload_profile(profile_name)

# start live mode with profile
t.live(profile_name)
# -----------------------------------------------------------






