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
			self.c.train_request(detection='mentalCommand',
								action=training_action,
								status=status)

			print('accept {0} time {1} ---------------'.format(training_action, num_train))
			print('\n')
			status='accept'
			self.c.train_request(detection='mentalCommand',
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
#	- 'push' action will show up after a while when you think 
#	similar thinking when training for 'push' action
# 
# 	{"id":6,"jsonrpc":"2.0","result":{"failure":[],"success":[{"cols":["act","pow"],"sid":"0b563e1e-9403-4f6a-b084-4e92713afe70","streamName":"com"}]}}
# 	{"com":["neutral",0.0],"sid":"abde5274-e33d-4373-b897-06778fccd619","time":1590736942.8479}
# 	{"com":["neutral",0.0],"sid":"abde5274-e33d-4373-b897-06778fccd619","time":1590736942.9729}
# 	{"com":["push",0.345774],"sid":"abde5274-e33d-4373-b897-06778fccd619","time":1590736943.0979}
# 	{"com":["push",0.294056],"sid":"abde5274-e33d-4373-b897-06778fccd619","time":1590736943.2229}
# 	{"com":["push",0.112473],"sid":"abde5274-e33d-4373-b897-06778fccd619","time":1590736943.3479}
# -----------------------------------------------------------
user = {
	"license" : "your emotivpro license, which could use for third party app",
	"client_id" : "your client id",
	"client_secret" : "your client secret",
	"debit" : 100
}

t=Train()
profile_name = 'your profile name'
number_of_train = 1


training_action = 'neutral'
t.train(profile_name,
		training_action,
		number_of_train)


training_action = 'push'
t.train(profile_name,
		training_action,
		number_of_train)

t.live(profile_name)
# -----------------------------------------------------------






