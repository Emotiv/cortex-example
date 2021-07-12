using UnityEngine;
using Zenject;
using dirox.emotiv.controller;

namespace dirox.emotiv
{
    public class GameControllerInstaller : MonoInstaller<GameControllerInstaller>
    {
        [Header ("Cortex")]
        [SerializeField] UI_ConnectingToCortex connectingToCortex;
        [SerializeField] UI_InstallEmotivApp   installEmotivApp;
        [SerializeField] UI_LoginViaEmotivApp  loginViaEmotivApp;
        [SerializeField] UI_TrialExpired       trialExpried;
        [SerializeField] UI_OfflineUseLimit    offlineUseLimit;

        [Header ("Connect Headset")]
        [SerializeField] HeadsetGroup headsetGroup;
        [SerializeField] ConnectHeadsetController connectHeadsetController;
        [SerializeField] ConnectHeadsetElement headsetElementPrefab;

        [Header ("Contact Quality")]
        [SerializeField] ContactQualityController contactQualityController;
        [SerializeField] ContactQualityColorSetting contactQualityColorSetting;

        [Header ("Connection Quality Indicator")]
        [SerializeField] ConnectionIndicatorGroup connectionIndicatorGroup;
        [SerializeField] ConnectionQualityIndicator connectionQualityIndicator;
        [SerializeField] BatteryIndicator batteryIndicator;

        [Header ("Data Subscriber")]
        [SerializeField] DataSubscriber dataSubscriber;

        [Header ("Examples Board")]
        [SerializeField] ExamplesBoard examplesBoard;

        [Header ("Markers Demo")]
        [SerializeField] MarkersDemo markersDemo;

        public override void InstallBindings ()
        {
            bindConnectHeadset ();
            bindContactQuality ();
            bindIndicators ();
            bindCortexGroup();
            bindExamplesBoard();
            bindDataSubscriber();
            bindMarkersDemo();
        }

        private void bindConnectHeadset ()
        {
            Container.BindInstance (headsetGroup);
            Container.BindInstance (connectHeadsetController);
            Container.BindFactory<ConnectHeadsetElement, ConnectHeadsetElement.Factory> ().FromComponentInNewPrefab (headsetElementPrefab);
            Container.Bind<ConnectHeadsetAdapter> ().FromNew ().AsSingle ();
        }

        private void bindContactQuality ()
        {
            var connectedDevice = new ConnectedDevice ();
            Container.BindInstance (contactQualityController);
            Container.BindInstance (connectedDevice);
            bindContactQualityNodes ();
        }
            
        /// <summary>
        /// To use any of this, do like this: [Inject(Id = "AF3")] ContactQualityNodeView AF3;
        /// </summary>
        private void bindContactQualityNodes ()
        {
            Container.Bind<ContactQualityNodeView> ().WithId ("AF3").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("AF4").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("CMS").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("DRL").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("F3").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("F4").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("F7").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("F8").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("FC5").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("FC6").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("O1").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("O2").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("P7").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("P8").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("PZ").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("T7").FromNew ().AsSingle ();
            Container.Bind<ContactQualityNodeView> ().WithId ("T8").FromNew ().AsSingle ();
            Container.BindInstance (contactQualityColorSetting);
        }

        private void bindIndicators ()
        {
            Container.BindInstance (connectionIndicatorGroup);
            Container.BindInstance (connectionQualityIndicator);
            Container.BindInstance (batteryIndicator);
        }

        private void bindCortexGroup()
        {
            Container.BindInstance (connectingToCortex);
            Container.BindInstance (installEmotivApp);
            Container.BindInstance (loginViaEmotivApp);
            Container.BindInstance (trialExpried);
            Container.BindInstance (offlineUseLimit);
        }

        private void bindDataSubscriber()
        {
            Container.BindInstance(dataSubscriber);
        }

        private void bindExamplesBoard()
        {
            Container.BindInstance(examplesBoard);
        }

        private void bindMarkersDemo()
        {
            Container.BindInstance(markersDemo);
        }
    }
}