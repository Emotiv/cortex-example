using CortexAccess;
using System;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;

namespace InjectMarker
{
    class Program
    {
        const string WantedHeadsetId = ""; // if you want to connect to specific headset, put headset id here. For example: "EPOCX-71D833AC"
        private static RecordManager _recordManager;
        private static int _recordNo = 1;
        private static AutoResetEvent _readyForRecordDataEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("INJECT MARKER DEMO");
            Console.WriteLine("Please wear Headset with good signal!!!");

            _recordManager = new RecordManager();
            _recordManager.sessionCreateOk += OnSessionCreatedOk;

            // Start
            _recordManager.Start(WantedHeadsetId);

            if (_readyForRecordDataEvent.WaitOne(50000))
            {
                Console.WriteLine("Press certain key except below keys to inject marker");
                Console.WriteLine("Press C to create record");
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
                    else if (keyInfo.Key == ConsoleKey.H)
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
                        _recordManager.InjectMarker(keyInfo.Key.ToString(), valueMaker.ToString());
                        valueMaker++;
                    }
                }
                _recordManager.Stop();

                Thread.Sleep(10000); 
            }
            else
            {
                Console.WriteLine("The preparation for injecting marker is unsuccessful. Please try again");
            }
        }
        private static void OnSessionCreatedOk(object sender, bool isOk)
        {
            if (isOk)
            {
                Console.WriteLine("SessionCreatedOK");
                _readyForRecordDataEvent.Set();
            }
        }
    }
}
