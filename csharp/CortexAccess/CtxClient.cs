using System;
using System.Threading;
using WebSocket4Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CortexAccess
{
    public enum SessionStatus
    {
        Opened = 0,
        Activated = 1,
        Closed = 2
    }

    // Event for subscribe  and unsubscribe
    public class MultipleResultEventArgs
    {
        public MultipleResultEventArgs(JArray successList, JArray failList)
        {
            SuccessList = successList;
            FailList = failList;
        }
        public JArray SuccessList { get; set; }
        public JArray FailList { get; set; }
    }

    // Event for createSession and updateSession
    public class SessionEventArgs
    {
        public SessionEventArgs(string sessionId, string status, string appId)
        {
            SessionId = sessionId;
            ApplicationId = appId;
            if (status == "opened")
                Status = SessionStatus.Opened;
            else if (status == "activated")
                Status = SessionStatus.Activated;
            else
                Status = SessionStatus.Closed;
        }
        public string SessionId { get; set; }
        public SessionStatus Status { get; set; }
        public string ApplicationId { get; set; }
    }
    public class StreamDataEventArgs
    {
        public StreamDataEventArgs(string sid, JArray data, double time, string streamName)
        {
            Sid = sid;
            Time = time;
            Data = data;
            StreamName = streamName;
        }
        public string Sid { get; private set; } // subscription id
        public double Time { get; private set; }
        public JArray Data { get; private set; }
        public string StreamName { get; private set; }
    }
    public class ErrorMsgEventArgs
    {
        public ErrorMsgEventArgs(int code, string messageError)
        {
            Code = code;
            MessageError = messageError;
        }
        public int Code { get; set; }
        public string MessageError { get; set; }
    }

    public sealed class CortexClient
    {
        const string Url = "wss://localhost:6868";
        private string m_CurrentMessage = string.Empty;
        private Dictionary<int, string> _methodForRequestId;

        private WebSocket _wSC; // Websocket Client
        private int _nextRequestId; // Unique id for each request
        private bool _isWSConnected;
        //Events
        private AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        private AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        private AutoResetEvent m_CloseEvent = new AutoResetEvent(false);

        public event EventHandler<bool> OnConnected;
        public event EventHandler<ErrorMsgEventArgs> OnErrorMsgReceived;
        public event EventHandler<StreamDataEventArgs> OnStreamDataReceived;
        public event EventHandler<List<Headset>> OnQueryHeadset;
        public event EventHandler<string> OnHeadsetConnected;
        public event EventHandler<bool> OnHeadsetDisConnected;
        public event EventHandler<bool> OnHasAccessRight;
        public event EventHandler<bool> OnRequestAccessDone;
        public event EventHandler<bool> OnAccessRightGranted;
        public event EventHandler<string> OnAuthorize;
        public event EventHandler<string> OnGetUserLogin;
        public event EventHandler<bool> OnEULAAccepted;
        public event EventHandler<string> OnUserLogin;
        public event EventHandler<string> OnUserLogout;
        public event EventHandler<SessionEventArgs> OnCreateSession;
        public event EventHandler<SessionEventArgs> OnUpdateSession;
        public event EventHandler<MultipleResultEventArgs> OnSubscribeData;
        public event EventHandler<MultipleResultEventArgs> OnUnSubscribeData;
        public event EventHandler<Record> OnCreateRecord;
        public event EventHandler<Record> OnStopRecord;
        public event EventHandler<Record> OnUpdateRecord;
        public event EventHandler<List<Record>> OnQueryRecords;
        public event EventHandler<MultipleResultEventArgs> OnDeleteRecords;
        public event EventHandler<JObject> OnInjectMarker;
        public event EventHandler<JObject> OnUpdateMarker;
        public event EventHandler<JObject> OnGetDetectionInfo;
        public event EventHandler<string> OnGetCurrentProfile;
        public event EventHandler<string> OnCreateProfile;
        public event EventHandler<string> OnLoadProfile;
        public event EventHandler<string> OnSaveProfile;
        public event EventHandler<bool> OnUnloadProfile;
        public event EventHandler<string> OnDeleteProfile;
        public event EventHandler<string> OnRenameProfile;
        public event EventHandler<JArray> OnQueryProfile;
        public event EventHandler<double> OnGetTrainingTime;
        public event EventHandler<JObject> OnTraining;

        // Constructor
        static CortexClient()
        {

        }
        private CortexClient()
        {
            _nextRequestId = 1;
            _wSC = new WebSocket(Url);
            _methodForRequestId = new Dictionary<int, string>();
            _wSC.Opened += new EventHandler(WebSocketClient_Opened);

            _wSC.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(WebSocketClient_Error);
           
            _wSC.Closed += new EventHandler(WebSocketClient_Closed);

            _wSC.MessageReceived += new EventHandler<MessageReceivedEventArgs>(WebSocketClient_MessageReceived);
    }
        // Properties
        public static CortexClient Instance { get; } = new CortexClient();

        public bool IsWSConnected
        {
            get
            {
                return _isWSConnected;
            }
        }

        // Build a request message
        private void SendTextMessage(JObject param, string method, bool hasParam = true)
        {
            JObject request = new JObject(
            new JProperty("jsonrpc", "2.0"),
            new JProperty("id", _nextRequestId),
            new JProperty("method", method));

            if (hasParam)
            {
                request.Add("params", param);
            }
            Console.WriteLine("Send " + method);
            //Console.WriteLine(request.ToString());

            // send the json message
            _wSC.Send(request.ToString());

            _methodForRequestId.Add(_nextRequestId, method);
            _nextRequestId++;
        }
        // Handle receieved message 
        private void WebSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            m_CurrentMessage = e.Message;
            m_MessageReceiveEvent.Set();
            //Console.WriteLine("Received: " + e.Message);

            JObject response = JObject.Parse(e.Message);

            if (response["id"] != null)
            {
                int id = (int)response["id"];
                string method = _methodForRequestId[id];
                _methodForRequestId.Remove(id);
                
                if (response["error"] != null)
                {
                    JObject error = (JObject)response["error"];
                    int code = (int)error["code"];
                    string messageError = (string)error["message"];
                    Console.WriteLine("Received: " + messageError);
                    //Send Error message event
                    OnErrorMsgReceived(this, new ErrorMsgEventArgs(code, messageError));
                }
                else
                {
                    // handle response
                    JToken data = response["result"];
                    HandleResponse(method, data);
                }
            }
            else if (response["sid"] != null)
            {
                string sid = (string)response["sid"];
                double time = 0;
                if (response["time"] != null)
                    time = (double)response["time"];

                foreach (JProperty property in response.Properties())
                {
                    //Console.WriteLine(property.Name + " - " + property.Value);
                    if (property.Name != "sid" &&
                        property.Name != "time")
                    {
                        OnStreamDataReceived(this, new StreamDataEventArgs(sid, (JArray)property.Value, time, property.Name));
                    }
                }
            }
            else if (response["warning"] != null)
            {
                JObject warning = (JObject)response["warning"];
                string messageWarning = "";
                int code = -1;
                if (warning["code"] != null)
                {
                    code = (int)warning["code"];
                }
                if (warning["message"].Type == JTokenType.String)
                {
                    messageWarning = warning["message"].ToString();
                }
                else if (warning["message"].Type == JTokenType.Object)
                {
                    Console.WriteLine("Received Warning Object");
                }
                HandleWarning(code, messageWarning);
            }
        }
        // handle Response
        private void HandleResponse(string method, JToken data)
        {
            Console.WriteLine("handleResponse: " + method);
            if (method == "queryHeadsets")
            {
                List<Headset> headsetLists = new List<Headset>();
                foreach (JObject item in data)
                {
                    headsetLists.Add(new Headset(item));
                }
                OnQueryHeadset(this, headsetLists);

            }
            else if (method == "controlDevice")
            {
                string command = (string)data["command"];
                if (command == "connect")
                {
                    string message = (string)data["message"];
                    string headsetId = "";

                    Console.WriteLine("ConnectHeadset " + message);
                    if (message.Contains("Start connecting to device"))
                    {
                        //"Start connecting to device " + headsetId
                        headsetId = message.Substring(27);
                    }
                    else if (message.Contains("The device"))
                    {
                        //"The device " + headsetId + " has been connected or is connecting";
                        string tmp = message.Replace(" has been connected or is connecting", "");
                        headsetId = tmp.Substring(11);
                    }
                    OnHeadsetConnected(this, headsetId);
                }
                else if (command == "disconnect")
                {
                    OnHeadsetDisConnected(this, true);
                }
            }
            else if (method == "getUserLogin")
            {
                JArray users = (JArray)data;
                string username = "";
                if (users.Count > 0)
                {
                    foreach (JObject user in users)
                    {
                        if (user["currentOSUId"].ToString() == user["loggedInOSUId"].ToString())
                        {
                            username = user["username"].ToString();
                        }
                    }
                }
                OnGetUserLogin(this, username);
            }
            else if (method == "hasAccessRight")
            {
                bool hasAccessRight = (bool)data["accessGranted"];
                OnHasAccessRight(this, hasAccessRight);
            }
            else if (method == "requestAccess")
            {
                bool hasAccessRight = (bool)data["accessGranted"];
                OnRequestAccessDone(this, hasAccessRight);
            }
            else if (method == "authorize")
            {
                string token = (string)data["cortexToken"];
                bool eulaAccepted = true;
                if (data["warning"] != null)
                {
                    JObject warning = (JObject)data["warning"];
                    eulaAccepted = !((int)warning["code"] == WarningCode.UserNotAcceptLicense);
                    token = "";
                }
                OnAuthorize(this, token);
            }
            else if (method == "createSession")
            {
                string sessionId = (string)data["id"];
                string status = (string)data["status"];
                string appId = (string)data["appId"];
                OnCreateSession(this, new SessionEventArgs(sessionId, status, appId));
            }
            else if (method == "updateSession")
            {
                string sessionId = (string)data["id"];
                string status = (string)data["status"];
                string appId = (string)data["appId"];
                OnUpdateSession(this, new SessionEventArgs(sessionId, status, appId));
            }
            else if (method == "createRecord")
            {
                Record record = new Record((JObject)data["record"]);
                OnCreateRecord(this, record);
            }
            else if (method == "stopRecord")
            {
                Record record = new Record((JObject)data["record"]);
                OnStopRecord(this, record);
            }
            else if (method == "updateRecord")
            {
                Record record = new Record((JObject)data);
                OnUpdateRecord(this, record);
            }
            else if (method == "queryRecords")
            {
                int count = (int)data["count"];
                JArray records = (JArray)data["records"];
                List<Record> recordLists = new List<Record>();
                foreach(JObject ele in records)
                {
                    recordLists.Add(new Record(ele));
                }
                OnQueryRecords(this, recordLists);
            }
            else if (method == "deleteRecord")
            {
                JArray successList = (JArray)data["success"];
                JArray failList = (JArray)data["failure"];
                OnDeleteRecords(this, new MultipleResultEventArgs(successList, failList));
            }
            else if (method == "unsubscribe")
            {
                JArray successList = (JArray)data["success"];
                JArray failList = (JArray)data["failure"];
                OnUnSubscribeData(this, new MultipleResultEventArgs(successList, failList));
            }
            else if (method == "subscribe")
            {
                JArray successList = (JArray)data["success"];
                JArray failList = (JArray)data["failure"];
                OnSubscribeData(this, new MultipleResultEventArgs(successList, failList));

            }
            else if (method == "injectMarker")
            {
                JObject marker = (JObject)data["marker"];
                OnInjectMarker(this, marker);
            }
            else if (method == "updateMarker")
            {
                JObject marker = (JObject)data["marker"];
                OnUpdateMarker(this, marker);
            }
            else if (method == "getDetectionInfo")
            {
                OnGetDetectionInfo(this, (JObject)data);
            }
            else if (method == "getCurrentProfile")
            {
                if (data["name"] == null)
                    OnGetCurrentProfile(this, "");
                else
                    OnGetCurrentProfile(this, (string)data["name"]);
            }
            else if (method == "setupProfile")
            {
                string action = (string)data["action"];
                string profileName = (string)data["name"];
                if (action == "create")
                {
                    OnCreateProfile(this, profileName);
                }
                else if (action == "load")
                {
                    OnLoadProfile(this, profileName);
                }
                else if (action == "save")
                {
                    OnSaveProfile(this, profileName);
                }
                else if (action == "unload")
                {
                    OnUnloadProfile(this, true);
                }
                else if (action == "rename")
                {
                    OnRenameProfile(this, profileName);
                }
                else if (action == "delete")
                {
                    OnDeleteProfile(this, profileName);
                }
            }
            else if (method == "queryProfile")
            {
                OnQueryProfile(this, (JArray)data);
            }
            else if (method == "training")
            {
                OnTraining(this, (JObject)data);
            }
            else if (method == "getTrainingTime")
            {
                OnGetTrainingTime(this, (double)data["time"]);
            }

        }

        // handle warning response
        private void HandleWarning(int code, string message)
        {
            Console.WriteLine("handleWarning: " + code + " message: " + message);
            if (code == WarningCode.AccessRightGranted)
            {
                // granted access right
                OnAccessRightGranted(this, true);
            }
            else if (code == WarningCode.AccessRightRejected)
            {
                OnAccessRightGranted(this, false);
            }
            else if (code == WarningCode.EULAAccepted)
            {
                OnEULAAccepted(this, true);
            }
            else if (code == WarningCode.UserLogin)
            {
                OnUserLogin(this, message);
            }
            else if (code == WarningCode.UserLogout)
            {
                OnUserLogout(this, message);
            }

        }
        private void WebSocketClient_Closed(object sender, EventArgs e)
        {
            m_CloseEvent.Set();
        }

        private void WebSocketClient_Opened(object sender, EventArgs e)
        {
            m_OpenedEvent.Set();
        }

        private void WebSocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.GetType() + ":" + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);

            if (e.Exception.InnerException != null)
            {
                Console.WriteLine(e.Exception.InnerException.GetType());
            }
        }


        //Open socket
        public void Open()
        {
            //Open websocket
            _wSC.Open();

            if (!m_OpenedEvent.WaitOne(10000))
            {
                Console.WriteLine("Failed to Opened session ontime");
                //Assert.Fail("Failed to Opened session ontime");
            }
            if (_wSC.State == WebSocketState.Open)
            {
                _isWSConnected = true;
                OnConnected(this, true);
            }
            else
            {
                _isWSConnected = false;
                OnConnected(this, false);
            }
        }

        // Has Access Right
        public void HasAccessRights()
        {
            JObject param = new JObject(
                    new JProperty("clientId", Config.AppClientId),
                    new JProperty("clientSecret", Config.AppClientSecret)
                );
            SendTextMessage(param, "hasAccessRight", true);
        }
        // Request Access
        public void RequestAccess()
        {
            JObject param = new JObject(
                    new JProperty("clientId", Config.AppClientId),
                    new JProperty("clientSecret", Config.AppClientSecret)
                );
            SendTextMessage(param, "requestAccess", true);
        }
        // Authorize
        public void Authorize(string licenseID, int debitNumber)
        {
            JObject param = new JObject();
            param.Add("clientId", Config.AppClientId);
            param.Add("clientSecret", Config.AppClientSecret);
            if (!string.IsNullOrEmpty(licenseID))
            {
                param.Add("license", licenseID);
            }
            param.Add("debit", debitNumber);
            SendTextMessage(param, "authorize", true);
        }

        // GetUserLogin
        public void GetUserLogin()
        {
            JObject param = new JObject();
            SendTextMessage(param, "getUserLogin", false);
        }
        // GenerateNewToken
        public void GenerateNewToken(string currentAccessToken)
        {
            JObject param = new JObject(
                    new JProperty("clientId", Config.AppClientId),
                    new JProperty("clientSecret", Config.AppClientSecret),
                    new JProperty("token", currentAccessToken)
                );
            SendTextMessage(param, "generateNewToken", true);
        }

        // QueryHeadset
        public void QueryHeadsets(string headsetId)
        {
            JObject param = new JObject();
            if (!String.IsNullOrEmpty(headsetId))
            {
                param.Add("id", headsetId);
            }
            SendTextMessage(param, "queryHeadsets", false);
        }

        // controlDevice
        // required params: command
        // command = {"connect", "disconnect", "refresh"}
        // mappings is required if connect to epoc flex
        public void ControlDevice(string command, string headsetId, JObject mappings)
        {
            JObject param = new JObject();
            param.Add("command", command);
            if (!String.IsNullOrEmpty(headsetId))
            {
                param.Add("headset", headsetId);
            }
            if (mappings.Count > 0)
            {
                param.Add("mappings", mappings);
            }
            SendTextMessage(param, "controlDevice", true);
        }


        // CreateSession
        // Required params: cortexToken, status
        public void CreateSession(string cortexToken, string headsetId, string status)
        {
            JObject param = new JObject();
            if (!String.IsNullOrEmpty(headsetId))
            {
                param.Add("headset", headsetId);
            }
            param.Add("cortexToken", cortexToken);
            param.Add("status", status);
            SendTextMessage(param, "createSession", true);
        }

        // UpdateSession
        // Required params: session, status, cortexToken
        public void UpdateSession(string cortexToken, string sessionId, string status)
        {
            JObject param = new JObject();
            param.Add("session", sessionId);
            param.Add("cortexToken", cortexToken);
            param.Add("status", status);
            SendTextMessage(param, "updateSession", true);
        }

        // CreateRecord
        // Required params: session, title, cortexToken
        public void CreateRecord(string cortexToken, string sessionId, string title,
                                 JToken description = null, JToken subjectName = null, JToken tags= null)
        {
            JObject param = new JObject();
            param.Add("session", sessionId);
            param.Add("cortexToken", cortexToken);
            param.Add("title", title);
            if (description != null)
            {
                param.Add("description", description);
            }
            if (subjectName != null)
            {
                param.Add("subjectName", subjectName);
            }
            if (tags != null)
            {
                param.Add("tags", tags);
            }
            SendTextMessage(param, "createRecord", true);
        }

        // StopRecord
        // Required params: session, cortexToken
        public void StopRecord(string cortexToken, string sessionId)
        {
            JObject param = new JObject();
            param.Add("session", sessionId);
            param.Add("cortexToken", cortexToken);
            SendTextMessage(param, "stopRecord", true);
        }

        // UpdateRecord
        // Required params: session, record
        public void UpdateRecord(string cortexToken, string recordId, string description = null, List<string> tags = null)
        {
            JObject param = new JObject();
            param.Add("record", recordId);
            param.Add("cortexToken", cortexToken);
            if (description != null)
            {
                param.Add("description", description);
            }
            if (tags != null)
            {
                param.Add("tags", JArray.FromObject(tags));
            }
            SendTextMessage(param, "updateRecord", true);
        }

        // QueryRecord
        // Required params: cortexToken, query
        public void QueryRecord(string cortexToken, JObject query, JArray orderBy = null, JToken offset = null, JToken limit = null)
        {
            JObject param = new JObject();
            param.Add("query", query);
            param.Add("cortexToken", cortexToken);
            if (orderBy != null)
            {
                param.Add("orderBy", orderBy);
            }
            if (offset != null)
            {
                param.Add("offset", (long)offset);
            }
            if (limit != null)
            {
                param.Add("limit", (long)limit);
            }
            SendTextMessage(param, "queryRecords", true);
        }

        // DeleteRecord
        // Required params: session, records
        public void DeleteRecord(string cortexToken, List<string> records)
        {
            JObject param = new JObject();
            param.Add("records", JArray.FromObject(records));
            param.Add("cortexToken", cortexToken);
            SendTextMessage(param, "deleteRecord", true);
        }

        // InjectMarker
        // Required params: session, cortexToken, label, value, time
        public void InjectMarker(string cortexToken, string sessionId, string label, JToken value, double time, string port = null)
        {
            JObject param = new JObject();
            param.Add("session", sessionId);
            param.Add("cortexToken", cortexToken);
            param.Add("label", label);
            param.Add("time", time);
            param.Add("value", value);
            if (port != null)
                param.Add("port", port);
            SendTextMessage(param, "injectMarker", true);
        }

        // UpdateMarker
        // Required params: session, cortexToken, label, value, time
        public void UpdateMarker(string cortexToken, string sessionId, string markerId, double time)
        {
            JObject param = new JObject();
            param.Add("session", sessionId);
            param.Add("cortexToken", cortexToken);
            param.Add("markerId", markerId);
            param.Add("time", time);
            SendTextMessage(param, "updateMarker", true);
        }

        // Subscribe Data
        // Required params: session, cortexToken, streams
        public void Subscribe(string cortexToken, string sessionId, List<string> streams)
        {
            JObject param = new JObject();
            param.Add("session", sessionId);
            param.Add("cortexToken", cortexToken);
            param.Add("streams", JToken.FromObject(streams));
            SendTextMessage(param, "subscribe", true);
        }

        // UnSubscribe Data
        // Required params: session, cortexToken, streams
        public void UnSubscribe(string cortexToken, string sessionId, List<string> streams)
        {
            JObject param = new JObject();
            param.Add("session", sessionId);
            param.Add("cortexToken", cortexToken);
            param.Add("streams", JToken.FromObject(streams));
            SendTextMessage(param, "unsubscribe", true);
        }

        // Training - Profile
        // getDetectionInfo
        // Required params: detection
        public void GetDetectionInfo(string detection)
        {
            JObject param = new JObject();
            param.Add("detection", detection);
            SendTextMessage(param, "getDetectionInfo", true);
        }
        // getCurrentProfile
        // Required params: cortexToken, headset
        public void GetCurrentProfile(string cortexToken, string headsetId)
        {
            JObject param = new JObject();
            param.Add("cortexToken", cortexToken);
            param.Add("headset", headsetId);
            SendTextMessage(param, "getCurrentProfile", true);
        }
        // setupProfile
        // Required params: cortexToken, profile, status
        public void SetupProfile(string cortexToken, string profile, string status, string headsetId = null, string newProfileName = null)
        {
            JObject param = new JObject();
            param.Add("profile", profile);
            param.Add("cortexToken", cortexToken);
            param.Add("status", status);
            if (headsetId != null)
            {
                param.Add("headset", headsetId);
            }
            if (newProfileName != null)
            {
                param.Add("newProfileName", newProfileName);
            }
            SendTextMessage(param, "setupProfile", true);
        }
        // queryProfile
        // Required params: cortexToken
        public void QueryProfile(string cortexToken)
        {
            JObject param = new JObject();
            param.Add("cortexToken", cortexToken);
            SendTextMessage(param, "queryProfile", true);
        }
        // getTrainingTime
        // Required params: cortexToken
        public void GetTrainingTime(string cortexToken, string detection, string sessionId)
        {
            JObject param = new JObject();
            param.Add("cortexToken", cortexToken);
            param.Add("detection", detection);
            param.Add("session", sessionId);
            SendTextMessage(param, "getTrainingTime", true);
        }
        // training
        // Required params: cortexToken, profile, status
        public void Training(string cortexToken, string sessionId, string status, string detection, string action)
        {
            JObject param = new JObject();
            param.Add("session", sessionId);
            param.Add("cortexToken", cortexToken);
            param.Add("status", status);
            param.Add("detection", detection);
            param.Add("action", action);

            SendTextMessage(param, "training", true);
        }



    }
}
