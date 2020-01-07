using System;

namespace CortexAccess
{
    public class SessionCreator
    {
        private string _sessionId;
        private string _applicationId;
        private SessionStatus _status;
        private CortexClient _ctxClient;
        private string _cortexToken;

        //event
        public event EventHandler<string> OnSessionCreated;
        public event EventHandler<string> OnSessionClosed;

        //Constructor
        public SessionCreator()
        {
            _sessionId = "";
            _applicationId = "";
            _cortexToken = "";

            _ctxClient = CortexClient.Instance;

            _ctxClient.OnCreateSession += CreateSessionOk;
            _ctxClient.OnUpdateSession += UpdateSessionOk;
        }

        private void CreateSessionOk(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Session " + e.SessionId + " is created successfully.");
            _sessionId = e.SessionId;
            _status = e.Status;
            _applicationId = e.ApplicationId;
            OnSessionCreated(this, _sessionId);
        }
        private void UpdateSessionOk(object sender, SessionEventArgs e)
        {
            _status = e.Status;
            if (_status == SessionStatus.Closed)
            {
                OnSessionClosed(this, e.SessionId);
                _sessionId = "";
                _cortexToken = "";
            }
            else if (_status == SessionStatus.Activated)
            {
                _sessionId = e.SessionId;
                OnSessionCreated(this, _sessionId);
            }
        }

        // Property
        public string SessionId
        {
            get
            {
                return _sessionId;
            }
        }

        public SessionStatus Status
        {
            get
            {
                return _status;
            }
        }

        public string CortexToken
        {
            get
            {
                return _cortexToken;
            }
        }

        public string ApplicationId
        {
            get
            {
                return _applicationId;
            }
        }

        // Create
        public void Create(string cortexToken, string headsetId, bool activeSession = false)
        {
            if (!String.IsNullOrEmpty(cortexToken) &&
                !String.IsNullOrEmpty(headsetId))
            {
                _cortexToken = cortexToken;
                string status = activeSession ? "active" : "open";
                _ctxClient.CreateSession(CortexToken, headsetId, status);
            }
            else
            {
                Console.WriteLine("CreateSession: Invalid parameters");
            }
            
        }

        // Close Session
        public void CloseSession()
        {
            if (!String.IsNullOrEmpty(SessionId))
            {
                _ctxClient.UpdateSession(_cortexToken, _sessionId, "close");
            }
        }
    }
}
