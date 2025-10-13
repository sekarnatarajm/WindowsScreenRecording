using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVideoRecording
{
    public static class AppConstant
    {
        public static string MutedRole = "tutor";
        public static string TempFolderName = "LBScreenRecording";
        public static string TimeFormatForFile = "hhmm";
        public static string DateFormatForFile = "yyyyMMdd";

        public static string SessionDateFormat = "dd-MMMM-yyyy";
        public static string SessionTimeFormat = "HH:mm";
        public static int ForceStopSleepTime = 15000;

        public static string VideoFileType = ".mp4";
        public static int FileUploadTimeout = 100;
        public static string MediaType = "video/mp4";

        public static int RecordingTaskDelayTime = 5000;
        public static string TodayDateForVideoUpload = "yyyy-MM-dd";

        public static string UserName = "lilbrahmas31@gmail.com";
        public static string Password = "EC7FC4D913F9433ACA43E1563ADE3089A3CC";
        public static string FromEmail = "lilbrahmas31@gmail.com";
        public static string ToEmail = "parentmeeting@lilbrahmas.com";
        public static string CcEmail = "lilbrahmas31@gmail.com";
    }
    public static class VideoOption
    {
        public static int Width = 1280;
        public static int Height = 720;
        public static int Quality = 50;
        public static int Bitrate = 20000;
        public static int Framerate = 15;
    }
}
