using MyVideoRecording.Client;
using MyVideoRecording.Contracts;
using MyVideoRecording.Model;
using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVideoRecording.Services
{
    public class ScreenRecording : IScreenRecording
    {
        private SessionStatus sessionStatus;
        private static bool _isRecording;
        private static Stopwatch _stopWatch;
        private Recorder rec;
        private readonly IFileUpload _fileUpload;

        public static GoogleDriveCrediantials googleDriveCrediantialData { get; set; } = new GoogleDriveCrediantials();
        public ScreenRecording(IFileUpload fileUpload)
        {
            //This is how you can select audio devices. If you want the system default device,
            //just leave the AudioInputDevice or AudioOutputDevice properties unset or pass null or empty string.
            var audioInputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.All);
            var audioOutputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.All);
            string selectedAudioInputDevice = audioInputDevices.Count > 0 ? audioInputDevices.First().DeviceName : null;
            string selectedAudioOutputDevice = audioOutputDevices.Count > 0 ? audioOutputDevices.First().DeviceName : null;

            var opts = new RecorderOptions
            {
                OutputOptions = new OutputOptions
                {
                    OutputFrameSize = new ScreenSize(VideoOption.Width, VideoOption.Height)
                },
                AudioOptions = new AudioOptions
                {
                    AudioInputDevice = selectedAudioInputDevice,
                    AudioOutputDevice = selectedAudioOutputDevice,
                    IsAudioEnabled = true,
                    IsInputDeviceEnabled = true,
                    IsOutputDeviceEnabled = true,
                },
                VideoEncoderOptions = new VideoEncoderOptions
                {
                    Quality = VideoOption.Quality,
                    Bitrate = VideoOption.Bitrate,
                    Framerate = VideoOption.Framerate,
                    IsThrottlingDisabled = false,
                    IsLowLatencyEnabled = true,
                    IsFixedFramerate = false,
                    Encoder = new H264VideoEncoder()
                    {
                        EncoderProfile = H264Profile.High,
                        BitrateMode = H264BitrateControlMode.Quality
                    }
                }
            };

            rec = Recorder.CreateRecorder(opts);
            rec.OnRecordingFailed += Rec_OnRecordingFailed;
            rec.OnRecordingComplete += Rec_OnRecordingComplete;
            rec.OnStatusChanged += Rec_OnStatusChanged;
            _fileUpload = fileUpload;
        }
        private static void Rec_OnStatusChanged(object sender, RecordingStatusEventArgs e)
        {
            switch (e.Status)
            {
                case RecorderStatus.Idle:
                    //Console.WriteLine("Recorder is idle");
                    break;
                case RecorderStatus.Recording:
                    _stopWatch = new Stopwatch();
                    _stopWatch.Start();
                    _isRecording = true;
                    //Console.WriteLine("Recording started");
                    //Console.WriteLine("Press ESC to stop recording");
                    break;
                case RecorderStatus.Paused:
                    //Console.WriteLine("Recording paused");
                    break;
                case RecorderStatus.Finishing:
                    //Console.WriteLine("Finishing encoding");
                    break;
                default:
                    break;
            }
        }

        private void Rec_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            _isRecording = false;
            _stopWatch?.Stop();
            Task.Run(() => UploadFile(e.FilePath));
        }
        public void UploadFile(string filePath)
        {
            OAuthFileDetail oAuthFileDetail = new OAuthFileDetail()
            {
                FilePath = filePath, //@"C:\Dell Documents\Sekar\Video Recording\2025-07-16 17-51-03.mp4",
                Description = "LB Screen Recording",
                ParentFolderId = "1jZLhurDHqMIHThTnDNbJGJpi4j2b9Hzv"
            };
            _fileUpload.UploadFile(oAuthFileDetail);
            //GoogleDriveService googleDriveService = new GoogleDriveService();
            //var fileUploadData = googleDriveService.UploadFilesToDrive(googleDriveCrediantialData, filePath);
        }
        private static void Rec_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            //Console.WriteLine("Recording failed with: " + e.Error);
            _isRecording = false;
            _stopWatch?.Stop();
            //Console.WriteLine();
            //Console.WriteLine("Press any key to exit");
        }
        public void StartRecording(string fileName)
        {
            string videoFormat = AppConstant.VideoFileType;
            string filePath = Path.Combine(Path.GetTempPath(), AppConstant.TempFolderName, fileName + videoFormat);
            rec.Record(filePath);
        }
        public void StopRecording(GoogleDriveCrediantials googleDriveCrediantials)
        {
            googleDriveCrediantialData = googleDriveCrediantials;
            rec.Stop();
        }
    }
}
