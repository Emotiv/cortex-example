using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using EmotivUnityPlugin;

namespace dirox.emotiv.controller
{
    public class ContactQualityController : BaseCanvasView
    {
        [SerializeField] private ContactQualityBaseManager insight;
        [SerializeField] private ContactQualityBaseManager epoc;
        [SerializeField] private float updateCQInterval = 0.5f;

        ConnectedDevice connectedDevice;
        HeadsetGroup headsetGroup;
        ContactQualityBaseManager  activeDevice;
        ConnectionIndicatorGroup   connectionIndicatorGroup;
        DataSubscriber dataSubscriber;
                
        public Text displayText;

        [Inject]
        public void SetDependencies (ConnectedDevice device, HeadsetGroup headsetGroup,
                                     ConnectionIndicatorGroup connectionIndicatorGroup,
                                     DataSubscriber subscriber)
        {
            this.connectedDevice  = device;
            this.headsetGroup     = headsetGroup;
            this.connectionIndicatorGroup = connectionIndicatorGroup;
            dataSubscriber = subscriber;
        }

        public override void Activate ()
        {
            headsetGroup.Activate();
            connectionIndicatorGroup.Activate ();

            activeDevice = connectedDevice.Information.HeadsetType == HeadsetTypes.HEADSET_TYPE_INSIGHT ? insight : epoc;
            activeDevice.gameObject.SetActive(true);
            base.Activate ();

            StartCoroutine(RunCoroutineDisplayColor(updateCQInterval));
        }

        public override void Deactivate ()
        {
            headsetGroup.Deactivate ();
            base.Deactivate ();            
            StopAllCoroutines();

            if (activeDevice != null)
                activeDevice.gameObject.SetActive(false);
        }

        public void onButtonDone()
        {
            Deactivate();
            dataSubscriber.Activate();
            // connectionIndicatorGroup.Activate ();
        }
            
        public void QuickOpen() {
            Activate();
        }

        public void DisplayContactQualityColor() {
            activeDevice.SetContactQualityColor(DataProcessing.Instance.GetContactQuality());
        }
                    
        IEnumerator RunCoroutineDisplayColor(float timeInteval) {
            while(this.IsActive) {
                DisplayContactQualityColor();
                yield return new WaitForSeconds(timeInteval);
            }
        }
    }
}