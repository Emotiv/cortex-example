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
        private string _headsetId; // headset id of connected device
        private Timer _aTimer;

        private bool _isFoundHeadset;

        // Event
        public event EventHandler<string> OnHeadsetConnected;
        public event EventHandler<bool> OnHeadsetDisConnected;

        public HeadsetFinder()
        {
            _ctxClient = CortexClient.Instance;
            _headsetId = "";
            _isFoundHeadset = false;
            _ctxClient.OnQueryHeadset += QueryHeadsetOK;
            _ctxClient.OnHeadsetConnected += HeadsetConnectedOK;
            _ctxClient.OnHeadsetDisConnected += HeadsetDisconnectedOK;
        }

        private void HeadsetDisconnectedOK(object sender, bool e)
        {
            _headsetId = "";
            OnHeadsetDisConnected(this, true);
        }

        private void HeadsetConnectedOK(object sender, string headsetId)
        {
            if (!String.IsNullOrEmpty(headsetId))
            {
                _headsetId = headsetId;
                OnHeadsetConnected(this, _headsetId);
            }
        }

        private void QueryHeadsetOK(object sender, List<Headset> headsets)
        {
            if ( headsets.Count > 0)
            {
                _isFoundHeadset = true;
                //Turn off timer
                _aTimer.Stop();
                _aTimer.Dispose();

                Headset headset = headsets.First<Headset>();
                if (headset.Status == "discovered")
                {
                    JObject flexMappings = new JObject();
                    if (headset.HeadsetID.IndexOf("FLEX", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        // For an Epoc Flex headset, we need a mapping
                        flexMappings = JObject.Parse(Config.FlexMapping);
                    }
                    _ctxClient.ControlDevice("connect", headset.HeadsetID, flexMappings);
                }
                else if (headset.Status == "connected")
                {
                    _headsetId = headset.HeadsetID;
                    OnHeadsetConnected(this, _headsetId);
                }
                else if (headset.Status == "connecting")
                {
                    Console.WriteLine(" Waiting for headset connection " + headset.HeadsetID);
                }
            }
            else
            {
                _isFoundHeadset = false;
                Console.WriteLine(" No headset available. Please connect headset to the machine");
            }
        }

        // Property
        public string HeadsetId
        {
            get
            {
                return _headsetId;
            }
        }

        public void FindHeadset()
        {
            Console.WriteLine("FindHeadset");
            if (!_isFoundHeadset)
            {
                SetTimer(); // set timer for query headset
                _ctxClient.QueryHeadsets("");
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
            if (!_isFoundHeadset)
            {
                // Still not found headset
                // Query headset again
                _ctxClient.QueryHeadsets("");
            }
        }
    }
}
