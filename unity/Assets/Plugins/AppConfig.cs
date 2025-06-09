/// <summary>
/// Contain configuration of a specific App.
/// </summary>
public static class AppConfig
{
    public static string AppVersion          = "1.1.0";
    public static string AppName             = "UnityApp";
    public static string ClientId            = "put_your_client_id_here";
    public static string ClientSecret        = "put_your_client_secret_here";
    public static bool IsDataBufferUsing     = false; // Set false if you want to display data directly to MessageLog without storing in Data Buffer
    public static bool AllowSaveLogToFile    = false; // Set true to save log to file and cortex token to local file for next time use
    
    #if !USE_EMBEDDED_LIB && !UNITY_ANDROID && !UNITY_IOS
    // only for desktop without embedded cortex
    public static string AppUrl              = "wss://localhost:6868"; // for desktop without embedded cortex
    #else
    public static string AppUrl              = ""; // Don't need AppUrl for mobile and embedded cortex
    #endif

}