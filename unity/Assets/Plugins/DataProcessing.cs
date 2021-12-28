/*
 * Class: Data Processing
 * 
 * Created by hoang@emotiv.com. 2014
 * Last Changed: 03/2020
 * NOTED: Don't know why a event from EmotivDataStream cannot call to tough to a UI action. 
 *        So we need DataProcessing detect the event then call another function to a UI action.
*/

using System;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
using EmotivUnityPlugin;

public struct DeviceScan
{
    public int index;	
    public string name;
}

public class DataProcessing
{
    static DataProcessing _instance = null;
    static readonly object _object = new object();
    const int NUMBER_OF_SENSORS = 16; // 14 channel EEG, CMS, DRL

    // -1: haven't checked yet
    // 0: license expried
    // > 0 and <= 7. Free license
    // a big number. License valid
    double _nRemainingDay = -1;
    int _lastNSamples = 0;
    ContactQualityValue[] _contactQualityData = new ContactQualityValue[NUMBER_OF_SENSORS];
    double _lastCQOverAll = 0;
    Dictionary<string, Headset> _headsetList = new Dictionary<string, Headset>();

    RecordManager _recordManager = RecordManager.Instance;

    public event EventHandler onHeadsetChange;
    public event EventHandler onCurrHeadsetRemoved;
    public event EventHandler onDetectManyDevices;
    public event EventHandler<string> HeadsetConnected;
    public event EventHandler<string> HeadsetConnectFail
    {
        add { DataStreamManager.Instance.HeadsetConnectFail += value; }
        remove { DataStreamManager.Instance.HeadsetConnectFail -= value; }
    }
    
    Headset _curHeadsetObjectConnected = null;

    // This value is for fix issue that noted in description of the class. 
    // if it is null. the connected event is processed. or haven't no event yet. 
    // if it is not null. the connected event is waiting for process. 
    string _headsetIdConnected = null;
    bool _enableQueryHeadset  = true;
    bool _isConnect           = false;    
    int  _buferSize           = 0;
    
    Channels[] _channelList =  {
        Channels.AF3, Channels.AF4, Channels.F3,
        Channels.F4,  Channels.F7,  Channels.F8,
        Channels.FC5, Channels.FC6, Channels.O1,
        Channels.O2,  Channels.P7,  Channels.P8,
        Channels.T7,  Channels.T8
    };
    
    public DeviceScan[] deviceNames;

    public DataProcessing()
    {
        DataStreamManager.Instance.SessionActivatedOK += OnSessionActivatedOK;
        DataStreamManager.Instance.LicenseValidTo  += onLicenseValidTo;
    }
    ~DataProcessing()
    {
    }

    static public DataProcessing Instance
    {
        get {
            if (_instance == null)  {
                _instance = new DataProcessing();
            }
            return _instance;
        }
    }

    public Channels[] GetEEGChannelList()
    {
        lock (_object) {
            return _channelList;
        }
    }	

    public void SetConnectedHeadset (Headset headsetInfos)
    {
        lock (_object) {
            // Debug.Log("======== SetConnectedHeadset " + headsetInfos.HeadsetID);
            _curHeadsetObjectConnected = headsetInfos;
        }
    }

    // disable query headset while connecting to a headset
    // and enable it again after done connecting process(success or failed)
    public void EnableQueryHeadset(bool enable)
    {
        lock (_object) {
            _enableQueryHeadset = enable;
        }
    }

    public int GetNumCQChannelMax() => NUMBER_OF_SENSORS;    

    void headsetListUpdate()
    {				              
        if (onHeadsetChange != null)
            onHeadsetChange(null, null);		
    }

