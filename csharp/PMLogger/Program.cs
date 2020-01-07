using System;
using System.Threading;
using System.IO;
using System.Collections;
using System.Text;
using CortexAccess;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PMLogger
{
    class Program
    {
        const string OutFilePath = @"PMLogger.csv";
        const string licenseID = "put_your_license_here";
        private static FileStream OutFileStream;

        static void Main(string[] args)
        {
            Console.WriteLine("PM LOGGER");
            Console.WriteLine("Please wear Headset with good signal!!!");

            // Delete Output file if existed
            if (File.Exists(OutFilePath))
            {
                File.Delete(OutFilePath);
            }
            OutFileStream = new FileStream(OutFilePath, FileMode.Append, FileAccess.Write);


            DataStreamExample dse = new DataStreamExample();
            dse.AddStreams("met");
            dse.OnSubscribed += SubscribedOK;
            dse.OnPerfDataReceived += OnPMDataReceived;

            // Need a valid license key and activeSession when subscribe performance metric data
            dse.Start(licenseID, true);

            Console.WriteLine("Press Esc to flush data to file and exit");
            while (Console.ReadKey().Key != ConsoleKey.Escape) { }

            // Unsubcribe stream
            dse.UnSubscribe();
            Thread.Sleep(5000);

            // Close Session
            dse.CloseSession();
            Thread.Sleep(5000);
            // Close Out Stream
            OutFileStream.Dispose();
        }

        private static void SubscribedOK(object sender, Dictionary<string, JArray> e)
        {
            foreach (string key in e.Keys)
            {
                if (key == "met")
                {
                    // print header
                    ArrayList header = e[key].ToObject<ArrayList>();
                    //add timeStamp to header
                    header.Insert(0, "Timestamp");
                    WriteDataToFile(header);
                }
            }
        }

        // Write Header and Data to File
        private static void WriteDataToFile(ArrayList data)
        {
            int i = 0;
            for (; i < data.Count - 1; i++)
            {
                string value;
                if (data[i] != null)
                {
                    value = data[i].ToString();
                }
                else
                    value = "null"; // mean no value data when the contact quality are low
                byte[] val = Encoding.UTF8.GetBytes(value + ", ");

                if (OutFileStream != null)
                    OutFileStream.Write(val, 0, val.Length);
                else
                    break;

            }
            // Last element
            string lastValue;
            if (data[i] != null)
                lastValue = data[i].ToString();
            else
                lastValue = "null"; // mean no value data when the contact quality are low

            byte[] lastVal = Encoding.UTF8.GetBytes(lastValue + "\n");
            if (OutFileStream != null)
                OutFileStream.Write(lastVal, 0, lastVal.Length);
        }

        private static void OnPMDataReceived(object sender, ArrayList pmData)
        {
            WriteDataToFile(pmData);
        }

    }
}
