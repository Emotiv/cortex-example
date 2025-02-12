/// <summary>
/// Contain configuration of a specific App.
/// </summary>
public static class AppConfig
{
    public static string AppVersion          = "1.1.0";
    public static string AppName             = "UnityApp";
    public static string ClientId            = "put_your_client_id_here";
    public static string ClientSecret        = "put_your_client_secret_here";
    public static bool IsDataBufferUsing     = true; // Set false if you want to display data directly to MessageLog without storing in Data Buffer
    
    #if UNITY_ANDROID || UNITY_IOS
    public static string UserName            = ""; // for private login
    public static string Password            = "";
    #elif !USE_EMBEDDED_LIB_WIN
    public static string AppUrl              = "wss://localhost:6868"; // for desktop without embedded cortex
    #endif
    
}