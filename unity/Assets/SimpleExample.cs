using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmotivUnityPlugin;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class SimpleExample : MonoBehaviour
{
    private bool _isEmotivUnityItfInitialized = false; // the flag to check if EmotivUnityItf is initialized and start connecting to Cortex or not
    EmotivUnityItf _eItf = EmotivUnityItf.Instance;
    float _timerDataUpdate = 0;
    const float TIME_UPDATE_DATA = 1f;
    private string _lastMessageLog = ""; // tracks last displayed _eItf.MessageLog to avoid overwriting manual MessageLog updates

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

    [SerializeField] public InputField MessageLog;
    // recordid for export record
    [SerializeField] public InputField RecordId;     // record id for export record

    // cached button references, populated once in Start() to avoid repeated GameObject.Find calls in Update()
    private Button _signInBtn;
    private Button _signOutBtn;
    private Button _createSessionBtn;
    private Button _queryHeadsetBtn;
    private Button _startRecordBtn;
    private Button _subscribeBtn;
    private Button _unsubscribeBtn;
    private Button _loadProfileBtn;
    private Button _unloadProfileBtn;
    private Button _saveProfileBtn;
    private Button _rejectTrainingBtn;
    private Button _eraseTrainingBtn;
    private Button _acceptTrainingBtn;
    private Button _startTrainingBtn;
    private Button _stopRecordBtn;
    private Button _injectMarkerBtn;
    private Button _exportRecordBtn;
    private Button _queryRecordBtn;

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
            _eItf.Init(AppConfig.ClientId, AppConfig.ClientSecret, AppConfig.AppName, AppConfig.AllowSaveLogToFile, AppConfig.IsDataBufferUsing);
            _eItf.Start(currentActivity);
            _isEmotivUnityItfInitialized = true;
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
        CacheButtonReferences();

    #if USE_EMBEDDED_LIB && UNITY_STANDALONE_WIN && !UNITY_EDITOR
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            _eItf.ProcessCallback(args[1]);
            return;
        }
#endif

#if UNITY_ANDROID
            StartEmotivUnityItfForAndroid();
#elif UNITY_IOS
            UnityEngine.Debug.Log("SimpleExp: Start EmotivUnityItf for ios");
            _eItf.Init(AppConfig.ClientId, AppConfig.ClientSecret, AppConfig.AppName, AppConfig.AllowSaveLogToFile, AppConfig.IsDataBufferUsing);
            _eItf.Start();
