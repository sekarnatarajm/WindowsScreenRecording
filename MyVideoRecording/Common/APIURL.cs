namespace LBScreenRecording.Common
{
    internal static class APIURL
    {
        public static string BaseUrl = "https://qa.lilbrahmas.org/";
        public static string SocketBaseUrl = "https://qa.lilbrahmas.org:3003";

        public static string LoginUrl = "api/v1/login";
        public static string LoginUserDataUrl = "api/v1/user";
        public static string TodaySessionUrl = "api/v1/today-sessions";
        public static string LogoutUrl = "api/v1/logout";
        public static string FileUpload = "api/v1/recordings";
    }
}
