using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Timers;

namespace CortexAccess
{
    public class HeadsetFinder
    {
        private CortexClient _ctxClient;
        private string _wantedHeadsetId; // headset id of wanted headset device
        private Timer _aTimer;

        private bool _hasHeadsetConnected;
        private bool _isHeadsetScanning = false;
        private bool _isAutoConnect = true; // set to false if you don't want to connect automatically to a headset

        // Event
        public event EventHandler<string> OnHeadsetConnected;
        public event EventHandler<bool> OnHeadsetDisConnected;

        public bool IsHeadsetScanning { get => _isHeadsetScanning; }
        public bool HasHeadsetConnected { get => _hasHeadsetConnected; set => _hasHeadsetConnected = value; }
        public bool IsAutoConnect { get => _isAutoConnect; set => _isAutoConnect = value; }

        public HeadsetFinder()
        {
            _ctxClient = CortexClient.Instance;
            _wantedHeadsetId = "";
            _hasHeadsetConnected = false;
            _ctxClient.OnQueryHeadset += QueryHeadsetOK;
            _ctxClient.HeadsetConnectNotify += OnHeadsetConnectNotify;
            _ctxClient.HeadsetScanFinished += OnHeadsetScanFinished;
        }

        public void FindHeadset(string wantedHeadsetId = "")
        {
            Console.WriteLine("FindHeadset: hasHeadsetConnected " + _hasHeadsetConnected + " wantedHeadsetId: " + wantedHeadsetId);
            if (!_hasHeadsetConnected)
            {
                _wantedHeadsetId = wantedHeadsetId;
                SetTimer(); // set timer for query headset
                _ctxClient.QueryHeadsets(wantedHeadsetId);
            }
        }

        /// <summary>
        /// ScanHeadsets to trigger scan headsets from Cortex
        /// </summary>
        public void ScanHeadsets()
        {
            Console.WriteLine("Start scanning headset.");
            _isHeadsetScanning = true;
            _ctxClient.ControlDevice("refresh", "", new JObject());
        }

        private void OnHeadsetScanFinished(object sender, string message)
        {
            _isHeadsetScanning = false;
            Console.WriteLine(message);
        }

        private void OnHeadsetConnectNotify(object sender, HeadsetConnectEventArgs e)
        {
            string headsetId = e.HeadsetId;
            Console.WriteLine("OnHeadsetConnectNotify headsetId:" + headsetId + " _wantedHeadsetId:" + _wantedHeadsetId + " isSuccess " + e.IsSuccess);
            if (headsetId == _wantedHeadsetId)
            {
                if (e.IsSuccess)
                {
                    OnHeadsetConnected(this, _wantedHeadsetId);
                    _hasHeadsetConnected = true;
                }
                else
                {
                    _hasHeadsetConnected = false;
                    Console.WriteLine("Connect the headset " + headsetId + " unsuccessfully. Message : " + e.Message);
                }
            }
        }

        private void QueryHeadsetOK(object sender, List<Headset> headsets)
        {
            if (headsets.Count > 0 && !_hasHeadsetConnected)
            {
                Headset _wantedHeadset = new Headset();
                foreach (var headsetItem in headsets)
                {
                    if (!String.IsNullOrEmpty(_wantedHeadsetId) && _wantedHeadsetId == headsetItem.HeadsetID)
                    {
                        _wantedHeadset = headsetItem;
                    }
                }

                if (String.IsNullOrEmpty(_wantedHeadsetId))
                {
                    // set wanted headset is first headset
                    _wantedHeadset = headsets.First<Headset>();
                    _wantedHeadsetId = _wantedHeadset.HeadsetID;
                }

                if (_wantedHeadset.Status == "discovered")
                {
                    // prepare flex mapping if the headset is EPOC Flex
                    JObject flexMappings = new JObject();
                    if (_wantedHeadset.HeadsetID.IndexOf("FLEX", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        // For an Epoc Flex headset, we need a mapping
                        flexMappings = JObject.Parse(Config.FlexMapping);
                    }
                    _ctxClient.ControlDevice("connect", _wantedHeadset.HeadsetID, flexMappings);
                }
                else if (_wantedHeadset.Status == "connected")
                {
                    OnHeadsetConnected(this, _wantedHeadsetId);
                    _hasHeadsetConnected = true;
                }
            }
            else
            {
                Console.WriteLine(" No headset available. Please connect headset to the machine");
            }
        }

        // Create Timer for headset finding
        private void SetTimer()
        {
            // Create a timer with 5 seconds
            _aTimer = new Timer(5000);

            // Hook up the Elapsed event for the timer. 
            _aTimer.Elapsed += OnTimedEvent;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (!_hasHeadsetConnected && _isAutoConnect)
            {
                // Query headset again
                _ctxClient.QueryHeadsets("");
            }
        }
    }
}
