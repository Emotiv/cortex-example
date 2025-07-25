﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CortexAccess
{
    /// <summary>
    /// Training is a helper class for handling profile management and training workflows with Emotiv Cortex.
    /// It manages authorization, headset selection, detection type, profile creation/loading/unloading, and training actions.
    /// Call Start() to begin authorization and set detection/headset. After authorization, it will get detection info and query available profiles.
    /// </summary>
    public class Training
    {
        private CortexClient _ctxClient;
        private string _profileName; // must not existed
        private string _cortexToken;
        private string _sessionId;
        private string _detection;
        private bool _isProfileLoaded;
        private string _headsetId; // Id of current headset
        private List<string> _availActions; // available actions

        private HeadsetFinder _headsetFinder;
        private Authorizer _authorizer;
        private SessionCreator _sessionCreator;
        private List<string> _profileLists;
        private string _wantedHeadsetId;

        // event
        public event EventHandler<string> OnProfileLoaded;
        public event EventHandler<bool> OnUnProfileLoaded;
        public event EventHandler<bool> OnTrainingSucceeded;
        public event EventHandler<bool> OnReadyForTraning;

        //Constructor
        public Training()
        {
            _authorizer = new Authorizer();
            _headsetFinder = new HeadsetFinder();
            _sessionCreator = new SessionCreator();
            _cortexToken = "";
            _sessionId = "";
            _isProfileLoaded = false;
            _availActions = new List<string>();
            _profileLists = new List<string>();

            // Event register
            _ctxClient = CortexClient.Instance;
            _ctxClient.OnErrorMsgReceived += MessageErrorRecieved;
            _ctxClient.OnGetDetectionInfo += GetDetectionOk;
            _ctxClient.OnStreamDataReceived += StreamDataReceived;
            _ctxClient.OnSubscribeData += SubscribeDataOK;
            _ctxClient.OnCreateProfile += ProfileCreatedOK;
            _ctxClient.OnLoadProfile += ProfileLoadedOK;
            _ctxClient.OnSaveProfile += ProfileSavedOK;
            _ctxClient.OnUnloadProfile += ProfileUnloadedOK;
            _ctxClient.OnTraining += TrainingOK;
            _ctxClient.OnQueryProfile += QueryProfileOK;
            _ctxClient.SessionClosedNotify += SessionClosedOK;

            _authorizer.OnAuthorized += AuthorizedOK;
            _headsetFinder.OnHeadsetConnected += HeadsetConnectedOK;
            _sessionCreator.OnSessionCreated += SessionCreatedOk;
            _sessionCreator.OnSessionClosed += SessionClosedOK;
        }

        private void SessionClosedOK(object sender, string sessionId)
        {
            Console.WriteLine("Session " + sessionId + " has closed");
            if (sessionId == _sessionId)
            {
                Console.WriteLine("The Session " + sessionId + " has closed successfully.");
                _sessionId = "";
                _headsetFinder.HasHeadsetConnected = false;
            }
        }

        private void ProfileUnloadedOK(object sender, bool e)
        {
            OnUnProfileLoaded(this, true);
        }

        private void QueryProfileOK(object sender, JArray profiles)
        {
            Console.WriteLine("QueryProfileOK" + profiles);
            foreach (JObject ele in profiles)
            {
                string name = (string)ele["name"];
                _profileLists.Add(name);
            }
            if (!_headsetFinder.IsHeadsetScanning)
            {
                // Start scanning headset. It will call one time whole program.
                // If you want re-scan, please check IsHeadsetScanning and call ScanHeadsets() again
                _headsetFinder.ScanHeadsets();
            }
            // find headset
            _headsetFinder.FindHeadset(_wantedHeadsetId);
        }

        private void ProfileSavedOK(object sender, string profileName)
        {
            Console.WriteLine("The profile " + profileName + " is saved successfully");
        }

        private void StreamDataReceived(object sender, StreamDataEventArgs e)
        {

            if (e.StreamName == "sys")
            {
                List<string> data = e.Data.ToObject<List<string>>();
                JArray dataEvent = e.Data;
                string detection = dataEvent[0].ToString();
                string eventType = dataEvent[1].ToString();
                if (detection == "mentalCommand")
                {
                    if (eventType == "MC_Started")
                    {
                        Console.WriteLine("Start training...");
                    }
                    else if (eventType == "MC_Succeeded")
                    {
                        OnTrainingSucceeded(this, true);
                    }
                    else if (eventType == "MC_Completed" ||
                             eventType == "MC_Rejected" ||
                             eventType == "MC_DataErased" ||
                             eventType == "MC_Reset")
                    {
                        _ctxClient.SetupProfile(_cortexToken, _profileName, "save", _headsetId);
                    }

                }
                else if (detection == "facialExpression")
                {
                    if (eventType == "FE_Started")
                    {
                        Console.WriteLine("Start training...");
                    }
                    else if (eventType == "FE_Succeeded")
                    {
                        OnTrainingSucceeded(this, true);
                    }
                    else if (eventType == "FE_Completed" ||
                             eventType == "FE_Rejected" ||
                             eventType == "FE_DataErased" ||
                             eventType == "FE_Reset")
                    {
                        _ctxClient.SetupProfile(_cortexToken, _profileName, "save", _headsetId);
                    }

                }
            }
        }

        private void TrainingOK(object sender, JObject e)
        {
            Console.WriteLine("TrainingOK: " + e);
        }

        private void ProfileLoadedOK(object sender, string profileName)
        {
            _profileName = profileName;
            _isProfileLoaded = true;
            OnProfileLoaded(this, profileName);
        }

        private void ProfileCreatedOK(object sender, string profileName)
        {
            Console.WriteLine("The profile " + profileName + " is created successfully. Please load the profile to use");
            if (!_profileLists.Contains(profileName))
                _profileLists.Add(profileName);
        }

        private void SubscribeDataOK(object sender, MultipleResultEventArgs e)
        {
            bool found = false;
            Dictionary<string, JArray> header = new Dictionary<string, JArray>();
            foreach (JObject ele in e.SuccessList)
            {
                string streamName = (string)ele["streamName"];
                if (streamName == "sys")
                {
                    found = true;
                    Console.WriteLine(ele);
                }
            }
            if (found)
            {
                // Ready for training
                OnReadyForTraning(this, true);
            }
            else
            {
                Console.WriteLine("Can not subscribe training event");
            }
        }

        private void GetDetectionOk(object sender, JObject rsp)
        {
            Console.WriteLine("GetDetectionInfoOK: " + rsp);

            _availActions = rsp["actions"].ToObject<List<string>>();

            // query profiles
            _ctxClient.QueryProfile(_cortexToken);
        }

        private void SessionCreatedOk(object sender, string sessionId)
        {
            // subscribe
            _sessionId = sessionId;
            // Subscribe sys
            List<string> stream = new List<string>() { "sys" };
            _ctxClient.Subscribe(_cortexToken, _sessionId, stream);
        }

        private void HeadsetConnectedOK(object sender, string headsetId)
        {
            //Console.WriteLine("HeadsetConnectedOK " + headsetId);
            _headsetId = headsetId;
            System.Threading.Thread.Sleep(1500); // wait a moment before creating session to make sure headset ready.
            // CreateSession
            _sessionCreator.Create(_cortexToken, headsetId);
        }

        private void AuthorizedOK(object sender, string cortexToken)
        {
            if (!String.IsNullOrEmpty(cortexToken))
            {
                _cortexToken = cortexToken;
                // Get detection info
                _ctxClient.GetDetectionInfo(_detection);
            }
        }

        private void MessageErrorRecieved(object sender, ErrorMsgEventArgs e)
        {
            Console.WriteLine("MessageErrorRecieved :code " + e.Code + " message " + e.MessageError);
        }

        /// <summary>
        /// Start authorization and set detection type and desired headset.
        /// After authorization, will get detection info and query available profiles.
        /// </summary>
        /// <param name="detection">Detection type: "mentalCommand" or "facialExpression"</param>
        /// <param name="wantedHeadsetId">Optional headset ID to use</param>
        public void Start(string detection, string wantedHeadsetId = "")
        {
            if (detection == "mentalCommand" ||
                detection == "facialExpression")
            {
                _wantedHeadsetId = wantedHeadsetId;
                _detection = detection;
                _authorizer.Start();
            }
            else
            {
                Console.WriteLine("Unsupported detection. Only mentalCommand or facialExpression supported.");
            }
        }

        /// <summary>
        /// Start training for a specific action and status (e.g., "start", "accept", "reject").
        /// The profile must be loaded and the action must be available in detection info.
        /// </summary>
        /// <param name="action">Action to train (e.g., "neutral", "push", "pull")</param>
        /// <param name="status">Training status ("start", "accept", "reject")</param>
        public void DoTraining(string action, string status)
        {
            Console.WriteLine(status + " " + action + " training.");
            if (!_isProfileLoaded)
            {
                Console.WriteLine("The profile for training have not still be loaded. Please wait");
                return;
            }
            if (_availActions.Contains(action))
            {
                //Do training
                _ctxClient.Training(_cortexToken, _sessionId, status, _detection, action);
            }
            else
            {
                Console.WriteLine("Can not train the action " + action + ". Available actions: " + _availActions);
            }
        }

        /// <summary>
        /// Create a new training profile for the current headset.
        /// </summary>
        /// <param name="profileName">Profile name (must not already exist)</param>
        public void CreateProfile(string profileName)
        {
            if (_profileLists.Contains(profileName))
            {
                Console.WriteLine("The profile has name " + profileName + " has existed. Please use other name");
                return;
            }
            _ctxClient.SetupProfile(_cortexToken, profileName, "create", _headsetId);
        }

        /// <summary>
        /// Load an existing training profile for the current headset.
        /// </summary>
        /// <param name="profileName">Profile name to load</param>
        public void LoadProfile(string profileName)
        {
            if (_profileLists.Contains(profileName))
                _ctxClient.SetupProfile(_cortexToken, profileName, "load", _headsetId);
            else
                Console.WriteLine("The profile can not be loaded. The name " + profileName + " has not existed.");
        }

        /// <summary>
        /// Unload a training profile for the current headset.
        /// </summary>
        /// <param name="profileName">Profile name to unload</param>
        public void UnLoadProfile(string profileName)
        {
            if (_profileLists.Contains(profileName))
                _ctxClient.SetupProfile(_cortexToken, profileName, "unload", _headsetId);
            else
                Console.WriteLine("The profile can not be unloaded. The name " + profileName + " has not existed.");
        }

        public void CloseSession()
        {
            _sessionCreator.CloseSession();
        }

    }
}
