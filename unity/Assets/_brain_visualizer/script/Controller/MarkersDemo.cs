using UnityEngine;
using UnityEngine.UI;
using EmotivUnityPlugin;
using Zenject;

namespace dirox.emotiv.controller
{
    /// <summary>
    /// Demo for inject and update markers
    /// </summary>
    public class MarkersDemo : BaseCanvasView
    {
        RecordManager _recordManager = RecordManager.Instance;

        float _timerDataUpdate = 0;
        const float TIME_UPDATE_DATA = 1f;

        private string _recordResult = "";
        private string _markerResult = "";


        [SerializeField] private InputField  recordTitle;     // record Title

        [SerializeField] private Text  recordLogger;     // record logger
        [SerializeField] private Text  markerLogger;     // markers logger

        [SerializeField] private InputField  markerLabel;     // marker label
        [SerializeField] private InputField  markerValue;     // marker value

        // disable button stop Record, if not start
        // disable inject marker if no record, no marker value, no marker label
        // disable update marker if no marker started

        protected override void OnStart()
        {
            Debug.Log("MarkersDemo: OnStart");
            _recordManager.informMarkerResult += OnInformMarkerResult;
            _recordManager.informRecordResult += OnInformRecordResult;

        }

        void Update() 
        {
            if (!this.isActive) {
                return;
            }

            _timerDataUpdate += Time.deltaTime;
            if (_timerDataUpdate < TIME_UPDATE_DATA) 
                return;

            _timerDataUpdate -= TIME_UPDATE_DATA;

            // update record Logger
            if (!string.IsNullOrEmpty(_recordResult))
                recordLogger.text = _recordResult;

            if (!string.IsNullOrEmpty(_markerResult))
                markerLogger.text = _markerResult;
        }


        private void OnInformRecordResult(object sender, string recordResult)
        {
            _recordResult = recordResult;
        }

        private void OnInformMarkerResult(object sender, string markerResult)
        {
            _markerResult = markerResult;
        }
        
        public override void Activate()
        {
            Debug.Log("MarkersDemo: Activate");
            base.Activate();
        }

        public override void Deactivate()
        {
            Debug.Log("MarkersDemo: Deactivate");
            base.Deactivate();
        }
        
        /// <summary>
        /// start a record
        /// </summary>
        public void onStartRecordBtnClick() {
            Debug.Log("onStartRecordBtnClick");
            _recordResult = "";
            _recordManager.StartRecord(recordTitle.text);
        }

        /// <summary>
        /// stop a record
        /// </summary>
        public void onStopRecordBtnClick() {
            Debug.Log("onStartRecordBtnClick");
            recordLogger.text = ""; // clear record logger
            _recordManager.StopRecord();
        }

        /// <summary>
        /// inject Marker instance to data stream
        /// </summary>
        public void onInjectMarkerBtnClick() {
            Debug.Log("onInjectMarkerBtnClick " + markerLabel.text + ": " + markerValue.text);
            _recordManager.InjectMarker(markerLabel.text, markerValue.text);
        }

        /// <summary>
        /// update marker: update endtime for current marker -> marker interval
        /// </summary>
        public void onUpdateMakerBtnClick() {
            Debug.Log("onUpdateMakerBtnClick ");
            _recordManager.UpdateMarker();
        }
    }
}
