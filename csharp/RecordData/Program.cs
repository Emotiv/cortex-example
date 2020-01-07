using CortexAccess;
using System;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;

namespace RecordData
{
    class Program
    {
        private static string _cortexToken;
        private static string _sessionId;
        private static string _currRecordId;// current recordID
        private static List<Record> _recordLists = new List<Record>();
        private static int _recordNo = 1;
        private static Authorizer _authorizer = new Authorizer();
        private static HeadsetFinder _headsetFinder = new HeadsetFinder();
        private static SessionCreator _sessionCreator = new SessionCreator();
        private static CortexClient _ctxClient;
        private static AutoResetEvent _readyForRecordDataEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("RECORD DATA DEMO");
            Console.WriteLine("Please wear Headset with good signal!!!");


            _ctxClient = CortexClient.Instance;
            _ctxClient.OnCreateRecord += RecordCreatedOK;
            _ctxClient.OnStopRecord += RecordStoppedOK;
            _ctxClient.OnErrorMsgReceived += MessageErrorRecieved;
            _ctxClient.OnQueryRecords += QueryRecordsOK;
            _ctxClient.OnDeleteRecords += DeleteRecordOK;
            _ctxClient.OnUpdateRecord += RecordUpdatedOK;

            _authorizer.OnAuthorized += AuthorizedOK;
            _headsetFinder.OnHeadsetConnected += HeadsetConnectedOK;
            _sessionCreator.OnSessionCreated += SessionCreatedOk;
            _sessionCreator.OnSessionClosed += SessionClosedOK;


            Console.WriteLine("Prepare to record Data");
            // Start
            _authorizer.Start();
            
            if (_readyForRecordDataEvent.WaitOne(50000))
            {
                Console.WriteLine("Press certain key to inject marker");
                Console.WriteLine("Press C to create record");
                Console.WriteLine("Press S to stop record");
                Console.WriteLine("Press Q to query record");
                Console.WriteLine("Press D to delete record");
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
                        _ctxClient.StopRecord(_cortexToken, _sessionId);
                    }
                    else if (keyInfo.Key == ConsoleKey.C)
                    {
                        // Create Record
                        string title = "RecDemo-" + _recordNo;
                        Console.WriteLine("Create Record" + title);
                        _ctxClient.CreateRecord(_cortexToken, _sessionId, title);
                        _recordNo++;
                    }
                    else if (keyInfo.Key == ConsoleKey.U)
                    {
                        // Update Record
                        string description = "description for RecDemo";
                        List<string> tags = new List<string>() { "demo1", "demo2" };
                        Console.WriteLine("Update Record: " + _currRecordId);
                        _ctxClient.UpdateRecord(_cortexToken, _currRecordId, description, tags);
                    }
                    else if (keyInfo.Key == ConsoleKey.Q)
                    {
                        // Query Record
                        // query param
                        JObject query = new JObject();
                        query.Add("applicationId", _sessionCreator.ApplicationId);

                        //order
                        JArray orderBy = new JArray();
                        JObject eleOrder = new JObject();
                        eleOrder.Add("startDatetime", "DESC" );
                        orderBy.Add(eleOrder);
                        // limit
                        int limit = 5; // number of maximum record return
                        int offset = 0;
                        _ctxClient.QueryRecord(_cortexToken, query, orderBy, offset, limit);
                    }
                    else if (keyInfo.Key == ConsoleKey.D)
                    {
                        // Delete first Record
                        if (_recordLists.Count > 0)
                        {
                            Record lastRecord = _recordLists[0];
                            string recordId = lastRecord.Uuid;
                            Console.WriteLine("Delete Record" + recordId);
                            _ctxClient.DeleteRecord(_cortexToken, new List<string>() { recordId });
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
                        _sessionCreator.CloseSession();
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

        private static void RecordUpdatedOK(object sender, Record record)
        {
            Console.WriteLine("RecordUpdatedOK");
            record.PrintOut();
        }

        private static void DeleteRecordOK(object sender, MultipleResultEventArgs e)
        {
            Console.WriteLine("DeleteRecordOK");
            Console.WriteLine("Successes: " + e.SuccessList);
            Console.WriteLine("Failures: " + e.FailList);
        }

        private static void QueryRecordsOK(object sender, List<Record> records)
        {
            _recordLists = records;
            Console.WriteLine("QueryRecordsOK");
            foreach(Record ele in records)
            {
                ele.PrintOut();
            }
        }

        private static void SessionClosedOK(object sender, string sessionId)
        {
            Console.WriteLine("The Session " + sessionId + " has closed successfully.");
        }

        private static void MessageErrorRecieved(object sender, ErrorMsgEventArgs e)
        {
            Console.WriteLine("MessageErrorRecieved :code " + e.Code + " message " + e.MessageError);
        }
        private static void RecordStoppedOK(object sender, Record record)
        {
            Console.WriteLine("Record " + record.Uuid + " is stopped successfully.");
        }

        private static void RecordCreatedOK(object sender, Record record)
        {
            Console.WriteLine("Record " + record.Uuid + " is created successfully.");
            _currRecordId = record.Uuid;
        }

        private static void SessionCreatedOk(object sender, string sessionId)
        {
            _sessionId = sessionId;
            _readyForRecordDataEvent.Set();
        }

        private static void HeadsetConnectedOK(object sender, string headsetId)
        {
            Console.WriteLine("HeadsetConnectedOK " + headsetId);
            // Wait a moment before creating session
            System.Threading.Thread.Sleep(1500);
            // CreateSession
            _sessionCreator.Create(_cortexToken, headsetId, true);
        }

        private static void AuthorizedOK(object sender, string cortexToken)
        {
            if (!String.IsNullOrEmpty(cortexToken))
            {
                _cortexToken = cortexToken;
                // find headset
                _headsetFinder.FindHeadset();
            }
        }
    }
}
