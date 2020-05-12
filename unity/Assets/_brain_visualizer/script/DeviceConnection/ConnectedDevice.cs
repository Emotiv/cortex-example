
using EmotivUnityPlugin;

public class ConnectedDevice
{
    private Headset _information;

    public Headset Information { 
        get{ return _information; }
        set {
            _information = value;
            if (onHeadsetSelected != null)
                onHeadsetSelected.Invoke (_information);
        }
    }

    public event System.Action<Headset> onHeadsetSelected;
}
