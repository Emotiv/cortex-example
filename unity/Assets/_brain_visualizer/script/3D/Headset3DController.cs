using UnityEngine;
using Zenject;
using EmotivUnityPlugin;

namespace dirox.emotiv.controller
{
    public class Headset3DController : MonoBehaviour
    {

        [SerializeField] private GameObject insight;
        [SerializeField] private GameObject epoc;

        [Inject]
        public void InjectDependencies (ConnectedDevice connectedDevice)
        {
            connectedDevice.onHeadsetSelected += setConnectedHeadset;
        }

        private void setConnectedHeadset (Headset selectedHeadsetInformation)
        {
            bool isInsightConnected = Utils.IsInsightType(selectedHeadsetInformation.HeadsetType);

            insight.SetActive (isInsightConnected);
            epoc.SetActive (!isInsightConnected);
        }
    }
}