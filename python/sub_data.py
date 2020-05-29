from cortex import Cortex

class Subcribe():
	def __init__(self):
		self.c = Cortex(user, debug_mode=True)
		self.c.do_prepare_steps()

	def sub(self, streams):
		self.c.sub_request(streams)



# -----------------------------------
user = {
	"license" : "your emotivpro license, which could use for third party app",
	"client_id" : "your client id",
	"client_secret" : "your client secret",
	"debit" : 100
}

s = Subcribe()

# sub all streams
streams = ['eeg','mot','met','pow']

# or only sub for eeg
streams = ['eeg']

s.sub(streams)
# -----------------------------------
