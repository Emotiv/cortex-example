/*
 * Class: Connect Headset Element
 * Manage headset element in headset list
 * Get click event
 * Set headset element infomation
 * 
 * Last Changed: hoang@emotiv.com
 * Date: 21 / 12 / 2017 
*/


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using EmotivUnityPlugin;

namespace dirox.emotiv.controller
{
    public class ConnectHeadsetElement : MonoBehaviour
    {
        #region UI

        [SerializeField] private Text   nameField;
        [SerializeField] private Text   connectingLabel;
        [SerializeField] private Button dongleButton;
        [SerializeField] private Button bluetoothButton;
        [SerializeField] private Button cableButton;

        private Headset _headsetInformation;

        bool _isConnected = false; // fixed the issue get much of event from only 1 signal

        void Start ()
        {
            var dongleSprite = Resources.Load<Sprite>("btnDongle");
            dongleButton.image.sprite = dongleSprite;
            dongleButton.onClick.AddListener (startConnectToDevice);

            var btleSprite = Resources.Load<Sprite>("btnBluetooth");
            bluetoothButton.image.sprite = btleSprite;
            bluetoothButton.onClick.AddListener (startConnectToDevice);

            var cableSprite = Resources.Load<Sprite>("btnCable");
            cableButton.image.sprite = cableSprite;
            cableButton.onClick.AddListener (startConnectToDevice);

            DataProcessing.Instance.HeadsetConnected += connectSuccess;
            DataProcessing.Instance.HeadsetConnectFail += connectFailed;
        }

        void OnDestroy()
        {
            DataProcessing.Instance.HeadsetConnected -= connectSuccess;
            DataProcessing.Instance.HeadsetConnectFail -= connectFailed;
        }

        public ConnectHeadsetElement SetName (string name)
        {			
            this.nameField.text = name;
            return this;
        }

        public ConnectHeadsetElement SetConnectionType (ConnectionType connectionType)
        {
            dongleButton.gameObject.SetActive (connectionType    == ConnectionType.CONN_TYPE_DONGLE);
            bluetoothButton.gameObject.SetActive (connectionType == ConnectionType.CONN_TYPE_BTLE);
            cableButton.gameObject.SetActive (connectionType     == ConnectionType.CONN_TYPE_USB_CABLE);
            return this;
        }

        private void startConnectToDevice ()
        {
            connectingLabel.gameObject.SetActive (true);
            dongleButton.gameObject.SetActive (false);
            bluetoothButton.gameObject.SetActive (false);
            cableButton.gameObject.SetActive (false);

            List<string> dataStreamList = new List<string>(){DataStreamName.DevInfos};
            DataStreamManager.Instance.StartDataStream(dataStreamList, _headsetInformation.HeadsetID);
        }

        private void connectSuccess(object sender, string headsetID)
        {
            if(_isConnected)
                return;

            if (_headsetInformation.HeadsetID == headsetID) {
                mainController.StartHeadsetForms (_headsetInformation, () => {
                });
                DataProcessing.Instance.EnableQueryHeadset(true);
                DataProcessing.Instance.SetConnectedHeadset (_headsetInformation);
            } else {
                Debug.Log("Another headset connected or wrong somewhere");
            }
        }

        private void connectFailed(object sender, string headsetID)
        {
            DataProcessing.Instance.EnableQueryHeadset(true);
        }

        #endregion

        #region Dependency Injection

        private ConnectHeadsetController mainController;

        [Inject]
        public void SetDependencies (ConnectHeadsetController mainController)
        {
            this.mainController = mainController;
        }

        #endregion

        public ConnectHeadsetElement WithInformation (Headset info)
        {
            this._headsetInformation = info;
            SetName (this._headsetInformation.HeadsetID);
            SetConnectionType (this._headsetInformation.HeadsetConnection);
            return this;
        }

        public class Factory:Factory<ConnectHeadsetElement>
        {
        }
    }
}