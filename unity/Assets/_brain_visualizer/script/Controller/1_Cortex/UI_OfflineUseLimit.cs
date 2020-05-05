using System;
using UnityEngine;
using UnityEngine.UI;
using EmotivUnityPlugin;
using Zenject;

namespace dirox.emotiv.controller
{
    public class UI_OfflineUseLimit : BaseCanvasView
    {
        UI_ConnectingToCortex _connectingToCortex;

        bool _isChecked = false;
        float _timerCortex_state = 0;
        const float TIME_UPDATE_CORTEX_STATE = 1f;
    
        [Inject]
        public void InjectDependencies (UI_ConnectingToCortex connectingToCortex)
        {
            this._connectingToCortex = connectingToCortex;
        }

        public override void Activate()
        {
            base.Activate ();
        }

        void Update()
        {
            if (_isChecked || !this.isActive)
                return;

            _timerCortex_state += Time.deltaTime;
            if (_timerCortex_state < TIME_UPDATE_CORTEX_STATE) 
                return;

            _timerCortex_state -= TIME_UPDATE_CORTEX_STATE;

            var curState = DataStreamManager.Instance.GetConnectToCortexState();
            switch (curState) {
                case ConnectToCortexStates.Service_connecting:
                case ConnectToCortexStates.EmotivApp_NotFound:
                case ConnectToCortexStates.Login_waiting:
                case ConnectToCortexStates.Login_notYet:
                case ConnectToCortexStates.Authorizing:
                case ConnectToCortexStates.Authorize_failed:
                case ConnectToCortexStates.Authorized:
                case ConnectToCortexStates.LicenseExpried: {
                    _isChecked = true;
                    _connectingToCortex.Activate();
                    this.Deactivate();
                    break;
                }
                case ConnectToCortexStates.License_HardLimited: {					
                    break;
                }
            }
        }

        public void onTryAgainClicked()
        {
            Debug.Log("onTryAgainClicked");
        }
    }
}


