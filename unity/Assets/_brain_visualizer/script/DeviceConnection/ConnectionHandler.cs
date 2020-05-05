
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Zenject;
namespace dirox.emotiv.controller
{
    public class ConnectionHandler : MonoBehaviour
    {

        ConnectHeadsetController   _connectHeadsetController;
        ConnectionIndicatorGroup   _connectionIndicatorGroup;
        HeadsetGroup		       _headsetGroup;

        ContactQualityController   _contactQualityController;

        void Start()
        {
            DataProcessing.Instance.onHeadsetChange      += OnHeadsetChanged;
            DataProcessing.Instance.onCurrHeadsetRemoved += onCurrHeadsetRemoved;
        }

        [Inject]
        public void InjectDependency(ConnectHeadsetController connectHeadsetController, ConnectionIndicatorGroup connectionIndicatorGroup, 
                                     HeadsetGroup headsetGroup,
                                     ContactQualityController contactQualityController)
        {
            _connectHeadsetController = connectHeadsetController;
            _connectionIndicatorGroup = connectionIndicatorGroup;
            _headsetGroup             = headsetGroup;
            _contactQualityController = contactQualityController;
        }
        
        private void OnHeadsetChanged(object sender, EventArgs args)
        {
            ShowHeadset();
        }

        private void onCurrHeadsetRemoved(object sender, EventArgs args)
        {
            ShowHeadsetListForm();
        }

        private void ShowHeadset()
        {
            _connectHeadsetController.ClearHeadsetList ();
            
            if (DataProcessing.Instance.GetHeadsetList().Count == 0) 
                return;

            foreach (var item in DataProcessing.Instance.GetHeadsetList())
            {
                if (item.Value == null)
                    continue;
                
                _connectHeadsetController.AddAvailableDevice(item.Value);
            }
        }

        private void ShowHeadsetListForm()
        {
            _contactQualityController.Deactivate();
            _connectionIndicatorGroup.Deactivate ();

            _connectHeadsetController.Refresh ();
            _connectHeadsetController.Activate ();
            ShowHeadset ();
        }
    }
}



