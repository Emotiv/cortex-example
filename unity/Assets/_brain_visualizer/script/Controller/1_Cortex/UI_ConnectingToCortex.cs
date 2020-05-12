using System;
using UnityEngine;
using UnityEngine.UI;
using EmotivUnityPlugin;
using Zenject;

namespace dirox.emotiv.controller
{
    public class UI_ConnectingToCortex : BaseCanvasView
    {
        public Text _stateText;

        ConnectHeadsetController _connectHeadsetController;
        HeadsetGroup             _headsetGroup;
        UI_InstallEmotivApp      _installEmotivApp;
        UI_LoginViaEmotivApp	 _loginViaEmotivApp;
        UI_TrialExpired          _trialExpried;
        UI_OfflineUseLimit       _offlineUseLimit;
        ConnectToCortexStates    _lastState;

        float _timerCortex_state = 0;
        const float TIME_UPDATE_CORTEX_STATE = 0.5f;
        bool _isConnectDone = false;

        [Inject]
        public void InjectDependencies (UI_InstallEmotivApp installEmotivApp, UI_LoginViaEmotivApp loginViaEmotivApp,
                                        UI_TrialExpired trialExpried, UI_OfflineUseLimit offlineUseLimit,
                                        ConnectHeadsetController connectHeadsetController, HeadsetGroup headsetGroup)
        {
            this._connectHeadsetController = connectHeadsetController;
            this._headsetGroup             = headsetGroup;
            this._installEmotivApp         = installEmotivApp;
            this._loginViaEmotivApp        = loginViaEmotivApp;
            this._trialExpried             = trialExpried;
            this._offlineUseLimit          = offlineUseLimit;
        }
                    
        void Update()
        {
            if (_isConnectDone)
                return;

            _timerCortex_state += Time.deltaTime;
            if (_timerCortex_state < TIME_UPDATE_CORTEX_STATE)
                return;

            _timerCortex_state -= TIME_UPDATE_CORTEX_STATE;
            var curState = DataStreamManager.Instance.GetConnectToCortexState();
            if (_lastState == curState)
                return;

            // curState = ConnectToCortexStates.License_HardLimited; // TODO: only for test now
            _lastState = curState;
            switch (curState) {
                case ConnectToCortexStates.Service_connecting: {
                    // _stateText.text = "Connecting To service..."; // TODO: check font size
                    Debug.Log("=============== Connecting To service");
                    break;
                }
                case ConnectToCortexStates.EmotivApp_NotFound: {

                    _isConnectDone = true;
                    _installEmotivApp.Activate();
                    this.Deactivate();
                    Debug.Log("=============== Connect_failed");
                    break;
                }
                case ConnectToCortexStates.Login_waiting: {
                    Debug.Log("=============== Login_waiting");
                    break;
                }
                case ConnectToCortexStates.Login_notYet: {
                    _isConnectDone = true;
                    _loginViaEmotivApp.Activate();
                    this.Deactivate();
                    Debug.Log("=============== Login_notYet");
                    break;
                }
                case ConnectToCortexStates.Authorizing: {
                    _stateText.text = "Authenticating...";
                    Debug.Log("=============== Authorizing");
                    break;
                }
                case ConnectToCortexStates.Authorize_failed: {
                    Debug.Log("=============== Authorize_failed");
                    break;
                }
                case ConnectToCortexStates.Authorized: {
                    _isConnectDone = true;
                    ActivateHeadsetQuery();
                    Debug.Log("=============== Authorized");
                    break;
                }
                case ConnectToCortexStates.LicenseExpried: {
                    _isConnectDone = true;
                    _trialExpried.Activate();
                    this.Deactivate();
                    Debug.Log("=============== Trial expired");
                    break;
                }
                case ConnectToCortexStates.License_HardLimited: {
                    _isConnectDone = true;
                    _offlineUseLimit.Activate();
                    this.Deactivate();
                    Debug.Log("=============== License_HardLimited");
                    break;
                }
            }
        }

        public override void Activate() 
        {
            _isConnectDone = false;
            base.Activate ();
        }

        public void ActivateHeadsetQuery()
        {
            _headsetGroup.Activate ();
            _connectHeadsetController.Activate ();
            this.Deactivate ();
        }
    }
}