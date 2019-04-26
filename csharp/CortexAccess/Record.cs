using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CortexAccess
{
    public class Record
    {
        private string _uuid;
        private string _applicationId;
        private string _licenseId;
        private string _title;
        private string _description;
        private string _startDateTime;
        private string _endDateTime;
        private JArray _markers;
        private List<string> _tags;

        // Properties
        public string Uuid
        {
            get
            {
                return _uuid;
            }

            set
            {
                _uuid = value;
            }
        }

        public string ApplicationId
        {
            get
            {
                return _applicationId;
            }

            set
            {
                _applicationId = value;
            }
        }

        public string LicenseId
        {
            get
            {
                return _licenseId;
            }

            set
            {
                _licenseId = value;
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                _title = value;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                _description = value;
            }
        }

        public string StartDateTime
        {
            get
            {
                return _startDateTime;
            }

            set
            {
                _startDateTime = value;
            }
        }

        public string EndDateTime
        {
            get
            {
                return _endDateTime;
            }

            set
            {
                _endDateTime = value;
            }
        }

        public JArray Markers
        {
            get
            {
                return _markers;
            }

            set
            {
                _markers = value;
            }
        }

        public List<string> Tags
        {
            get
            {
                return _tags;
            }

            set
            {
                _tags = value;
            }
        }
        //Constructor
        public Record()
        {

        }
        public Record(JObject obj)
        {
            _uuid = (string)obj["uuid"];
            _licenseId = (string)obj["licenseId"];
            _applicationId = (string)obj["applicationId"];
            _title = (string)obj["title"];
            _description = (string)obj["description"];
            _startDateTime = (string)obj["startDatetime"];
            _endDateTime = (string)obj["endDatetime"];
            _markers = (JArray)obj["markers"];
            _tags = obj["tags"].ToObject<List<string>>();
        }
        public void PrintOut()
        {
            Console.WriteLine("id: " + _uuid + ", title: " + _title + ", startDatetime: " + _startDateTime + ", endDatetime: " + _endDateTime);
        }

    }
}
