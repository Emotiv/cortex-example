using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dirox.emotiv.controller
{
    public class ConnectionIndicatorGroup : BaseCanvasView
    {
        [SerializeField] private BaseIndicator qualityIndicator;
        [SerializeField] private BaseIndicator batteryIndicator;

        public override void Activate()
        {
            base.Activate();
            qualityIndicator.Activate();
            batteryIndicator.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            qualityIndicator.Deactivate();
            batteryIndicator.Deactivate();
        }
    }
}