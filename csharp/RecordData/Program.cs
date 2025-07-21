using CortexAccess;
using System;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;

namespace RecordData
{
    class Program
    {
        const string WantedHeadsetId = ""; // if you want to connect to specific headset, put headset id here. For example: "EPOCX-71D833AC"

        private static int _recordNo = 1;
        private static string _recentRecordId = ""; // keep track of the most recent record ID created
        private static RecordManager _recordManager;
        private static AutoResetEvent _readyForRecordDataEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("RECORD DATA DEMO");
            Console.WriteLine("Please wear Headset with good signal!!!");


            _recordManager = new RecordManager();
            _recordManager.sessionCreateOk += OnSessionCreatedOk;
            _recordManager.DataPostProcessingFinished += OnDataPostProcessingFinished;
            _recordManager.ExportRecordsFinished += onExportRecordsFinished;

            Console.WriteLine("Prepare to record Data");
            // Start
            _recordManager.Start(WantedHeadsetId);

            if (_readyForRecordDataEvent.WaitOne(50000))
            {
                Console.WriteLine("Press C to create record");
                Console.WriteLine("Press S to stop record");
                Console.WriteLine("Press Q to query record");
                Console.WriteLine("Press D to delete first record Id from recording list");
                Console.WriteLine("Press E to export the most recently stopped record.");
                Console.WriteLine("Press U to update record");
                Console.WriteLine("Press H to show all commands");
                Console.WriteLine("Press Esc to quit");
                Console.WriteLine("Ignore Tab, Enter, Spacebar and Backspace key");

                ConsoleKeyInfo keyInfo;
                while (true)
                {
                    keyInfo = Console.ReadKey(true);
                    Console.WriteLine(keyInfo.KeyChar.ToString() + " has pressed");
                    if (keyInfo.Key == ConsoleKey.S)
                    {
                        // Stop Record
                        Console.WriteLine("Stop Record");
                        _recordManager.StopRecord();
                    }
                    else if (keyInfo.Key == ConsoleKey.C)
                    {
                        // Create Record
                        string title = "RecDemo-" + _recordNo;
                        Console.WriteLine("Create Record" + title);
                        _recordManager.StartRecord(title);
                        _recordNo++;
                    }
                    else if (keyInfo.Key == ConsoleKey.U)
                    {
                        // Update for first record in the list. Or you can update for any record has recordId
                        if (RecordManager.RecordLists.Count > 0)
                        {
                            Record lastRecord = RecordManager.RecordLists[0];
                            string recordId = lastRecord.Uuid;
                            string description = "description for RecDemo";
                            List<string> tags = new List<string>() { "demo1", "demo2" };
                            _recordManager.UpdateRecord(recordId, description, tags);
                        }
                        else
                        {
                            Console.WriteLine("No recording list. Please create records and queryRecords first");
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.Q)
                    {
                        // query param
                        JObject query = new JObject();

                        //order
                        JArray orderBy = new JArray();
                        JObject eleOrder = new JObject();
                        eleOrder.Add("startDatetime", "DESC" );
                        orderBy.Add(eleOrder);
                        // limit
                        int limit = 5; // number of maximum record return
                        int offset = 0;
                        _recordManager.QueryRecords(query, orderBy, limit, offset); 
                    }
                    else if (keyInfo.Key == ConsoleKey.D)
                    {
                        // Delete first Record
                        if (RecordManager.RecordLists.Count > 0)
                        {
                            Record lastRecord = RecordManager.RecordLists[0];
                            string recordId = lastRecord.Uuid;
                            Console.WriteLine("Delete Record" + recordId);
                            List<string> wantedDeleteRecord = new List<string>();
                            wantedDeleteRecord.Add(recordId);
                            _recordManager.DeleteRecords(wantedDeleteRecord);
                        }
                        else
                        {
                            Console.WriteLine("Please queryRecords first before call deleteRecord which delete first record in Lists");
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.E)
                    {
                        // Export Record
                        if (string.IsNullOrEmpty(_recentRecordId))
                        {
                            Console.WriteLine("No record available to export. Please create a record first.");
                        }
                        else
                        {
                            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                            List<string> recordsToExport = new List<string> { _recentRecordId };
                            List<string> streamTypes = new List<string> { "EEG", "MOTION" }; // Specify the stream types you want to export
                            string format = "CSV"; // or "CSV", "EDFPLUS", "BDFPLUS"
                            string version = "V2"; // Optional, specify if needed
                            _recordManager.ExportRecord(recordsToExport, folderPath, streamTypes, format, version);
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.H)
                    {
                        Console.WriteLine("Press certain key to inject marker");
                        Console.WriteLine("Press C to create record");
                        Console.WriteLine("Press S to stop record");
                        Console.WriteLine("Press Q to query record");
                        Console.WriteLine("Press D to delete record");
                        Console.WriteLine("Press U to update record");
                        Console.WriteLine("Press E to export the most recently stopped record");
                        Console.WriteLine("Press H to show all commands");
                        Console.WriteLine("Press Esc to quit");
                    }
                    else if (keyInfo.Key == ConsoleKey.Tab) continue;
                    else if (keyInfo.Key == ConsoleKey.Backspace) continue;
                    else if (keyInfo.Key == ConsoleKey.Enter) continue;
                    else if (keyInfo.Key == ConsoleKey.Spacebar) continue;
                    else if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        _recordManager.Stop();
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Unsupported key");
                    }
                }

                Thread.Sleep(10000);
            }
            else
            {
                Console.WriteLine("Unable to prepare for recording data. Please ensure your headset is available and ready to connect, then try again.");
            }
        }

        private static void onExportRecordsFinished(object sender, MultipleResultEventArgs e)
        {
            // extract the result from e.Result
            // get successful list
            JArray successfulList = e.SuccessList;
            // check _recentRecordId is in the successful list
            bool isExportedSuccess = false;
            if (successfulList != null && successfulList.Count > 0)
            {
                foreach (var record in successfulList)
                {
                    if (record is JObject recordObj && recordObj["recordId"]?.ToString() == _recentRecordId)
                    {
                        isExportedSuccess = true;
                        break;
                    }
                }
            }

            if (!isExportedSuccess)
            {
                Console.WriteLine("Export failed for record with ID: " + _recentRecordId);
            }
            else
            {
                Console.WriteLine("Export finished for record with ID: " + _recentRecordId);
            }

        }

        private static void OnSessionCreatedOk(object sender, bool isOK)
        {
            if (isOK)
            {
                Console.WriteLine("SessionCreatedOK");
                _readyForRecordDataEvent.Set();
            }
        }
        private static void OnDataPostProcessingFinished(object sender, string recordId)
        {
            Console.WriteLine("Data post processing finished for record: " + recordId +
                ". You can now export the record.");
            _recentRecordId = recordId; // Update the most recent record ID
        }
    }
}
