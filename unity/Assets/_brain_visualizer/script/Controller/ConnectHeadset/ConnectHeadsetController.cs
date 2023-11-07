using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System;
using DG.Tweening;

using EmotivUnityPlugin;

namespace dirox.emotiv.controller
{
    public class ConnectHeadsetController : BaseCanvasView
    {
        [SerializeField] private Transform    deviceList;
        [SerializeField] private GameObject   txtNote;
        [SerializeField] private CanvasScaler rootCanvasScaler;
        [SerializeField]
        private Image fadeImage;
        [SerializeField] Image title;
        [SerializeField] Text  connectText;

        ConnectHeadsetAdapter      _headsetAdapter;
        ConnectedDevice            _connectedDevice;
        ContactQualityController   _contactQualityController;
        UI_ConnectingToCortex      _connectingToCortex;
        ConnectionIndicatorGroup   _connectionIndicatorGroup;

        Color tempColor;
        RectTransform canvasRect;
        
        ConnectToCortexStates _lastState;
        float _timerCortex_state = 0;
        const float TIME_UPDATE_CORTEX_STATE = 2f;
        bool _enableChecking = false;

        [Inject]
        public void InjectDependencies (UI_ConnectingToCortex connectingToCortex, HeadsetGroup headsetGroup,
                                        ConnectHeadsetAdapter adapter,  ConnectedDevice connectedDevice,
                                        ContactQualityController contactQualityController,
                                        ConnectionIndicatorGroup connectionIndicatorGroup)
        {
            _headsetAdapter             = adapter;
            _connectedDevice            = connectedDevice;
            _contactQualityController   = contactQualityController;
            _connectingToCortex         = connectingToCortex;
            _connectionIndicatorGroup   = connectionIndicatorGroup;

            init ();
        }

        private void init ()
        {
            this._headsetAdapter.onNewItemReceived += addNewDevice;
            this._headsetAdapter.onClearItems      += clearDeviceAll;
            tempColor = fadeImage.color;
            canvasRect = rootCanvasScaler.GetComponent<RectTransform>();
            if(canvasRect.rect.height< rootCanvasScaler.referenceResolution.y) {
                float ratio = canvasRect.rect.height / rootCanvasScaler.referenceResolution.y;
                float yPos  = title.rectTransform.anchoredPosition.y * ratio;

                yPos = Mathf.Clamp(yPos, connectText.rectTransform.anchoredPosition.y + connectText.rectTransform.rect.height + title.rectTransform.rect.height, title.rectTransform.anchoredPosition.y);
                title.rectTransform.anchoredPosition = new Vector2(title.rectTransform.anchoredPosition.x, yPos);
            }
        }

        public void Refresh () 
        {						
            StartCoroutine(ChangeCanvasScaleMode(FADE_IN_TIME, true));
            tempColor = fadeImage.color;
            canvasRect = rootCanvasScaler.GetComponent<RectTransform>();
            if(canvasRect.rect.height< rootCanvasScaler.referenceResolution.y) {
                float ratio = canvasRect.rect.height / rootCanvasScaler.referenceResolution.y;
                float yPos  = title.rectTransform.anchoredPosition.y * ratio;

                yPos = Mathf.Clamp(yPos, connectText.rectTransform.anchoredPosition.y + connectText.rectTransform.rect.height + title.rectTransform.rect.height, title.rectTransform.anchoredPosition.y);
                title.rectTransform.anchoredPosition = new Vector2(title.rectTransform.anchoredPosition.x, yPos);
            }
        }
            
        private void addNewDevice (ConnectHeadsetElement newDevice)
        {
            txtNote.SetActive (false);
            deviceList.gameObject.SetActive (true);
            newDevice.transform.SetParent (deviceList, false);
        }

        private void clearDeviceAll (ConnectHeadsetElement newDevice)
        {
            txtNote.SetActive (true);
            foreach(Transform child in deviceList.transform) {
                Destroy(child.gameObject);
            }
            deviceList.DetachChildren();
            deviceList.gameObject.SetActive (false);
        }

        void showNextForm ()
        {
            Deactivate ();
            _contactQualityController.Activate ();
        }

        public void StartHeadsetForms(Headset deviceInfo, Action onProgress)
        {
            onProgress.Invoke ();
            StartCoroutine (setConnect (deviceInfo, showNextForm));
        }

        private YieldInstruction timeToWait = new WaitForSeconds (1);

        IEnumerator setConnect (Headset headsetInformation, Action onConnected)
        {
            _connectedDevice.Information = headsetInformation;
            yield return null;
            onConnected.Invoke ();
        }

        public void AddAvailableDevice(Headset headsetInfo) {
            // Debug.Log("AddAvailableDevice");
            _headsetAdapter.AddHeadset(headsetInfo);
        }

        public void ClearHeadsetList() {
            _headsetAdapter.ClearHeadsetList();
        }
            
        public override void Deactivate()
        {
            //tempColor.a = 1f;
            //fadeImage.color = tempColor;
            base.Deactivate(()=> {
                StartCoroutine(ChangeCanvasScaleMode(FADE_OUT_TIME, false));
            });
        }

        public override void Activate()
        {
            _enableChecking = true;
            base.Activate();
        }

        void Update() {

            if (IsActive && !DataStreamManager.Instance.IsHeadsetScanning) {
				// Start scanning headset at headset list screen
				DataStreamManager.Instance.ScanHeadsets();
			}
        }

        IEnumerator ChangeCanvasScaleMode(float delayTime, bool isHeadsetListForm)
        {
            if (isHeadsetListForm) {
                rootCanvasScaler.uiScaleMode     = CanvasScaler.ScaleMode.ConstantPixelSize;
            } else {
                rootCanvasScaler.uiScaleMode     = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }

            rootCanvasScaler.referenceResolution = new Vector2(1024, 768);
            rootCanvasScaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            rootCanvasScaler.matchWidthOrHeight  = 0.5f;
            tempColor.a = 0f;
            yield return new WaitForSeconds(delayTime);
            fadeImage.DOColor(tempColor, delayTime);
        }
        
        bool updateCortexStates ()
        {
            if (!_enableChecking)
                return _enableChecking;

            _timerCortex_state += Time.deltaTime;
            if (_timerCortex_state < TIME_UPDATE_CORTEX_STATE)
                return _enableChecking;

            _timerCortex_state -= TIME_UPDATE_CORTEX_STATE;
            var curState = DataStreamManager.Instance.GetConnectToCortexState();

            if (_lastState == curState)
                return _enableChecking;

            _lastState = curState;
            switch (curState) {
                case ConnectToCortexStates.Service_connecting:
                case ConnectToCortexStates.EmotivApp_NotFound:
                case ConnectToCortexStates.Login_waiting:
                case ConnectToCortexStates.Login_notYet:
                case ConnectToCortexStates.Authorizing:
                case ConnectToCortexStates.Authorize_failed:
                case ConnectToCortexStates.LicenseExpried:
                case ConnectToCortexStates.License_HardLimited: {
                    _enableChecking = false;
                    _connectionIndicatorGroup.Deactivate ();
                    _connectingToCortex.Activate();

                    break;
                }
                case ConnectToCortexStates.Authorized:
                    break;
            }

            return _enableChecking;
        }
    }
}