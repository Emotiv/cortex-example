using CortexAccess;
using System;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;

namespace MentalCommandTraining
{
    class Program
    {
        private static string _profileName = "put_your_profile_here"; // new profile name for creating or existed profile name for loading

        private static Training _trainer = new Training();
        private static bool _isSucceeded = false;
        private static bool _isProfileLoaded = false;
        private static string _currentAction="";

        private static CortexClient _ctxClient;
        private static AutoResetEvent _readyForTrainingEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("TRAINING MENTAL COMMAND DEMO");
            Console.WriteLine("Please wear Headset with good signal!!!");
            _ctxClient = CortexClient.Instance;
            _trainer.OnReadyForTraning += ReadyForTraining;
            _trainer.OnTrainingSucceeded += TrainingSucceededOK;
            _trainer.OnProfileLoaded += ProfileLoadedOK;
            _trainer.OnUnProfileLoaded += ProfileUnloadedOK;

            Console.WriteLine("Prepare to training");
            // Start
            _trainer.Start("mentalCommand");

            if (_readyForTrainingEvent.WaitOne(50000))
            {
                Console.WriteLine("Press C to create a Profile.");
                Console.WriteLine("Press L to load a Profile.");
                Console.WriteLine("Press U to unload a Profile.");
                Console.WriteLine("Press 0 to start Neutral training.");
                Console.WriteLine("Press 1 to start Push training.");
                Console.WriteLine("Press 2 to start Pull training.");
                Console.WriteLine("Press A to accept training.");
                Console.WriteLine("Press R to reject training.");
                Console.WriteLine("Press H to show all commands.");
                Console.WriteLine("Press Esc to quit");
                Console.WriteLine("Ignore Tab, Enter, Spacebar and Backspace key");

                ConsoleKeyInfo keyInfo;
                while (true)
                {
                    keyInfo = Console.ReadKey(true);
                    Console.WriteLine(keyInfo.KeyChar.ToString() + " has pressed");

                    if (keyInfo.Key == ConsoleKey.C)
                    {
                        if (string.IsNullOrEmpty(Program._profileName))
                            _profileName = Utils.GenerateUuidProfileName("McDemo");

                        Console.WriteLine("Create profile: " + _profileName);
                        _trainer.CreateProfile(_profileName);
                        Thread.Sleep(1000);
                    }
                    else if (keyInfo.Key == ConsoleKey.L)
                    {
                        //Load profile
                        Console.WriteLine("Load profile: " + _profileName);
                        _trainer.LoadProfile(_profileName);
                        Thread.Sleep(1000);
                    }
                    else if (keyInfo.Key == ConsoleKey.U)
                    {
                        //Load profile
                        Console.WriteLine("UnLoad profile: " + _profileName);
                        _trainer.UnLoadProfile(_profileName);
                        Thread.Sleep(1000);
                    }
                    else if (keyInfo.Key == ConsoleKey.D0)
                    {
                        if (_isProfileLoaded)
                        {
                            _currentAction = "neutral";
                            //Start neutral training
                            _trainer.DoTraining(_currentAction, "start");
                            Thread.Sleep(2000);
                        }                        
                    }
                    else if (keyInfo.Key == ConsoleKey.D1)
                    {
                        if (_isProfileLoaded)
                        {
                            //Start push training
                            _currentAction = "push";
                            _trainer.DoTraining(_currentAction, "start");
                            Thread.Sleep(2000);
                        }                           
                    }
                    else if (keyInfo.Key == ConsoleKey.D2)
                    {
                        if (_isProfileLoaded)
                        {
                            //Start pull training
                            _currentAction = "pull";
                            _trainer.DoTraining(_currentAction, "start");
                            Thread.Sleep(2000);
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.A)
                    {
                        //Accept training
                        if (_isSucceeded)
                        {
                            _trainer.DoTraining(_currentAction, "accept");
                            Thread.Sleep(1000);
                            _isSucceeded = false; // reset
                        }                       
                    }
                    else if (keyInfo.Key == ConsoleKey.R)
                    {
                        //Reject training
                        if (_isSucceeded)
                        {
                            _trainer.DoTraining(_currentAction, "reject");
                            Thread.Sleep(1000);
                            _isSucceeded = false; // reset
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.H)
                    {
                        Console.WriteLine("Press C to create a Profile.");
                        Console.WriteLine("Press L to load a Profile.");
                        Console.WriteLine("Press U to unload a Profile.");
                        Console.WriteLine("Press 0 to start Neutral training.");
                        Console.WriteLine("Press 1 to start Push training.");
                        Console.WriteLine("Press 2 to start Pull training.");
                        Console.WriteLine("Press A to accept training.");
                        Console.WriteLine("Press R to reject training.");
                        Console.WriteLine("Press H to show all commands");
                        Console.WriteLine("Press Esc to quit");
                    }
                    else if (keyInfo.Key == ConsoleKey.Tab) continue;
                    else if (keyInfo.Key == ConsoleKey.Backspace) continue;
                    else if (keyInfo.Key == ConsoleKey.Enter) continue;
                    else if (keyInfo.Key == ConsoleKey.Spacebar) continue;
                    else if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        _trainer.CloseSession();
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
                Console.WriteLine("The preparation for training is unsuccessful. Please try again");
            }
        }

        private static void ProfileUnloadedOK(object sender, bool e)
        {
            Console.WriteLine("The profile has unloaded successfully");
            _isProfileLoaded = false;
        }

        private static void ProfileLoadedOK(object sender, string profile)
        {
            _isProfileLoaded = true;
            Console.WriteLine("The profile " + profile + " has loaded successfully.");
            Console.WriteLine("Press 0 to start Neutral training.");
            Console.WriteLine("Press 1 to start Push training.");
            Console.WriteLine("Press 2 to start Pull training.");
            Console.WriteLine("Press A to accept training.");
            Console.WriteLine("Press R to reject training.");
            Console.WriteLine("Press Esc to quit");
        }

        private static void TrainingSucceededOK(object sender, bool isSucceeded)
        {
            _isSucceeded = isSucceeded;
            Console.WriteLine("Please accept(A) or reject(R) the training");
        }

        private static void ReadyForTraining(object sender, bool e)
        {
            _readyForTrainingEvent.Set();
        }
    }
}
