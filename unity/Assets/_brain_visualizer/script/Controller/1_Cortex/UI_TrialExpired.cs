using System;
using UnityEngine;
using UnityEngine.UI;
using EmotivUnityPlugin;
using Zenject;

namespace dirox.emotiv.controller
{
    public class UI_TrialExpired : BaseCanvasView
    {
        const string _buyNowLink = "https://www.emotiv.com";

        float _timerCortex_state = 0;
        const float TIME_UPDATE_CORTEX_STATE = 1f;
        UI_ConnectingToCortex _connectingToCortex;
    
        [Inject]
        public void InjectDependencies (UI_ConnectingToCortex connectingToCortex)
        {
            _connectingToCortex = connectingToCortex;
        }

        public override void Activate() 
        {
            base.Activate ();
        }

        public void onBuyNowClicked()
        {
            // Debug.Log("onBuyNowClicked");
            Application.OpenURL(_buyNowLink);
        }

        void Update()
        {
            if (!this.isActive)
                return;

            _timerCortex_state += Time.deltaTime;
            if (_timerCortex_state < TIME_UPDATE_CORTEX_STATE) 
                return;

            _timerCortex_state -= TIME_UPDATE_CORTEX_STATE;

            var curState = DataStreamManager.Instance.GetConnectToCortexState();
            switch (curState) {
                case ConnectToCortexStates.LicenseExpried:
                    break;
                case ConnectToCortexStates.Service_connecting:
                case ConnectToCortexStates.EmotivApp_NotFound:
                case ConnectToCortexStates.Login_waiting:
                case ConnectToCortexStates.Login_notYet:
                case ConnectToCortexStates.Authorizing:
                case ConnectToCortexStates.Authorize_failed:
                case ConnectToCortexStates.Authorized:
                case ConnectToCortexStates.License_HardLimited: {
                    _connectingToCortex.Activate();
                    this.Deactivate();
                    break;
                }
            }
        }
    }
}