#else
        UnityEngine.Debug.Log("SimpleExp: Start EmotivUnityItf for desktop " + AppConfig.AppUrl);
            _eItf.Init(AppConfig.ClientId, AppConfig.ClientSecret, AppConfig.AppName, AppConfig.AllowSaveLogToFile, AppConfig.IsDataBufferUsing, AppConfig.AppUrl);
            _eItf.Start();
            _isEmotivUnityItfInitialized = true;
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        _timerDataUpdate += Time.deltaTime;
        if (_timerDataUpdate < TIME_UPDATE_DATA) 
            return;
        _timerDataUpdate -= TIME_UPDATE_DATA;

        #if UNITY_ANDROID
        if (!_isEmotivUnityItfInitialized) {
            // start EmotivUnityItf for android if not started
            StartEmotivUnityItfForAndroid();
        }
        #endif

        // Check buttons interactable
        CheckButtonsInteractable();
        
        // Display message log only when it has changed
        if (_eItf.MessageLog != _lastMessageLog)
        {
            _lastMessageLog = _eItf.MessageLog;
            MessageLog.text = _eItf.MessageLog;
        }

        // Demo how to get detected headset lists
        // List<Headset> detectedHeadsets = _eItf.GetDetectedHeadsets();

        // Demo how to get subcribed data if use Data Buffer
        if (AppConfig.IsDataBufferUsing) {
            // eeg data
            // if (_eItf.GetNumberEEGSamples() > 0) {
            //     string eegHeaderStr = "EEG Header: ";
            //     string eegDataStr   = "EEG Data: ";
            //     foreach (var ele in _eItf.GetEEGChannels()) {
            //         string chanStr  = ChannelStringList.ChannelToString(ele);
            //         double[] data     = _eItf.GetEEGData(ele);
            //         eegHeaderStr    += chanStr + ", ";
            //         if (data != null && data.Length > 0)
            //             eegDataStr      +=  data[0].ToString() + ", ";
            //         else
            //             eegDataStr      +=  "null, "; // for null value
            //     }
            //     string msgLog = eegHeaderStr + "\n" + eegDataStr;
            //     MessageLog.text = msgLog;
            // }

            // Demo how to get cq data
            if (_eItf.GetNumberCQSamples() > 0) {
                BatteryInfo batteryInfo = _eItf.GetBattery();
                // get contact quality for channel t7 and t8, similar for other channels
                double cqT7 = _eItf.GetContactQuality(Channel_t.CHAN_T7);
                double cqT8 = _eItf.GetContactQuality(Channel_t.CHAN_T8);
                double cqOverall = _eItf.GetContactQuality(Channel_t.CHAN_CQ_OVERALL);
                MessageLog.text = "CQ T7: " + cqT7 + ", CQ T8: " + cqT8 + " cq overall: " + cqOverall;
            }
        }

        // Demo how to check Mentalcommand training completed and get data power
        if (_eItf.IsMCTrainingCompleted) {
            UnityEngine.Debug.Log("Training done. Action: " + _eItf.LatestMentalCommand.act + ", power: " + _eItf.LatestMentalCommand.pow);
        }
    }

    // sign out button
    public void onSignOutBtnClick() {
        Debug.Log("onSignOutBtnClick");
        _eItf.Logout();
    }

    public void onQueryHeadsetBtnClick() {
        Debug.Log("onQueryHeadsetBtnClick");
        _eItf.QueryHeadsets();
    }

    public async void onSignInBtnClick() {
        Debug.Log("onSignInBtnClick");
        #if USE_EMBEDDED_LIB || UNITY_ANDROID || UNITY_IOS
        await _eItf.AuthenticateAsync();
        #endif
    }

    public void onCreateSessionBtnClick() {
        Debug.Log("onCreateSessionBtnClick");
        if (!_eItf.IsSessionCreated) {
            _eItf.CreateSessionWithHeadset(HeadsetId.text);
        }
        else {
            UnityEngine.Debug.LogError("There is a session created.");
        }
    }

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

    public void onStopRecordBtnClick() {
        Debug.Log("onStopRecordBtnClick");
        _eItf.StopRecord();
    }

    // export record to desktop
    public async void onExportRecordBtnClick()
    {
        Debug.Log("onExportRecordBtnClick");
        string _recordId = RecordId.text;
        if (string.IsNullOrEmpty(_recordId))
        {
            UnityEngine.Debug.LogError("The recordId must not be empty. Please export with a valid recordId.");
            return;
        }

        // Determine the folder path for export. Default to Desktop folder (or persistent data path on mobile)
        string folderPath;
    #if UNITY_ANDROID
        // On mobile, use persistent data path
        folderPath = Application.persistentDataPath;
    #elif UNITY_IOS
        // On iOS do not need to specify the folder.
        // Cortex exports the data to the "Documents" folder of the current application.
        // Empty path lets the plugin handle default Documents folder location
        folderPath = "";
    #else
        // On desktop, use Desktop folder
        folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    #endif
        List<string> recordsToExport = new List<string> { _recordId };
        List<string> streamTypes = new List<string> { "EEG", "MOTION" }; // Specify the stream types you want to export
        string format = "CSV"; // or "CSV", "EDFPLUS", "BDFPLUS"
        string version = "V2"; // Optional, specify if needed

        try
        {
            ExportRecordResult result = await _eItf.ExportRecordAsync(recordsToExport, folderPath, streamTypes, format, version);

            string msgLog = "Export records - success: " + result.SuccessRecordIds.Count + ", failed: " + result.FailedRecords.Count + "\n";
            foreach (string recordId in result.SuccessRecordIds)
            {
                msgLog += "Success: " + recordId + "\n";
            }
            foreach (ExportRecordFailure failure in result.FailedRecords)
            {
                msgLog += "Failed: " + failure.RecordId + " (code " + failure.Code + "): " + failure.Message + "\n";
            }
            MessageLog.text = msgLog;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("onExportRecordBtnClick failed: " + ex.Message);
            MessageLog.text = "Export records failed: " + ex.Message;
        }
    }   

    // query records with a startDatetime range
    public async void onQueryRecordBtnClick()
    {
        Debug.Log("onQueryRecordBtnClick");
        // demo to query with startDatetime range, the from and to are in ISO 8601 format
        JObject query = new JObject(
            new JProperty("startDatetime", new JObject(
                new JProperty("from", "2026-01-06T16:32:50.572490+07:00"),
                new JProperty("to", "2026-08-06T16:32:50.572490+07:00")
            ))
        );

        // demo to query with keywords which can be record title or record note
        // JObject query = new JObject(
        //     new JProperty("keyword", "123abc")
        // );

        // If you want to query records of owner but not created by the application,
        // you can set the licenseId to filter the records.
        // If you set the licenseId, then you can set this parameter to further filter the records by application.
        // JObject query = new JObject(
        //     new JProperty("applicationId", "com.emotivid.appname"),
        //     new JProperty("licenseId", "xxx--yyy")
        // );

        try
        {
            List<Record> records = await _eItf.QueryRecords(query, null, 100, 0, true, true);

            string msgLog = "Query records: found " + records.Count + " record(s).\n";
            foreach (Record record in records)
            {
                msgLog += "RecordId: " + record.Uuid + ", Title: " + record.Title + ", StartTime: " + record.StartDateTime +
                          ", EndTime: " + record.EndDateTime + ", SyncStatus: " + record.SyncStatus + "\n";
            }
            MessageLog.text = msgLog;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("onQueryRecordBtnClick failed: " + ex.Message);
            MessageLog.text = "Query records failed: " + ex.Message;
        }
    }

    public void onInjectMarkerBtnClick()
    {
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
        _eItf.LoadProfile(ProfileName.text);
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
        Debug.Log("onStartMCTrainingBtnClick " + ActionNameList.captionText.text);
        _eItf.StartMCTraining(ActionNameList.captionText.text);
    }

    /// <summary>
    /// accept a mental command training
    /// </summary>
    public void onAcceptMCTrainingBtnClick() {
        Debug.Log("onAcceptMCTrainingBtnClick");
        _eItf.AcceptMCTraining();
    }

    /// <summary>
    /// reject a mental command training
    /// </summary>
    public void onRejectMCTrainingBtnClick() {
        Debug.Log("onRejectMCTrainingBtnClick");
        _eItf.RejectMCTraining();
    }

    /// <summary>
    /// erase a mental command training
    /// </summary>
    public void onEraseMCTrainingBtnClick() {
        Debug.Log("onEraseMCTrainingBtnClick");
        _eItf.EraseMCTraining(ActionNameList.captionText.text);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        if (_isEmotivUnityItfInitialized)
            _eItf.Stop();
    }

    private void CacheButtonReferences()
    {
        _signInBtn = GameObject.Find("AuthenPart").transform.Find("signInBtn").GetComponent<Button>();
        _signOutBtn = GameObject.Find("AuthenPart").transform.Find("signOutBtn").GetComponent<Button>();
        _createSessionBtn = GameObject.Find("SessionPart").transform.Find("createSessionBtn").GetComponent<Button>();
        _queryHeadsetBtn = GameObject.Find("SessionPart").transform.Find("queryHeadset").GetComponent<Button>();
        _startRecordBtn = GameObject.Find("RecordPart").transform.Find("startRecordBtn").GetComponent<Button>();
        _subscribeBtn = GameObject.Find("SubscribeDataPart").transform.Find("subscribeBtn").GetComponent<Button>();
        _unsubscribeBtn = GameObject.Find("SubscribeDataPart").transform.Find("unsubscribeBtn").GetComponent<Button>();
        _loadProfileBtn = GameObject.Find("TrainingPart").transform.Find("loadProfileBtn").GetComponent<Button>();
        _unloadProfileBtn = GameObject.Find("TrainingPart").transform.Find("unloadProfileBtn").GetComponent<Button>();
        _saveProfileBtn = GameObject.Find("TrainingPart").transform.Find("saveProfileBtn").GetComponent<Button>();
        _rejectTrainingBtn = GameObject.Find("TrainingPart").transform.Find("rejectTrainingBtn").GetComponent<Button>();
        _eraseTrainingBtn = GameObject.Find("TrainingPart").transform.Find("eraseTrainingBtn").GetComponent<Button>();
        _acceptTrainingBtn = GameObject.Find("TrainingPart").transform.Find("acceptTrainingBtn").GetComponent<Button>();
        _startTrainingBtn = GameObject.Find("TrainingPart").transform.Find("startTrainingBtn").GetComponent<Button>();
        _stopRecordBtn = GameObject.Find("RecordPart").transform.Find("stopRecordBtn").GetComponent<Button>();
        _injectMarkerBtn = GameObject.Find("RecordPart").transform.Find("injectMarkerBtn").GetComponent<Button>();
        _exportRecordBtn = GameObject.Find("RecordPart").transform.Find("exportRecordBtn").GetComponent<Button>();
        _queryRecordBtn = GameObject.Find("RecordPart").transform.Find("queryRecordBtn").GetComponent<Button>();
    }

    private void CheckButtonsInteractable()
    {
        #if USE_EMBEDDED_LIB || UNITY_ANDROID || UNITY_IOS
        ConnectToCortexStates connectionState =  _eItf.GetConnectToCortexState();
        _signInBtn.interactable = (connectionState == ConnectToCortexStates.Login_notYet);
        _signOutBtn.interactable = (connectionState > ConnectToCortexStates.Login_notYet);
        #else
        _signInBtn.interactable = false;
        _signOutBtn.interactable = false;
        #endif

        _createSessionBtn.interactable = _eItf.IsAuthorizedOK;
        _queryHeadsetBtn.interactable = _eItf.IsAuthorizedOK;
        _startRecordBtn.interactable = _eItf.IsSessionCreated;
        _stopRecordBtn.interactable = _eItf.IsRecording;
        _exportRecordBtn.interactable = _eItf.IsAuthorizedOK && !_eItf.IsRecording;
        _queryRecordBtn.interactable = _eItf.IsAuthorizedOK;
        _injectMarkerBtn.interactable = _eItf.IsRecording;
        _subscribeBtn.interactable = _eItf.IsSessionCreated;
        _unsubscribeBtn.interactable = _eItf.IsSessionCreated;
        _loadProfileBtn.interactable = _eItf.IsSessionCreated;
        _saveProfileBtn.interactable = _eItf.IsProfileLoaded;
        _startTrainingBtn.interactable = _eItf.IsProfileLoaded;
        _rejectTrainingBtn.interactable = _eItf.IsProfileLoaded;
        _eraseTrainingBtn.interactable = _eItf.IsProfileLoaded;
        _acceptTrainingBtn.interactable = _eItf.IsProfileLoaded;
        _unloadProfileBtn.interactable = _eItf.IsProfileLoaded;
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

