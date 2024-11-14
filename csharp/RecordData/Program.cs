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
        private static RecordManager _recordManager;
        private static AutoResetEvent _readyForRecordDataEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("RECORD DATA DEMO");
            Console.WriteLine("Please wear Headset with good signal!!!");


            _recordManager = new RecordManager();
            _recordManager.sessionCreateOk += OnSessionCreatedOk;

            Console.WriteLine("Prepare to record Data");
            // Start
            _recordManager.Start(WantedHeadsetId);

            if (_readyForRecordDataEvent.WaitOne(50000))
            {
                Console.WriteLine("Press C to create record");
                Console.WriteLine("Press S to stop record");
                Console.WriteLine("Press Q to query record");
                Console.WriteLine("Press D to delete first record Id from recording list");
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
                    else if (keyInfo.Key == ConsoleKey.H)
                    {
                        Console.WriteLine("Press certain key to inject marker");
                        Console.WriteLine("Press C to create record");
                        Console.WriteLine("Press S to stop record");
                        Console.WriteLine("Press Q to query record");
                        Console.WriteLine("Press D to delete record");
                        Console.WriteLine("Press U to update record");
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
                Console.WriteLine("The preparation for injecting marker is unsuccessful. Please try again");
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
    }
}
