from cortex import Cortex

class TrainAdvance():
	def __init__(self):
		self.c = Cortex(user, debug_mode=True)
		self.c.do_prepare_steps()

	def get_active_action(self, profile_name):
		self.c.get_mental_command_active_action(profile_name)

	def get_command_brain_map(self, profile_name):
		self.c.get_mental_command_brain_map(profile_name)

	def get_training_threshold(self):
		self.c.get_mental_command_training_threshold(profile_name)

# ---------------------------------------------------

user = {
	"license" : "your emotivpro license, which could use for third party app",
	"client_id" : "your client id",
	"client_secret" : "your client secret",
	"debit" : 100
}

t = TrainAdvance()

profile_name = 'your trained profile name'

t.get_active_action(profile_name)
t.get_command_brain_map(profile_name)
t.get_training_threshold()

# ---------------------------------------------------