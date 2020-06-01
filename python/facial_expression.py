from cortex import Cortex

class Train():
	def __init__(self):
		self.c = Cortex(user, debug_mode=True)
		self.c.do_prepare_steps()

	def train(self,
			profile_name,
			training_action,
			number_of_train):

		stream = ['sys']
		self.c.sub_request(stream)
		
		profiles = self.c.query_profile()

		if profile_name not in profiles:
			status = 'create'
			self.c.setup_profile(profile_name, status)

		status = 'load'
		self.c.setup_profile(profile_name, status)

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

		status = 'unload'
		self.c.setup_profile(profile_name, status)


	def live(self, profile_name):
		print('begin live mode ----------------------------------')
		# load profile
		status = 'load'
		self.c.setup_profile(profile_name, status)

		# sub 'com' stream and view live mode
		stream = ['com']
		self.c.sub_request(stream)


# -----------------------------------------------------------
# SETTING
# 	- replace your license, client_id, client_secret to user dic
# 	- naming your profile
# 	- connect your headset with dongle or bluetooth, you should saw headset on EmotivApp
#
#
# TRAIN
# 	you should have 3 separate runs
# 		- first run train for neutral only
# 		- second run train for push only
# 		- third run for live mode
#
# 
# RESULT
#	- "cols" in first row is order and name of output data
#	- 'surprise' action will show up after a while when you think 
#	similar thinking when training for 'surprise' action
# -----------------------------------------------------------
user = {
	"license" : "your emotivpro license, which could use for third party app",
	"client_id" : "your client id",
	"client_secret" : "your client secret",
	"debit" : 100
}

t=Train()
profile_name = 'your profile name'
number_of_train = 5


training_action = 'neutral'
t.train(profile_name,
		training_action,
		number_of_train)


training_action = 'surprise'
t.train(profile_name,
		training_action,
		number_of_train)

t.live(profile_name)
# -----------------------------------------------------------






