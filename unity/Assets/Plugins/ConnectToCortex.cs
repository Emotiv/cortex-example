using UnityEngine;
using UnityEditor;
using EmotivUnityPlugin;
using System.IO;

public class ConnectToCortex : MonoBehaviour
{
    DataStreamManager _dataStream = DataStreamManager.Instance;

    Logger _logger = Logger.Instance;

    bool _isLicenseValid           = false;
    bool _isCheckedLicenseValidDay = false;

    void Start()
    {
        // set Application configuration
        _dataStream.SetAppConfig(AppConfig.ClientId, AppConfig.ClientSecret,
                                 AppConfig.AppVersion, AppConfig.AppName, AppConfig.AppName, AppConfig.AppUrl,
                                 EmotivAppslicationPath());
        
        // Init logger
        _logger.Init();
        
        Debug.Log("Configure BrainViz - PRODUCT SERVER - version: " +AppConfig.AppVersion);
        
        // start App
        _dataStream.StartAuthorize();
    }
    
    float _timerCounter_CQ              = 0;
    float _timerCounter_queryHeadset    = 0;
    float _timerCounter_LicenseValidDay = 0;

    const float TIME_UPDATE_CQ          = 0.5f;
    const float TIME_QUERY_HEADSET      = 2.0f;
    const float TIME_LICENSE_VALID_DAY  = 1.0f;

    void Update()
    {
        if(!_isCheckedLicenseValidDay) {
            _timerCounter_LicenseValidDay += Time.deltaTime;
            if (_timerCounter_LicenseValidDay < TIME_LICENSE_VALID_DAY) 
                return;

            _timerCounter_LicenseValidDay -= TIME_LICENSE_VALID_DAY;

            double nDay = DataProcessing.Instance.GetLicenseRemainingDay();
            if(nDay == -1) {
                return;
            } else if (nDay == 0) {
                _isLicenseValid = false;
            } else if (nDay > 0) {
                _isLicenseValid = true;
            }

            _isCheckedLicenseValidDay = true;
        }

        if (!_isLicenseValid)
            return;

        _timerCounter_CQ += Time.deltaTime;
        if (_timerCounter_CQ > TIME_UPDATE_CQ) {
            _timerCounter_CQ -= TIME_UPDATE_CQ;
            DataProcessing.Instance.updateContactQuality();
        }

        _timerCounter_queryHeadset += Time.deltaTime;
        if (_timerCounter_queryHeadset > TIME_QUERY_HEADSET) {
            _timerCounter_queryHeadset -= TIME_QUERY_HEADSET;
            DataProcessing.Instance.Process();
        }
    }
    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        _dataStream.Stop();
    }

    string EmotivAppslicationPath() {
        string path = Application.dataPath;
        string newPath = "";
        // Debug.Log("ConnectToCortex: applicationPath: before " + path);
        if (Application.platform == RuntimePlatform.OSXPlayer) {
            newPath = Path.GetFullPath(Path.Combine(path, @"../../"));
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer) {
            newPath = Path.GetFullPath(Path.Combine(path, @"../"));
        }
        // Debug.Log("ConnectToCortex: applicationPath: after " + newPath);
        return newPath;
    }
}
