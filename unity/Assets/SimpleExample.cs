using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmotivUnityPlugin;
using UnityEngine.UI;
using System;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif
public class SimpleExample : MonoBehaviour
{
    // Please fill clientId and clientSecret of your application in AppConfig.cs before starting
    private string _appName = "UnityApp";
    private string _appVersion = "3.3.0";
    private bool _isStarted = false;

    Logger _logger = Logger.Instance;

    EmotivUnityItf _eItf = EmotivUnityItf.Instance;
    BCIGameItf _bciGameItf = BCIGameItf.Instance;
    float _timerDataUpdate = 0;
    const float TIME_UPDATE_DATA = 1f;

    [SerializeField] public InputField  HeadsetId;   // headsetId
    [SerializeField] public InputField  RecordTitle;     // record Title
    [SerializeField] public InputField  RecordDescription;     // record description
    [SerializeField] public InputField  ProfileName;   // headsetId

    [SerializeField] public Dropdown ActionNameList;

    [SerializeField] public InputField  MarkerValue;     // marker value
    [SerializeField] public InputField  MarkerLabel;     // marker Label
    [SerializeField] public Toggle EEGToggle;
    [SerializeField] public Toggle MOTToggle;
    [SerializeField] public Toggle PMToggle;
    [SerializeField] public Toggle CQToggle;
    [SerializeField] public Toggle POWToggle;
    [SerializeField] public Toggle EQToggle;
    [SerializeField] public Toggle COMToggle;
    [SerializeField] public Toggle FEToggle;
    [SerializeField] public Toggle SYSToggle;

    [SerializeField] public Text MessageLog;

    // for android
    #if UNITY_ANDROID
    private const string FineLocationPermission = "android.permission.ACCESS_FINE_LOCATION";
    private const string BluetoothScanPermission = "android.permission.BLUETOOTH_SCAN";
    private const string BluetoothConnectPermission = "android.permission.BLUETOOTH_CONNECT";
    private const string BluetoothPermission = "android.permission.BLUETOOTH";


