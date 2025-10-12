using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVideoRecording
{
    public static class AppConstant
    {
        public static string TempFolderName = "LBScreenRecording";
        public static string TimeFormatForFile = "hhmm";
        public static string DateFormatForFile = "yyyyMMdd";

        public static string SessionDateFormat = "dd-MMMM-yyyy";
        public static string SessionTimeFormat = "HH:mm:ss";
        public static int ForceStopSleepTime = 10000;

        public static string VideoFileType = ".mp4";
        public static int FileUploadTimeout = 100;
        public static string VideoMimeType = "video/mp4";

        public static int RecordingTaskDelayTime = 5000;
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
