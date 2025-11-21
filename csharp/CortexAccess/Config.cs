namespace CortexAccess
{
    static class Config
    {
        /*
         * Enter your application Client ID and Client Secret below.
         * You can obtain these credentials after registering your App ID with the Cortex SDK for development.
         * For instructions, visit: https://emotiv.gitbook.io/cortex-api#create-a-cortex-app
         */
        public static string AppClientId = "put_your_application_client_id_here";
        public static string AppClientSecret = "put_your_application_client_secret_here";

        // If you use an Epoc Flex headset, then you must put your configuration here
        public static string FlexMapping = @"{
                                  'CMS':'TP8', 'DRL':'P6',
                                  'RM':'TP10','RN':'P4','RO':'P8'}";

    }

    public static class WarningCode
    {
        public const int StreamStop = 0;
        public const int SessionAutoClosed = 1;
        public const int UserLogin = 2;
        public const int UserLogout = 3;
        public const int ExtenderExportSuccess = 4;
        public const int ExtenderExportFailed = 5;
        public const int UserNotAcceptLicense = 6;
        public const int UserNotHaveAccessRight = 7;
        public const int UserRequestAccessRight = 8;
        public const int AccessRightGranted = 9;
        public const int AccessRightRejected = 10;
        public const int CannotDetectOSUSerInfo = 11;
        public const int CannotDetectOSUSername = 12;
        public const int ProfileLoaded = 13;
        public const int ProfileUnloaded = 14;
        public const int CortexAutoUnloadProfile = 15;
        public const int UserLoginOnAnotherOsUser = 16;
        public const int EULAAccepted = 17;
        public const int StreamWritingClosed = 18;
        public const int DataPostProcessingFinished = 30;
        public const int HeadsetWrongInformation = 100;
        public const int HeadsetCannotConnected = 101;
        public const int HeadsetConnectingTimeout = 102;
        public const int HeadsetDataTimeOut = 103;
        public const int HeadsetConnected = 104;
        public const int HeadsetScanFinished = 142;
    }
}