    public void updateContactQuality() 
    {
        lock (_object) {
            if(_curHeadsetObjectConnected == null) {
                _lastCQOverAll = 0;
                return;
            }

            for (int i = 0; i < _contactQualityData.Length; i++) {
                _contactQualityData[i] = ContactQualityValue.NO_SIGNAL;
            }

            // Only for TEST: Subscribe data: motions, power band , peformance metrics
            // Will be remove when release
            // print motions
            // print header motion
            // string motionChansStr = "";
            // string motionDataStr = "";
            // foreach (var chan in EmotivDataStream.Instance.GetMotionChannels()) {
            //     motionChansStr += ChannelStringList.ChannelToString(chan) + " , ";
                
            //     double[] data = EmotivDataStream.Instance.GetMotionData(chan);
            //     double dataTmp = 0;
            //     if (data != null && data.Length > 0)
            //         dataTmp = data[0];
            //     motionDataStr += dataTmp.ToString() + " , ";
            // }
            // UnityEngine.Debug.Log(" Motion Chanels streams " + motionChansStr);
            // UnityEngine.Debug.Log(" Motion Data streams " + motionDataStr);
            // print power band data
            // string powListsStr = "";
            // foreach (var ele in EmotivDataStream.Instance.GetBandPowerLists()) {
            //     powListsStr += ele + " , ";
            // }
            // UnityEngine.Debug.Log(" Power band lists streams " + powListsStr);

            // if (EmotivDataStream.Instance.GetNumberPowerBandSamples() > 0) {
            //     double alphaAF3 = EmotivDataStream.Instance.GetAlphaData(Channel_t.CHAN_AF3);
            //     UnityEngine.Debug.Log(" Alpha AF3 Data " + alphaAF3.ToString());
            // }
            // else
            // {
            //     UnityEngine.Debug.Log(" No Alpha AF3 Data ");
            // }

            // Peformance metrics data
            // string pmListsStr = "";
            // string pmDataStr = "";
            // foreach (var ele in EmotivDataStream.Instance.GetPMLists()) {
            //     pmListsStr      += ele + " , ";
            //     double pmData   = EmotivDataStream.Instance.GetPMData(ele);
            //     pmDataStr       += pmData.ToString() + " , ";
            // }
            // UnityEngine.Debug.Log(" Peformance metrics lists streams " + pmListsStr);
            // UnityEngine.Debug.Log(" Peformance metrics data " + pmDataStr);


            int cqSize = DataStreamManager.Instance.GetNumberCQSamples();

            // Currently , we only support insight, epoc, epoc+, epocX . Other headsets will be supported in next version
            if (!Utils.IsInsightType(_curHeadsetObjectConnected.HeadsetType))
            {
                _contactQualityData[(int)Channels.AF3] = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_AF3);
                _contactQualityData[(int)Channels.AF4] = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_AF4);
                _contactQualityData[(int)Channels.F3]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_F3);
                _contactQualityData[(int)Channels.F4]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_F4);
                _contactQualityData[(int)Channels.F7]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_F7);
                _contactQualityData[(int)Channels.F8]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_F8);
                _contactQualityData[(int)Channels.FC5] = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_FC5);
                _contactQualityData[(int)Channels.FC6] = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_FC6);
                _contactQualityData[(int)Channels.O1]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_O1);
                _contactQualityData[(int)Channels.O2]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_O2);
                _contactQualityData[(int)Channels.P7]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_P7);
                _contactQualityData[(int)Channels.P8]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_P8);
                _contactQualityData[(int)Channels.T7]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_T7);
                _contactQualityData[(int)Channels.T8]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_T8);
            } 
            else { // header: Battery, Signal, AF3, T7, Pz, T8, AF4, OVERALL,
                // We keep using Channels because this app still follow it
                _contactQualityData[(int)Channels.AF3] = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_AF3);
                _contactQualityData[(int)Channels.T7]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_T7);
                _contactQualityData[(int)Channels.O1]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_Pz);
                _contactQualityData[(int)Channels.T8]  = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_T8);
                _contactQualityData[(int)Channels.AF4] = (ContactQualityValue)(int)DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_AF4);
            }

            _lastCQOverAll = DataStreamManager.Instance.GetContactQuality(Channel_t.CHAN_CQ_OVERALL);

            _contactQualityData[(int)Channels.CMS] = ContactQualityValue.VERY_BAD;
            _contactQualityData[(int)Channels.DRL] = ContactQualityValue.VERY_BAD;
            for (int i = 1; i < _contactQualityData.Length; i++)
            {
                if (_contactQualityData[i] > ContactQualityValue.VERY_BAD) {
                    _contactQualityData[(int)Channels.CMS] = ContactQualityValue.GOOD;
                    _contactQualityData[(int)Channels.DRL] = ContactQualityValue.GOOD;
                    break;
                }
            }
        }
    }

    public bool IsHeadsetConnected() {
        lock (_object) {
            return _isConnect;
        }
    }

    public int GetNumberEegSamples()
    {
        return DataStreamManager.Instance.GetNumberEEGSamples();
    }

    //TODO: Improve it next time
    public int GetLastNumberEEGSamples()
    {
        return _lastNSamples;
    }

    public double[][] GetDataPoker()
    {
        lock (_object) {
            _lastNSamples = DataStreamManager.Instance.GetNumberEEGSamples();
            if(_lastNSamples <= 0)
                return null;

            if(_curHeadsetObjectConnected == null)
                return null;
            
            double[][] buf = new double[_channelList.Length][];

            buf[(int)Channels.AF3] = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_AF3);
            buf[(int)Channels.T7]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_T7);
            buf[(int)Channels.T8]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_T8);
            buf[(int)Channels.AF4] = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_AF4);

            if (!Utils.IsInsightType(_curHeadsetObjectConnected.HeadsetType))
            {
                buf[(int)Channels.O1]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_O1);
                buf[(int)Channels.F7]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_F7);
                buf[(int)Channels.F3]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_F3);
                buf[(int)Channels.FC5] = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_FC5);
                buf[(int)Channels.P7]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_P7);
                buf[(int)Channels.O2]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_O2);
                buf[(int)Channels.P8]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_P8);
                buf[(int)Channels.FC6] = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_FC6);
                buf[(int)Channels.F4]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_F4);
                buf[(int)Channels.F8]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_F8);
            } 
            else {
                buf[(int)Channels.O1]  = DataStreamManager.Instance.GetEEGData(Channel_t.CHAN_Pz);
            }

            if (buf[(int)Channels.AF3] == null)
                return null;

            // UnityEngine.Debug.Log(buf[(int)Channels.AF3][0] + ", " + 
            // buf[(int)Channels.T7][0] + ", " + 
            // buf[(int)Channels.T8][0] + ", " + 
            // buf[(int)Channels.AF4][0] + ", " + 
            // buf[(int)Channels.O1][0] + ", " + 
            // buf[(int)Channels.F7][0] + ", " + 
            // buf[(int)Channels.F3][0] + ", " + 
            // buf[(int)Channels.FC5][0] + ", " + 
            // buf[(int)Channels.P7][0] + ", " + 
            // buf[(int)Channels.O2][0] + ", " + 
            // buf[(int)Channels.P8][0] + ", " + 
            // buf[(int)Channels.FC6][0] + ", " + 
            // buf[(int)Channels.F4][0] + ", " + 
            // buf[(int)Channels.F8][0]);

            // double[] counterList = EmotivDataStream.Instance.GetEEGData(Channel_t.CHAN_COUNTER);
            // for (int i = 0; i < _lastNSamples; i++) {
            //     UnityEngine.Debug.Log("====== " + _lastNSamples + ", " + counterList[i]);
            // }

            return buf;
        }
    }

    public ContactQualityValue[] GetContactQuality()
    {
        lock (_object) {
            return _contactQualityData;
        }
    }

    public ContactQualityValue GetContactQuality(Channels channel)
    {
        lock (_object) {
            return _contactQualityData[(int)channel];
        }
    }

    public Dictionary<string, Headset> GetHeadsetList()
    {
        lock (_object) {
            return _headsetList;
        }
    }

    public double GetCQOverAll()
    {
        lock (_object) {
            return _lastCQOverAll;
        }
    }

    // return number of headset discovered
    int queryHeadset ()
    {
        if (!_enableQueryHeadset)
            return 0;

        List<Headset> detectedHeadset = DataStreamManager.Instance.GetDetectedHeadsets();

        _headsetList.Clear();
        foreach (var item in detectedHeadset) {
            _headsetList[item.HeadsetID] = item;
        }

        // Detect the headset is disconnected
        if(_curHeadsetObjectConnected != null)
        {
            bool isDisconnected = false;
            if (!_headsetList.ContainsKey(_curHeadsetObjectConnected.HeadsetID)) {
                isDisconnected = true;
            } else {
                isDisconnected = (_headsetList[_curHeadsetObjectConnected.HeadsetID].Status == HeadsetConnectionStatus.DISCOVERED);
            }

            if (isDisconnected) {
                Debug.Log("DataProcessing:queryHeadset - Disconnected the headset");

                if (onCurrHeadsetRemoved != null)
                    onCurrHeadsetRemoved(null, null);

                _curHeadsetObjectConnected = null;
            }
        }

        if (_headsetIdConnected != null && _headsetList.ContainsKey(_headsetIdConnected)) {
            if(_headsetList[_headsetIdConnected].Status == HeadsetConnectionStatus.CONNECTED) {
                Debug.Log("DataProcessing:queryHeadset - emit headset connected");
                HeadsetConnected(this, _headsetIdConnected);
                _headsetIdConnected = null;
            } else if (_headsetList[_headsetIdConnected].Status == HeadsetConnectionStatus.DISCOVERED) {
                Debug.Log("DataProcessing:queryHeadset - remove event headset connected");
                // _headsetIdConnected = null;
            } else {
                Debug.Log("DataProcessing:queryHeadset - the headset still connecting");
            }
        }

        headsetListUpdate();

        return detectedHeadset.Count;
    }

    bool checkHeadsetConnected()
    {
        if (_curHeadsetObjectConnected == null || _curHeadsetObjectConnected.HeadsetID == "") {
            _isConnect = false;
        } else {
            _isConnect = true;
        }

        return _isConnect;
    }

    public void Process()
    {
        lock (_object) {

            if (queryHeadset () <= 0)
                return;

            if (!checkHeadsetConnected()) {
                for (int i = 0; i < _contactQualityData.Length; i++) {
                    _contactQualityData[i] = ContactQualityValue.NO_SIGNAL;
                }
                return;
            }
        }
    }

    // Slots
    private void OnSessionActivatedOK(object sender, string headsetID)
    {
        lock (_object) {
            _headsetIdConnected = headsetID;

            // TEST Start Record
            // _recordManager.StartRecord("tung260420", "demoUnity");
        }
    }

    private void onLicenseValidTo(object sender, DateTime validToDate)
    {
        DateTime curUTC_Now = DateTime.UtcNow;
        System.TimeSpan diffDate = validToDate - curUTC_Now;
        double nDay = (double)((int)(diffDate.TotalDays * 10)) / 10;
        if (nDay < 0)
            _nRemainingDay = 0;
        else 
            _nRemainingDay = nDay;

        // Debug.Log("ValidToDate = " + validToDate + ", UTC = " + curUTC_Now + ", nDay = " + nDay);
    }

    // -1: haven't checked yet
    // 0: license expried
    // > 0 and <= 7. Free license
    // a big number. License valid
    public double GetLicenseRemainingDay() => _nRemainingDay;
}

