using System.Collections;
using UnityEngine;
using EmotivUnityPlugin;

namespace dirox.emotiv.controller
{
	public class BatteryIndicator : BaseIndicator
    {
        private float _currentBatteryLevel = 0;
        private int   _maxBatteryLevel = 4;
        private float _lastBattery = 0;

        public override void Activate()
        {
            base.Activate();
            StartCoroutine(RunCoRoutineDisplayBatteryLevel(updateTimeInterval));
        }

        IEnumerator RunCoRoutineDisplayBatteryLevel(float timeInterval)
        {
            while (this.IsActive)
            {
                _currentBatteryLevel = (float)DataStreamManager.Instance.Battery();
                _maxBatteryLevel     = (int)DataStreamManager.Instance.BatteryMax();

                if (_currentBatteryLevel >= 0)
                    _lastBattery = _currentBatteryLevel;

                float batteryLevel = 0;
                if (_maxBatteryLevel > 0)
                    batteryLevel = (_lastBattery*1f / _maxBatteryLevel*1f) * 100f;

                // Debug.LogFormat("<color=blue> XXXXX charge level: {0}, max battery level: {1}, Battery Level (%): {2}</color>", _currentBatteryLevel, _maxBatteryLevel, batteryLevel);
                yield return new WaitForSeconds(timeInterval);
                SetIndicatorDisplay(batteryLevel);
            }
        }
    }
}