using System;
using UnityEngine;
using UnityEngine.UI;
using EmotivUnityPlugin;
using Zenject;

namespace dirox.emotiv.controller
{
    public class UI_InstallEmotivApp : BaseCanvasView
    {
        public Image image_mac;
        public Image image_win;
        UI_ConnectingToCortex _connectingToCortex;

        const string _downloadLink = "https://www.emotiv.com/developer";
        bool _isCortexInstalled = false;
        float _timerCortex_state = 0;
        const float TIME_UPDATE_CORTEX_STATE = 1f;

        [Inject]
        public void InjectDependencies (UI_ConnectingToCortex connectingToCortex)
        {
            this._connectingToCortex = connectingToCortex;
        }

        public override void Activate() 
        {
#if UNITY_STANDALONE_OSX
            image_mac.gameObject.SetActive (true);
            image_win.gameObject.SetActive (false);
#elif UNITY_STANDALONE_WIN
            image_mac.gameObject.SetActive (false);
            image_win.gameObject.SetActive (true);
#endif
            base.Activate ();
        }

        void Update()
        {
            if (_isCortexInstalled || !this.isActive)
                return;

            _timerCortex_state += Time.deltaTime;
            if (_timerCortex_state < TIME_UPDATE_CORTEX_STATE) 
                return;

            _timerCortex_state -= TIME_UPDATE_CORTEX_STATE;

            var curState = DataStreamManager.Instance.GetConnectToCortexState();
            switch (curState) {
                case ConnectToCortexStates.Service_connecting:
                case ConnectToCortexStates.EmotivApp_NotFound:
                    break;
                case ConnectToCortexStates.Login_waiting:
                case ConnectToCortexStates.Login_notYet:					
                case ConnectToCortexStates.Authorizing:
                case ConnectToCortexStates.Authorize_failed:
                case ConnectToCortexStates.Authorized:
                case ConnectToCortexStates.LicenseExpried:
                case ConnectToCortexStates.License_HardLimited: {
                    _isCortexInstalled = true;
                    _connectingToCortex.Activate();
                    this.Deactivate();
                    break;
                }
            }
        }

        public void onDownloadInstallerClicked()
        {
            Application.OpenURL(_downloadLink);
        }
    }
}
