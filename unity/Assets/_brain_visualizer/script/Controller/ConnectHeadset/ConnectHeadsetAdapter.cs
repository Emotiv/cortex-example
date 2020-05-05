/*
 * Class: Connect Headset Adapter
 * Create a event with headset info to add / clear headset list
 * 
 * Last Changed: hoang@emotiv.com
 * Date: 21 / 12 / 2017 
*/

using System.Collections.Generic;
using System;

using EmotivUnityPlugin;

namespace dirox.emotiv.controller
{
    public class ConnectHeadsetAdapter
    {
        private List<Headset> headsets;
        private ConnectHeadsetElement.Factory factory;

        public event Action<ConnectHeadsetElement> onNewItemReceived;
        public event Action<ConnectHeadsetElement> onClearItems;

        public ConnectHeadsetAdapter (ConnectHeadsetElement.Factory factory)
        {
            this.factory = factory;
            headsets = new List<Headset> ();
        }

        public void AddHeadset (Headset headset)
        {
            headsets.Add (headset);
            if (onNewItemReceived != null)
                onNewItemReceived.Invoke (factory.Create ().WithInformation (headset));
        }

        public void ClearHeadsetList ()
        {
            headsets.Clear();
            if (onClearItems != null)
                onClearItems.Invoke (null);
        }
    }
}