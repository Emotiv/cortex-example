using CortexAccess;
using System;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;

namespace InjectMarker
{
    class Program
    {
        private static string _cortexToken;
        private static string _sessionId;
        private static bool _isRecording;
        private static List<string> _markersList = new List<string>();
        private static int _recordNo = 1;
        private static Authorizer _authorizer = new Authorizer();
        private static HeadsetFinder _headsetFinder = new HeadsetFinder();
        private static SessionCreator _sessionCreator = new SessionCreator();
        private static CortexClient _ctxClient;
        private static AutoResetEvent _readyForInjectMarkerEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("INJECT MARKER DEMO");
            Console.WriteLine("Please wear Headset with good signal!!!");


            _ctxClient = CortexClient.Instance;
            _ctxClient.OnCreateRecord += RecordCreatedOK;
            _ctxClient.OnStopRecord += RecordStoppedOK;
            _ctxClient.OnInjectMarker += MarkerInjectedOk;
            _ctxClient.OnErrorMsgReceived += MessageErrorRecieved;

            _authorizer.OnAuthorized += AuthorizedOK;
            _headsetFinder.OnHeadsetConnected += HeadsetConnectedOK;
            _sessionCreator.OnSessionCreated += SessionCreatedOk;
            _sessionCreator.OnSessionClosed += SessionClosedOK;

            Console.WriteLine("Prepare to inject marker");
            // Start
            _authorizer.Start();            

            if (_readyForInjectMarkerEvent.WaitOne(50000))
            {
                Console.WriteLine("Press certain key except below keys to inject marker");
                Console.WriteLine("Press S to stop record and quit");
                Console.WriteLine("Press Esc to quit");
                Console.WriteLine("Press H to show all commands");
                Console.WriteLine("Ignore Tab, Enter, Spacebar and Backspace key");

                int valueMaker = 1;
                ConsoleKeyInfo keyInfo;
                while (true)
                {
                    keyInfo = Console.ReadKey(true);
                    Console.WriteLine(keyInfo.KeyChar.ToString() + " has pressed");
                    if (keyInfo.Key == ConsoleKey.S)
                    {
                        // Querry Sessions before quit
                        _ctxClient.StopRecord(_cortexToken, _sessionId);
                        Thread.Sleep(10000);
                        break;
                    }
                    if (keyInfo.Key == ConsoleKey.H)
                    {
                        Console.WriteLine("Press certain key except below keys to inject marker");
                        Console.WriteLine("Press S to stop record and quit");
                        Console.WriteLine("Press Esc to quit");
                        Console.WriteLine("Press H to show all commands");
                        Console.WriteLine("Ignore Tab, Enter, Spacebar and Backspace key");
                    }
                    else if (keyInfo.Key == ConsoleKey.Tab) continue;
                    else if (keyInfo.Key == ConsoleKey.Backspace) continue;
                    else if (keyInfo.Key == ConsoleKey.Enter) continue;
                    else if (keyInfo.Key == ConsoleKey.Spacebar) continue;
                    else if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                    else
                    {
                        // Inject marker
                        if (_isRecording)
                        {
                            _ctxClient.InjectMarker(_cortexToken, _sessionId, keyInfo.Key.ToString(), valueMaker, Utils.GetEpochTimeNow());
                            Thread.Sleep(2000);
                            valueMaker++;
                        }
                    }
                }

                Thread.Sleep(10000); 
            }
            else
            {
                Console.WriteLine("The preparation for injecting marker is unsuccessful. Please try again");
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

        private static void MarkerInjectedOk(object sender, JObject marker)
        {
            Console.WriteLine("Marker is injected successfully. Marker " + marker);
            string markerId = (string)marker["uuid"];
            _markersList.Add(markerId);
        }

        private static void RecordStoppedOK(object sender, Record record)
        {
            Console.WriteLine("Record " + record.Uuid + " is stopped successfully.");
            _isRecording = false;
            _sessionCreator.CloseSession();
        }

        private static void RecordCreatedOK(object sender, Record record)
        {
            Console.WriteLine("Record " + record.Uuid + " is created successfully.");
            _readyForInjectMarkerEvent.Set();
            _isRecording = true;
        }

        private static void SessionCreatedOk(object sender, string sessionId)
        {
            _sessionId = sessionId;
            // create Record
            string title = "test-" + _recordNo;
            _ctxClient.CreateRecord(_cortexToken, sessionId, title);
            _recordNo++;
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
