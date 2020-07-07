from cortex import Cortex

class LiveAdvance():
	def __init__(self):
		self.c = Cortex(user, debug_mode=True)
		self.c.do_prepare_steps()

	def live(self, profile_name):
		print('begin live mode ----------------------------------')
		self.c.setup_profile(profile_name=profile_name, status='load')
		self.c.sub_request(stream=['com'])

	def get_sensitivity(self, profile_name):
		self.c.get_mental_command_action_sensitivity(profile_name)

	def set_sensitivity(self, profile_name, values):
		self.c.set_mental_command_action_sensitivity(profile_name, values)

# -----------------------------------------------------------
user = {
	"license" : "your emotivpro license, which could use for third party app",
	"client_id" : "your client id",
	"client_secret" : "your client secret",
	"debit" : 100
}

l = LiveAdvance()

profile_name = 'your trained profile name'

l.get_sensitivity(profile_name)

values = [7,7,7,7]
l.set_sensitivity(profile_name, values)

l.live(profile_name)
# -----------------------------------------------------------
