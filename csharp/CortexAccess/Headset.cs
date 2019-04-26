using Newtonsoft.Json.Linq;
using System.Collections;
using System;

namespace CortexAccess
{
    public class Headset
    {
        private string _headsetID;
        private string _status;
        private string _serialId;
        private string _firmwareVersion;
        private string _dongleSerial;
        private ArrayList _sensors;
        private ArrayList _motionSensors;
        private JObject _settings;
        private string _connectedBy;
        private string _mode;

        // Contructor
        public Headset()
        {
        }
        public Headset (JObject jHeadset)
        {
            HeadsetID = (string)jHeadset["id"];
            Status = (string)jHeadset["status"];
            FirmwareVersion = (string)jHeadset["firmware"];
            DongleSerial = (string)jHeadset["dongle"];
            Sensors = new ArrayList();
            
            foreach (JToken sensor in (JArray)jHeadset["sensors"])
            {
                Sensors.Add(sensor.ToString());
            }
            MotionSensors = new ArrayList();
            foreach (JToken sensor in (JArray)jHeadset["motionSensors"])
            {
                MotionSensors.Add(sensor.ToString());
            }
            Mode = (string)jHeadset["mode"];
            ConnectedBy = (string)jHeadset["connectedBy"];
            Settings = (JObject)jHeadset["settings"];
        }

        // Properties
        public string HeadsetID
        {
            get
            {
                return _headsetID;
            }

            set
            {
                _headsetID = value;
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }

        public string SerialId
        {
            get
            {
                return _serialId;
            }

            set
            {
                _serialId = value;
            }
        }

        public string FirmwareVersion
        {
            get
            {
                return _firmwareVersion;
            }

            set
            {
                _firmwareVersion = value;
            }
        }

        public string DongleSerial
        {
            get
            {
                return _dongleSerial;
            }

            set
            {
                _dongleSerial = value;
            }
        }

        public ArrayList Sensors
        {
            get
            {
                return _sensors;
            }

            set
            {
                _sensors = value;
            }
        }

        public ArrayList MotionSensors
        {
            get
            {
                return _motionSensors;
            }

            set
            {
                _motionSensors = value;
            }
        }

        public JObject Settings
        {
            get
            {
                return _settings;
            }

            set
            {
                _settings = value;
            }
        }

        public string ConnectedBy
        {
            get
            {
                return _connectedBy;
            }

            set
            {
                _connectedBy = value;
            }
        }

        public string Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                _mode = value;
            }
        }
    }
}
