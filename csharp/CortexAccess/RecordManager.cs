using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CortexAccess
{
    /// <summary>
    /// Reponsible for managing and handling records and markers.
    /// </summary>
    public class RecordManager
    {
        private static Authorizer _authorizer;
        private static HeadsetFinder _headsetFinder;
        private static SessionCreator _sessionCreator;
        private static CortexClient _ctxClient;
        private string _wantedHeadsetId;
        private static List<Record> _recordLists = new List<Record>();
        private static List<string> _markersList = new List<string>();


        private string _currRecordId;
        private string _currMarkerId;
        private string _cortexToken;
        private string _sessionId;

        public static List<Record> RecordLists { get => _recordLists; set => _recordLists = value; }
        public static List<string> MarkersList { get => _markersList; set => _markersList = value; }

        public event EventHandler<JObject> informMarkerResult;
        public event EventHandler<bool> sessionCreateOk;

        // Constructor
        public RecordManager ()
        {
            _authorizer = new Authorizer();
            _headsetFinder = new HeadsetFinder();
            _sessionCreator = new SessionCreator();
            _ctxClient = CortexClient.Instance;

            _cortexToken = "";
            _sessionId = "";

            _ctxClient.OnErrorMsgReceived += MessageErrorRecieved;
            _ctxClient.SessionClosedNotify += SessionClosedOK;
            _ctxClient.OnCreateRecord += OnCreateRecordOK;
            _ctxClient.OnStopRecord += OnStopRecordOK;
            _ctxClient.OnQueryRecords += QueryRecordsOK;
            _ctxClient.OnDeleteRecords += DeleteRecordOK;
            _ctxClient.OnUpdateRecord += RecordUpdatedOK;
            _ctxClient.OnInjectMarker += OnInjectMarkerOK;
            _ctxClient.OnUpdateMarker += OnUpdateMarkerOK;
            _ctxClient.OnErrorMsgReceived += MessageErrorRecieved;

            _authorizer.OnAuthorized += AuthorizedOK;
            _headsetFinder.OnHeadsetConnected += HeadsetConnectedOK;
            _sessionCreator.OnSessionCreated += SessionCreatedOk;
            _sessionCreator.OnSessionClosed += SessionClosedOK;
        }

        private void RecordUpdatedOK(object sender, Record record)
        {
            Console.WriteLine("RecordUpdatedOK");
            record.PrintOut();
        }

        private void DeleteRecordOK(object sender, MultipleResultEventArgs e)
        {
            Console.WriteLine("DeleteRecordOK");
            Console.WriteLine("Successes: " + e.SuccessList);
            Console.WriteLine("Failures: " + e.FailList);
        }

        private void AuthorizedOK(object sender, string cortexToken)
        {
            if (!String.IsNullOrEmpty(cortexToken))
            {
                _cortexToken = cortexToken;
                if (!_headsetFinder.IsHeadsetScanning)
                {
                    // Start scanning headset. It will call one time whole program.
                    // If you want re-scan, please check IsHeadsetScanning and call ScanHeadsets() again
                    _headsetFinder.ScanHeadsets();
                }
                // find headset
                _headsetFinder.FindHeadset(_wantedHeadsetId);
            }
        }

        private void HeadsetConnectedOK(object sender, string headsetId)
        {
            Console.WriteLine("HeadsetConnectedOK " + headsetId);
            // Wait a moment before creating session
            System.Threading.Thread.Sleep(1500);
            // CreateSession
            _sessionCreator.Create(_cortexToken, headsetId, true);
        }

        private void OnStopRecordOK(object sender, Record record)
        {
             Console.WriteLine("RecordManager: OnStopRecordOK recordId: " + record.Uuid + 
                                   " at: " + record.EndDateTime);
            _currRecordId = "";
        }

        private static void QueryRecordsOK(object sender, List<Record> records)
        {
            _recordLists = records;
            Console.WriteLine("QueryRecordsOK");
            foreach (Record ele in records)
            {
                ele.PrintOut();
            }
        }

        private void SessionClosedOK(object sender, string sessionId)
        {
            if (sessionId == _sessionId)
            {
                Console.WriteLine("The Session " + sessionId + " has closed successfully.");
                _sessionId = "";
                _headsetFinder.HasHeadsetConnected = false;
            }
        }

        private void SessionCreatedOk(object sender, string sessionId)
        {
            _sessionId = sessionId;
            sessionCreateOk(this, true);
        }

        private void OnCreateRecordOK(object sender, Record record)
        {
            _currRecordId = record.Uuid;
            Console.WriteLine("Record " + record.Uuid + " is created successfully.");
        }
        private void OnInjectMarkerOK(object sender, JObject markerObj)
        {
            Console.WriteLine("Marker is injected successfully. Marker " + markerObj);
            _currMarkerId = markerObj["uuid"].ToString();
            _markersList.Add(_currMarkerId);
        }
        private void OnUpdateMarkerOK(object sender, JObject markerObj)
        {
            Console.WriteLine("Marker is updated successfully. Marker " + markerObj);
        }

        private void MessageErrorRecieved(object sender, ErrorMsgEventArgs errorInfo)
        {
            
            string message  = errorInfo.MessageError;
            int errorCode   = errorInfo.Code;
             Console.WriteLine("MessageErrorRecieved :code " + errorCode
                                   + " message " + message);
            //string errorMsg = method +" gets error: "+ message;
            //if (method == "injectMarker" || method == "updateMarker") {
            //    informMarkerResult(this, errorMsg);
            //}
            //else if (method == "createRecord" || method == "stopRecord") {
            //    informRecordResult(this, errorMsg);
            //}
        }


        /// <summary>
        /// start a record manager with a license ID and a wanted headset ID.
        /// </summary>
        public void Start( string wantedHeadsetId = "", string licenseID = "")
        {
            _wantedHeadsetId = wantedHeadsetId;
            _authorizer.Start(licenseID);
        }

        /// <summary>
        /// stop a record manager
        /// </summary>
        public void Stop()
        {
            _sessionCreator.CloseSession();
            _recordLists.Clear();
            _currRecordId = "";
        }

        /// <summary>
        /// Create a new record.
        /// </summary>
        public void StartRecord(string title, JToken description = null,
                                JToken subjectName = null, List<string> tags= null)
        {
            // start record
            _sessionCreator.StartRecord(_authorizer.CortexToken, title, description, subjectName, tags);
        }

        /// <summary>
        /// Stop a record that was previously started by StartRecord
        /// </summary>
        public void StopRecord()
        {
            _sessionCreator.StopRecord(_authorizer.CortexToken);
        }
        /// <summary>
        /// Update for record has uuid is recordId
        /// </summary>
        public void UpdateRecord(string recordId , string description = null, List<string> tags = null)
        {
            _sessionCreator.UpdateRecord(_authorizer.CortexToken, recordId, description, tags);
        }
        /// <summary>
        /// Query records
        /// </summary>
        public void QueryRecords(JObject queryObj, JArray orderBy, int limit, int offset)
        {
            queryObj.Add("applicationId", _sessionCreator.ApplicationId);

            _ctxClient.QueryRecord(_authorizer.CortexToken, queryObj, orderBy, offset, limit);
        }

        public void DeleteRecords(List<string> records)
        {
            _ctxClient.DeleteRecord(_authorizer.CortexToken, records);
        }

        /// <summary>
        /// inject marker
        /// </summary>
        public void InjectMarker(string markerLabel, string markerValue)
        {
            string cortexToken = _authorizer.CortexToken;
            string sessionId = _sessionCreator.SessionId;

            // inject marker
            _ctxClient.InjectMarker(cortexToken, sessionId, markerLabel, markerValue, Utils.GetEpochTimeNow());
        }

        /// <summary>
        /// update marker to set the end date time of a marker, turning an "instance" marker into an "interval" marker
        /// </summary>
        public void UpdateMarker()
        {
            string cortexToken = _authorizer.CortexToken;
            string sessionId = _sessionCreator.SessionId;

            // update marker
            _ctxClient.UpdateMarker(cortexToken, sessionId, _currMarkerId, Utils.GetEpochTimeNow());
        }

    }
}
