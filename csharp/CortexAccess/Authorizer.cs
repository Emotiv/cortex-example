using System;

namespace CortexAccess
{
    public class Authorizer
    {
        private CortexClient _ctxClient;
        private string _cortexToken;
        private string _emotivId;
        private bool _isEulaAccepted;
        private bool _hasAccessRight;
        private string _licenseID;
        private ushort _debitNo = 5; // default value

        // Event
        public event EventHandler<string> OnAuthorized;

        // Constructor
        public Authorizer()
        {
            _ctxClient = CortexClient.Instance;
            _cortexToken = "";
            _emotivId = "";
            _isEulaAccepted = false;
            _hasAccessRight = false;

            _ctxClient.OnConnected += ConnectedOK;
            _ctxClient.OnGetUserLogin += GetUserLoginOK;
            _ctxClient.OnUserLogin += UserLoginOK; // inform user loggin 
            _ctxClient.OnUserLogout += UserLogoutOK; // inform user log out
            _ctxClient.OnHasAccessRight += HasAccessRightOK;
            _ctxClient.OnRequestAccessDone += RequestAccessDone;
            _ctxClient.OnAccessRightGranted += AccessRightGrantedOK; // inform user have granted or rejected access right for the App
            _ctxClient.OnAuthorize += AuthorizedOK;
            _ctxClient.OnEULAAccepted += EULAAcceptedOK;
           
        }

        private void ConnectedOK(object sender, bool isConnected)
        {
            if (isConnected)
            {
                _ctxClient.GetUserLogin();
            }
            else
            {
                Console.WriteLine("Can not connect to Cortex. Please restart cortex service");
            }
        }

        private void EULAAcceptedOK(object sender, bool isEULAAccepted)
        {
            _isEulaAccepted = isEULAAccepted;
            if (isEULAAccepted)
            {
                // Authorize again
                _ctxClient.Authorize(_licenseID, _debitNo);
            }
            else
            {
                Console.WriteLine("User has not accepted eula. Please accept EULA on EMOTIV App to proceed");
            }
        }

        private void AuthorizedOK(object sender, string cortexToken)
        {
            if (!String.IsNullOrEmpty(cortexToken))
            {
                Console.WriteLine("Authorize successfully.");
                _cortexToken = cortexToken;
                _isEulaAccepted = true;
                OnAuthorized(this, _cortexToken);
            }
            else
            {
                _isEulaAccepted = false;
                Console.WriteLine("User has not accepted eula. Please accept EULA on EMOTIV App to proceed");
            }
        }

        private void AccessRightGrantedOK(object sender, bool isGranted)
        {
            if (isGranted)
            {
                if (String.IsNullOrEmpty(_cortexToken))
                {
                    _ctxClient.Authorize(_licenseID, _debitNo);
                }
            }
            else
            {
                Console.WriteLine("The access right to the Application has been rejected");
            }
        }

        private void RequestAccessDone(object sender, bool hasAccessRight)
        {
            if (hasAccessRight)
            {
                Console.WriteLine("The User has access right to this application.");
            }
            else
            {
                Console.WriteLine("The User has not granted access right to this application. Please use EMOTIV App to proceed.");
            }
        }

        private void HasAccessRightOK(object sender, bool hasAccessRight)
        {
            if (hasAccessRight)
            {
                // Authorize
                _ctxClient.Authorize(_licenseID, _debitNo);
            }
            else
            {
                _ctxClient.RequestAccess();
            }
        }

        private void UserLogoutOK(object sender, string message)
        {
            Console.WriteLine(message);
            _emotivId = "";
            _cortexToken = "";
            _isEulaAccepted = false;
            _hasAccessRight = false;
        }

        private void UserLoginOK(object sender, string message)
        {
            if (String.IsNullOrEmpty(EmotivId))
            {
                _ctxClient.GetUserLogin();
            }
        }

        private void GetUserLoginOK(object sender, string emotivId)
        {
            if (!String.IsNullOrEmpty(emotivId))
            {
                _emotivId = emotivId;

                // check has access right
                _ctxClient.HasAccessRights();
            }
            else
            {
                Console.WriteLine("You must login via EMOTIV App before working with Cortex");
            }
            
        }

        // Event



        // Property
        public string CortexToken
        {
            get
            {
                return _cortexToken;
            }
        }

        public string EmotivId
        {
            get
            {
                return _emotivId;
            }
        }

        public bool IsEulaAccepted
        {
            get
            {
                return _isEulaAccepted;
            }
        }

        public bool HasAccessRight
        {
            get
            {
                return _hasAccessRight;
            }
        }

        // Start
        public void Start(string licenseID ="")
        {
            _licenseID = licenseID;
            _ctxClient.Open();
        }
    }
}
