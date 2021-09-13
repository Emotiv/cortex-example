from cortex import Cortex

class LiveAdvance():
	"""
    A class to show mental command data at live mode of trained profile.
    You can load a profile trained on EmotivBCI or via train.py example

    Attributes
    ----------
    c : Cortex
        Cortex communicate with Emotiv Cortex Service

    Methods
    -------
    do_prepare_steps():
        Do prepare steps before training.
    load_profile(profile_name):
        To load an existed profile or create new profile for training
    unload_profile(profile_name):
        To unload an existed profile or create new profile for training
    get_active_action(profile_name):
        To get active actions for the mental command detection.
    get_sensitivity(profile_name):
        To get the sensitivity of the 4 active mental command actions.
    get_sensitivity(profile_name):
        To set the sensitivity of the 4 active mental command actions.
    live(profile_name):
        Load a trained profiles then subscribe mental command data to enter live mode
    on_new_data(*args, **kwargs):
        To handle mental command data emitted from Cortex
    """
	def __init__(self):
		"""
        Constructs cortex client and bind a function to handle subscribed data streams for the Train object
		If you do not want to log request and response message , set debug_mode = False. The default is True
        """
		self.c = Cortex(user, debug_mode=True)
		self.c.bind(new_com_data=self.on_new_data)

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

	def live(self, profile_name):
		"""
        Subscribe mental command data to enter live mode of trained profile

        Returns
        -------
        None
        """
		print('begin live mode ----------------------------------')
		self.c.sub_request(stream=['com'])

	def on_new_data(self, *args, **kwargs):
		"""
        To handle mental command data emitted from Cortex
        
        Returns
        -------
        data: dictionary
             the format such as {'action': 'neutral', 'power': 0.0, 'time': 1590736942.8479}
        """
		data = kwargs.get('data')
		print('mc data: {}'.format(data))

	#load if profile existed
	def load_profile(self, profile_name):
		"""
        To load an existed profile

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
			status = 'load'
			self.c.setup_profile(profile_name, status)
		else:
			print("The profile " + profile_name + " is not existed.")

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

	def get_active_action(self, profile_name):
		"""
        To get active actions for the mental command detection.
        Maximum 4 mental command actions are actived. This doesn't include "neutral"

        Parameters
        ----------
        profile_name : str, required
            profile name

        Returns
        -------
        None
        """
		self.c.get_mental_command_active_action(profile_name)

	def get_sensitivity(self, profile_name):
		"""
        To get the sensitivity of the 4 active mental command actions. This doesn't include "neutral"
        It will return arrays of 4 numbers, range 1 - 10
        The order of the values must follow the order of the active actions, as returned by mentalCommandActiveAction
        If the number of active actions < 4, the rest numbers are ignored.

        Parameters
        ----------
        profile_name : str, required
            profile name

        Returns
        -------
        None
        """
		self.c.get_mental_command_action_sensitivity(profile_name)

	def set_sensitivity(self, profile_name, values):
		"""
        To set the sensitivity of the 4 active mental command actions. This doesn't include "neutral".
        The order of the values must follow the order of the active actions, as returned by mentalCommandActiveAction
        
        Parameters
        ----------
        profile_name : str, required
            profile name
        values: list, required
        	list of sensitivity values. The range is from 1 (lowest sensitivy) - 10 (higest sensitivity)
        	For example: [neutral, push, pull, lift, drop] -> sensitivity [7, 8, 3, 6] <=> push : 7 , pull: 8, lift: 3, drop:6
					     [neutral, push, pull] -> sensitivity [7, 8, 5, 5] <=> push : 7 , pull: 8  , others resvered


        Returns
        -------
        None
        """
		self.c.set_mental_command_action_sensitivity(profile_name, values)

		status = "save"
		self.c.setup_profile(profile_name, status)

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

# name of training profile
profile_name = 'your trained profile name'

# Init live advance
l = LiveAdvance()

# do prepare steps
l.do_prepare_steps()

# load existed profile
l.load_profile(profile_name)

# get active actions
l.get_active_action(profile_name)

# get sensitivity values of actions
l.get_sensitivity(profile_name)

# set sensitivity for active actions
values = [7,7,5,5]
l.set_sensitivity(profile_name, values)


# live mental command data
l.live(profile_name)

# -----------------------------------------------------------
