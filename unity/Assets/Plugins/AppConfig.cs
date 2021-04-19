/// <summary>
/// Contain configuration of a specific App.
/// </summary>
static class AppConfig
{
    public static string AppUrl              = "wss://localhost:6868";
    public static string AppName             = "UnityApp";
    
    /// <summary>
    /// Name of directory where contain tmp data and logs file.
    /// </summary>
    public static string TmpAppDataDir       = "UnityApp";
    public static string ClientId            = "put_your_clientId";
    public static string ClientSecret        = "put_your_clientSecret";
    public static string AppVersion          = "2.7.0 Dev";
    
    /// <summary>
    /// License Id is used for App
    /// In most cases, you don't need to specify the license id. Cortex will find the appropriate license based on the client id
    /// </summary>
    public static string AppLicenseId        = "";
}