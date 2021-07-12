using UnityEngine;
using Zenject;

namespace dirox.emotiv.controller
{
    /// <summary>
    /// Board of examples
    /// </summary>
    public class ExamplesBoard : BaseCanvasView 
    {

        // data subscribers
        DataSubscriber dataSubscriber;

        // injectmarkers
        MarkersDemo markersDemo;
        
        [Inject]
        public void SetDependencies(DataSubscriber subscriber, MarkersDemo markers)
        {
            dataSubscriber = subscriber;
            markersDemo = markers;
        }
        public override void Activate()
        {
            Debug.Log("ExamplesBoard: Activate");
            if (markersDemo.IsActive)
                markersDemo.Deactivate();
            if (dataSubscriber.IsActive)
                dataSubscriber.Deactivate();
            base.Activate();
        }

        /// <summary>
        /// go to data subscriber example
        /// </summary>
        public void onBtnSubscriberClick() {
            Debug.Log("onBtnSubscriberClick");
            Deactivate();
            dataSubscriber.Activate();
        }


        /// <summary>
        /// go to injectmarkers example
        /// </summary>
        public void onBtnMarkersClick() {
            Debug.Log("onBtnMarkersClick");
            Deactivate();
            markersDemo.Activate();
        }

    }
}