    private bool HasAllPermissions()
    {
        // check location permission
        if (!HasPermission(FineLocationPermission))
        {
            return false;
        }

        // check bluetooth permission
        AndroidJavaClass jc = new AndroidJavaClass("android.os.Build$VERSION");
        int androidVersion = jc.GetStatic<int>("SDK_INT");

        if (androidVersion >= 31)
        {
            // Android 12 or higher. need bluetooth scan and connect permission
            if (!HasPermission(BluetoothScanPermission) || !HasPermission(BluetoothConnectPermission))
            {
                return false;
            }
        }
        else
        {
            // Android 11 or lower. need bluetooth permission
            if (!HasPermission(BluetoothPermission))
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator RequestPermissions()
    {
        // check android version . if android version >= 31, need bluetooth scan and connect permission otherwise need bluetooth permission only. the location permission is always needed
        AndroidJavaClass jc = new AndroidJavaClass("android.os.Build$VERSION");
        int androidVersion = jc.GetStatic<int>("SDK_INT");
        string[] _permissions;

        if (androidVersion >= 30)
        {
            _permissions = new string[] { FineLocationPermission, BluetoothScanPermission, BluetoothConnectPermission };
        }
        else
        {
            _permissions = new string[] { FineLocationPermission, BluetoothPermission };
        }

        foreach (var permission in _permissions)
        {
            if (!HasPermission(permission))
            {
                RequestPermission(permission);
                yield return new WaitUntil(() => HasPermission(permission));
            }
        }
    }

    // start EmotivUnityItf for android
    private void StartEmotivUnityItfForAndroid() {
        if (HasAllPermissions()) {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _bciGameItf.Start(AppConfig.ClientId, AppConfig.ClientSecret, AppConfig.UserName, AppConfig.Password, currentActivity);
            _isStarted = true;
        }
        else {
            StartCoroutine(RequestPermissions());
        }
    }

    private static bool HasPermission(string permissionName)
    {
        return Permission.HasUserAuthorizedPermission(permissionName);
    }

    private static void RequestPermission(string permissionName) {
        if (Permission.HasUserAuthorizedPermission(permissionName))
        {
            Debug.Log("Permission " + permissionName + " is authorized");
        }
        else
        {
            // We do not have permission to use the microphone.
            // Ask for permission or proceed without the functionality enabled.
            Permission.RequestUserPermission(permissionName);
        }
    }
    #endif
    
    void Start()
    {
        if (_isStarted)
            return;
        
        // init utils to create needed directory
        Utils.Init();
        
        _logger.Init();

        #if UNITY_ANDROID
            StartEmotivUnityItfForAndroid();
        # elif UNITY_IOS
            UnityEngine.Debug.Log("SimpleExp: Start EmotivUnityItf for ios");
        #else
            UnityEngine.Debug.Log("SimpleExp: Start EmotivUnityItf for desktop");
            _eItf.Init(AppConfig.ClientId, AppConfig.ClientSecret, _appName, _appVersion, AppConfig.UserName, AppConfig.Password);
            _eItf.Start();
            _isStarted = true;
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        _timerDataUpdate += Time.deltaTime;
        if (_timerDataUpdate < TIME_UPDATE_DATA) 
            return;
        _timerDataUpdate -= TIME_UPDATE_DATA;

        if ( _bciGameItf.GetLogMessage().Contains("Get Error:")) {
            // show error in red color
            MessageLog.color = Color.red;
        }
        else {
            // update message log
            MessageLog.color = Color.black;
        }
        MessageLog.text = _bciGameItf.GetLogMessage();


        // get detected headset lists
        // List<Headset> detectedHeadsets = _bciGameItf.GetDetectedHeadsets();


        #if UNITY_ANDROID
        // check all permissions are granted
        if (!_isStarted) {
            StartEmotivUnityItfForAndroid();
        }
        #endif

        if (!_bciGameItf.IsAuthorized())
            return;


        // check connect headset state
        // ConnectHeadsetStates connectHeadsetState = _bciGameItf.GetConnectHeadsetState();
        
        // Check buttons interactable
        CheckButtonsInteractable();

        // If save data to Data buffer. You can do the same EEG to get other data streams
        // Otherwise check output data at OnEEGDataReceived(), OnMotionDataReceived() ..etc..
        // get contact quality and battery level of the headset
        if (_bciGameItf.GetNumberCQSamples() > 0) {
            BatteryInfo batteryInfo = _bciGameItf.GetBattery();
            UnityEngine.Debug.Log("Battery: " + batteryInfo.overallBattery + ", batteryleft: " + batteryInfo.batteryLeft + ", batteryRight: " + batteryInfo.batteryRight);

            // get contact quality for channel t7 and t8
            double cqT7 = _bciGameItf.GetContactQuality(Channel_t.CHAN_T7);
            double cqT8 = _bciGameItf.GetContactQuality(Channel_t.CHAN_T8);
            UnityEngine.Debug.Log("CQ T7: " + cqT7 + ", CQ T8: " + cqT8);
        }

        // if (_bciGameItf.GetNumberEQSamples() > 0) {
        //     BatteryInfo batteryInfo = _bciGameItf.GetBattery();
        //     UnityEngine.Debug.Log("qqqq Battery: " + batteryInfo.overallBattery + ", batteryleft: " + batteryInfo.batteryLeft + ", batteryRight: " + batteryInfo.batteryRight);

        //     // get contact quality for channel t7 and t8
        //     double cqT7 = _bciGameItf.GetEQ(Channel_t.CHAN_T7);
        //     double cqT8 = _bciGameItf.GetEQ(Channel_t.CHAN_T8);
        //     UnityEngine.Debug.Log("qqqqqq CQ T7: " + cqT7 + ", CQ T8: " + cqT8);
        // }


        // get training done
        if (_bciGameItf.IsTrainingCompleted()) {
            UnityEngine.Debug.Log("Training done" + _bciGameItf.GetMentalCommandActionPower() + " is good mcAction " + _bciGameItf.IsGoodMCAction());
        }

        // get sensitivity
        // if (_bciGameItf.GetFirstMCActionSensitivity()  > -1) {
        //     UnityEngine.Debug.Log("First MC Action Sensitivity: " + _bciGameItf.GetFirstMCActionSensitivity());
        // }

        // if (_isDataBufferUsing) {
        //     // get eeg data
        //     if (_eItf.GetNumberEEGSamples() > 0) {
        //         string eegHeaderStr = "EEG Header: ";
        //         string eegDataStr   = "EEG Data: ";
        //         foreach (var ele in _eItf.GetEEGChannels()) {
        //             string chanStr  = ChannelStringList.ChannelToString(ele);
        //             double[] data     = _eItf.GetEEGData(ele);
        //             eegHeaderStr    += chanStr + ", ";
        //             if (data != null && data.Length > 0)
        //                 eegDataStr      +=  data[0].ToString() + ", ";
        //             else
        //                 eegDataStr      +=  "null, "; // for null value
        //         }
        //         string msgLog = eegHeaderStr + "\n" + eegDataStr;
        //         MessageLog.text = msgLog;
        //     }
        // }

    }

    /// <summary>
    /// create session 
    /// </summary>
    public void onQueryHeadsetBtnClick() {
        Debug.Log("onQueryHeadsetBtnClick");
        _bciGameItf.QueryHeadsets();
    }


    /// <summary>
    /// create session 
    /// </summary>
    public void onCreateSessionBtnClick() {
        Debug.Log("onCreateSessionBtnClick");
        if (!_bciGameItf.IsSessionCreated())
        {
            _bciGameItf.StartStreamData(HeadsetId.text);
        }
        else
        {
            UnityEngine.Debug.LogError("There is a session created.");
        }
    }

    /// <summary>
    /// start a record 
    /// </summary>
    public void onStartRecordBtnClick() {
        Debug.Log("onStartRecordBtnClick " + RecordTitle.text + ":" + RecordDescription.text);
        if (_eItf.IsSessionCreated)
        {
            string recordTitle = RecordTitle.text;
            if (string.IsNullOrEmpty(recordTitle)) {
                recordTitle = "Record test_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }

            _eItf.StartRecord(recordTitle, RecordDescription.text);
        }
        else {
            UnityEngine.Debug.LogError("Can not start a record because there is no active session or record title is empty.");
        }
    }

    /// <summary>
    /// start a record 
    /// </summary>
    public void onStopRecordBtnClick() {
        Debug.Log("onStopRecordBtnClick");
        _eItf.StopRecord();
    }

    /// <summary>
    /// inject marker
    /// </summary>
    public void onInjectMarkerBtnClick() {
        Debug.Log("onInjectMarkerBtnClick " + MarkerValue.text + ":" + MarkerLabel.text);
        String markerValue = MarkerValue.text;
        String markerLabel = MarkerLabel.text;
        if (string.IsNullOrEmpty(markerValue)) {
            markerValue = DateTime.Now.ToString("ss");
        }
        if (string.IsNullOrEmpty(markerLabel)) {
            markerLabel = "Marker_" + markerValue;
        }
        _eItf.InjectMarker(markerLabel, markerValue);
    }

    /// <summary>
    /// subscribe data stream
    /// </summary>
    public void onSubscribeBtnClick() {
        Debug.Log("onSubscribeBtnClick: " + _eItf.IsSessionCreated + ": " + GetStreamsList().Count);
        if (_eItf.IsSessionCreated)
        {
            if (GetStreamsList().Count == 0) {
                UnityEngine.Debug.LogError("The stream name is empty. Please set a valid stream name before subscribing.");
            }
            else {
                _eItf.DataSubLog = ""; // clear data subscribing log
                _eItf.SubscribeData(GetStreamsList());
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Must create a session first before subscribing data.");
        }
    }

    /// <summary>
    /// un-subscribe data
    /// </summary>
    public void onUnsubscribeBtnClick() {
        Debug.Log("onUnsubscribeBtnClick");
        if (GetStreamsList().Count == 0) {
            UnityEngine.Debug.LogError("The stream name is empty. Please set a valid stream name before unsubscribing.");
        }
        else {
            _eItf.DataSubLog = ""; // clear data subscribing log
            _eItf.UnSubscribeData(GetStreamsList());
        }
    }

    /// <summary>
    /// load an exited profile or create a new profile then load the profile
    /// </summary>
    public void onLoadProfileBtnClick() {
        Debug.Log("onLoadProfileBtnClick " + ProfileName.text);
        _bciGameItf.CreatePlayer(ProfileName.text);
    }

    /// <summary>
    /// unload a profile
    /// </summary>
    public void onUnLoadProfileBtnClick() {
        Debug.Log("onUnLoadProfileBtnClick " + ProfileName.text);
        _eItf.UnLoadProfile();
    }

    /// <summary>
    /// save a profile
    /// </summary>
    public void onSaveProfileBtnClick() {
        Debug.Log("onSaveProfileBtnClick " + ProfileName.text);
        _eItf.SaveProfile(ProfileName.text);
    }

    /// <summary>
    /// start a mental command training action
    /// </summary>
    public void onStartMCTrainingBtnClick() {        
        
        _bciGameItf.StartTraining(ActionNameList.captionText.text);
    }

    /// <summary>
    /// accept a mental command training
    /// </summary>
    public void onAcceptMCTrainingBtnClick() {
        _bciGameItf.AcceptTraining();
    }

    /// <summary>
    /// reject a mental command training
    /// </summary>
    public void onRejectMCTrainingBtnClick() {
        _bciGameItf.RejectTraining();
    }

    /// <summary>
    /// erase a mental command training
    /// </summary>
    public void onEraseMCTrainingBtnClick() {
        
        _bciGameItf.EraseDataForMCTrainingAction(ActionNameList.captionText.text);
    }


    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        _bciGameItf.Stop();
    }

    private void CheckButtonsInteractable()
    {
        if (!_eItf.IsAuthorizedOK)
            return;

        Button createSessionBtn = GameObject.Find("SessionPart").transform.Find("createSessionBtn").GetComponent<Button>();
        if (!createSessionBtn.interactable)
        {
            createSessionBtn.interactable = true;
            return;
        }

        // make startRecordBtn interactable
        Button startRecordBtn = GameObject.Find("RecordPart").transform.Find("startRecordBtn").GetComponent<Button>();
        Button subscribeBtn = GameObject.Find("SubscribeDataPart").transform.Find("subscribeBtn").GetComponent<Button>();
        Button unsubscribeBtn = GameObject.Find("SubscribeDataPart").transform.Find("unsubscribeBtn").GetComponent<Button>();
        Button loadProfileBtn = GameObject.Find("TrainingPart").transform.Find("loadProfileBtn").GetComponent<Button>();
        Button unloadProfileBtn = GameObject.Find("TrainingPart").transform.Find("unloadProfileBtn").GetComponent<Button>();
        Button saveProfileBtn = GameObject.Find("TrainingPart").transform.Find("saveProfileBtn").GetComponent<Button>();
        Button rejectTrainingBtn = GameObject.Find("TrainingPart").transform.Find("rejectTrainingBtn").GetComponent<Button>();
        Button eraseTrainingBtn = GameObject.Find("TrainingPart").transform.Find("eraseTrainingBtn").GetComponent<Button>();
        Button acceptTrainingBtn = GameObject.Find("TrainingPart").transform.Find("acceptTrainingBtn").GetComponent<Button>();
        Button startTrainingBtn = GameObject.Find("TrainingPart").transform.Find("startTrainingBtn").GetComponent<Button>();
        Button stopRecordBtn = GameObject.Find("RecordPart").transform.Find("stopRecordBtn").GetComponent<Button>();
        Button injectMarkerBtn = GameObject.Find("RecordPart").transform.Find("injectMarkerBtn").GetComponent<Button>();

        startRecordBtn.interactable = _eItf.IsSessionCreated;
        subscribeBtn.interactable = _eItf.IsSessionCreated;
        unsubscribeBtn.interactable = _eItf.IsSessionCreated;
        loadProfileBtn.interactable = _eItf.IsSessionCreated;

        saveProfileBtn.interactable = _eItf.IsProfileLoaded;
        startTrainingBtn.interactable = _eItf.IsProfileLoaded;
        rejectTrainingBtn.interactable = _eItf.IsProfileLoaded;
        eraseTrainingBtn.interactable = _eItf.IsProfileLoaded;
        acceptTrainingBtn.interactable = _eItf.IsProfileLoaded;
        unloadProfileBtn.interactable = _eItf.IsProfileLoaded;

        stopRecordBtn.interactable = _eItf.IsRecording;
        injectMarkerBtn.interactable = _eItf.IsRecording;
    }

    private List<string> GetStreamsList() {
        List<string> _streams = new List<string> {};
        if (EEGToggle.isOn) {
            _streams.Add("eeg");
        }
        if (MOTToggle.isOn) {
            _streams.Add("mot");
        }
        if (PMToggle.isOn) {
            _streams.Add("met");
        }
        if (CQToggle.isOn) {
            _streams.Add("dev");
        }
        if (SYSToggle.isOn) {
            _streams.Add("sys");
        }
        if (EQToggle.isOn) {
            _streams.Add("eq");
        }
        if (POWToggle.isOn) {
            _streams.Add("pow");
        }
        if (FEToggle.isOn) {
            _streams.Add("fac");
        }
        if (COMToggle.isOn) {
            _streams.Add("com");
        }
        return _streams;
    }
}
