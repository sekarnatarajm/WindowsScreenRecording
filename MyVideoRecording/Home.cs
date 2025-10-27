using MyVideoRecording.Client;
using MyVideoRecording.Common;
using MyVideoRecording.Contracts;
using MyVideoRecording.Enums;
using MyVideoRecording.Model;
using MyVideoRecording.Services;
using Newtonsoft.Json;
using NLog;
using ScreenRecorderLib;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mod = MyVideoRecording.Model;

namespace MyVideoRecording
{
    public partial class Home : Form
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IUserService _userService;
        public UserDetail userDetail { get; set; } = new UserDetail();
        public List<ScheduledClass> scheduledClassList { get; set; } = new List<ScheduledClass>();
        public List<TodayScheduledSessions> todayScheduledSessions { get; set; } = new List<TodayScheduledSessions>();
        public Mod.LoginResponse loginResponse { get; set; } = new Mod.LoginResponse();
        public bool isLogout { get; set; } = false;
        public bool isKeepCheckForSession { get; set; } = true; //Checking the class status
        public int previousSessionStatusId = 0;
        public bool isLogoutOrFormClosed { get; set; } = false;
        private FileUploadStatus FileUploadedStatus { get; set; }
        private ClickEventType ClickedEventType { get; set; }
        private RecordingStatus currentRecordingStatus { get; set; }
        SocketIOClient.SocketIO socket = null;

        #region Screen Recording
        private SessionStatus sessionStatus;
        private static bool _isRecording;
        private static Stopwatch _stopWatch;
        private Recorder rec;
        private static string currentSessionTime = string.Empty;
        private static string accessToken = string.Empty;
        private static string employee_id = string.Empty;
        #endregion

        public Home(IUserService userService)
        {
            InitializeComponent();
            SetupScreenRecording();
            _userService = userService;
        }

        private void Home_Load(object sender, EventArgs e)
        {
            try
            {
                logger.Info("Received request for Home_Load");
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string appVersion = Application.ProductVersion;
                this.FormClosing += MainForm_FormClosing;
                Task.Run(() => ProcessApiCallsAsync()).Wait();
                PrepareTodaySessionData();
                Task.Run(() => ScheduleSessionRecordingAsync());
                if (userDetail != null)
                {
                    lbl_employeeId.Text = userDetail.employee_id;
                    lbl_name.Text = userDetail.name;
                    lbl_email.Text = userDetail.email;
                    lbl_dob.Text = userDetail.date_of_birth;
                    lbl_mobileno.Text = userDetail.contact_no;
                    if (todayScheduledSessions != null)
                    {
                        Task.Run(() => ConnectLBSocketAsync(loginResponse.AccessToken));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while processing Home_Load, ErrorMessage : {ex.Message}");
                MessageBox.Show($"While loading the Home_Load error occurred, ErrorMessage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task ProcessApiCallsAsync()
        {
            try
            {
                logger.Info("Received request for ProcessApiCallsAsync");
                Task taskGetUserDataAsync = GetUserDataAsync();
                Task taskGetTodaySessionsAsync = GetTodaySessionsAsync();
                await Task.WhenAll(taskGetUserDataAsync, taskGetTodaySessionsAsync);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while processing ProcessApiCallsAsync, ErrorMessage : {ex.Message}");
                MessageBox.Show($"While processing the ProcessApiCallsAsync error occurred, ErrorMessage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void PrepareTodaySessionData()
        {
            try
            {
                logger.Info("Received request for PrepareTodaySessionData");
                if (scheduledClassList != null && scheduledClassList.Any())
                {
                    int index = 1;
                    foreach (var session in scheduledClassList)
                    {
                        var startTimeData = GetTime(session.start_time);
                        var endTimeData = GetTime(session.end_time);
                        if (startTimeData.hour > 0 && endTimeData.hour > 0)
                        {
                            DateTime startDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, startTimeData.hour, startTimeData.minute, 0);
                            DateTime endDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, endTimeData.hour, endTimeData.minute, 0);
                            todayScheduledSessions.Add(new TodayScheduledSessions()
                            {
                                Id = session.id,
                                Index = index++,
                                TodayDate = DateTime.Now,
                                StartTime = startDateTime,
                                EndTime = endDateTime,
                                ClassStatus = ClassStatus.Pending,
                                RecordingStatus = RecordingStatus.Pending
                            });
                        }
                    }
                    UpdateTodaySessionGridData();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while processing PrepareTodaySessionData, ErrorMessage : {ex.Message}");
                MessageBox.Show($"An error occurred in PrepareTodaySessionData, ErrorMessage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public async Task GetUserDataAsync()
        {
            try
            {
                logger.Info("Received request for GetUserDataAsync");
                userDetail = await _userService.GetLoginUserData(loginResponse.AccessToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while processing GetUserDataAsync, ErrorMessage : {ex.Message}");
            }
        }
        public async Task GetTodaySessionsAsync()
        {
            try
            {
                logger.Info("Received request for GetTodaySessionsAsync");
                scheduledClassList = await _userService.GetTodaySessions(loginResponse.AccessToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while processing GetTodaySessionsAsync, ErrorMessage : {ex.Message}");
            }
        }
        private async Task Logout()
        {
            logger.Info("Received request for Logout");
            isLogout = await _userService.Logout(loginResponse.AccessToken, userDetail.employee_id);
        }
        private (int hour, int minute) GetTime(string time)
        {
            if (!string.IsNullOrEmpty(time) && time.Contains(':'))
            {
                return (Convert.ToInt32(time.Split(':')[0]), Convert.ToInt32(time.Split(':')[1]));
            }
            return (default, default);
        }
        public void UpdateTodaySessionGridData()
        {
            logger.Info("Received request for UpdateTodaySessionGridData");
            if (dataTodaySessions.InvokeRequired)
            {
                dataTodaySessions.Invoke(new MethodInvoker(delegate
                {
                    TodaySessionDataBinding();
                }));
            }
            else
            {
                if (todayScheduledSessions != null && todayScheduledSessions.Any())
                {
                    TodaySessionDataBinding();
                }
            }
        }

        private void TodaySessionDataBinding()
        {
            logger.Info("Received request for TodaySessionDataBinding");
            dataTodaySessions.DataSource = todayScheduledSessions;
            dataTodaySessions.ReadOnly = true;
            dataTodaySessions.Columns["Index"].HeaderText = "Sl No";
            dataTodaySessions.Columns["Index"].Width = 155;

            dataTodaySessions.Columns["TodayDate"].HeaderText = "Today Date";
            dataTodaySessions.Columns["TodayDate"].Width = 175;
            dataTodaySessions.Columns["TodayDate"].DefaultCellStyle.Format = AppConstant.SessionDateFormat;

            dataTodaySessions.Columns["StartTime"].HeaderText = "Start Time";
            dataTodaySessions.Columns["StartTime"].Width = 185;
            dataTodaySessions.Columns["StartTime"].DefaultCellStyle.Format = AppConstant.SessionTimeFormat;

            dataTodaySessions.Columns["EndTime"].HeaderText = "End Time";
            dataTodaySessions.Columns["EndTime"].Width = 185;
            dataTodaySessions.Columns["EndTime"].DefaultCellStyle.Format = AppConstant.SessionTimeFormat;

            dataTodaySessions.Columns["RecordingStatus"].HeaderText = "Recording Status";
            dataTodaySessions.Columns["RecordingStatus"].Width = 195;

            dataTodaySessions.Columns["Id"].Visible = false;
            dataTodaySessions.Columns["ClassStatus"].Visible = false;

            dataTodaySessions.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
            dataTodaySessions.DefaultCellStyle.Font = new Font("Arial", 10);

            dataTodaySessions.Refresh();
            dataTodaySessions.Update();
        }

        public async Task ScheduleSessionRecordingAsync()
        {
            logger.Info("Received request for ScheduleSessionRecordingAsync");
            Mod.SessionStatus sessionStatus = new Mod.SessionStatus();
            VideoRecordingClient videoService = new VideoRecordingClient();
            isKeepCheckForSession = true;
            while (isKeepCheckForSession)
            {
                try
                {
                    DateTime currentDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
                    var currentSession = todayScheduledSessions.FirstOrDefault(w => currentDateTime >= w.StartTime && currentDateTime <= w.EndTime);
                    if (currentSession != null && currentSession.Id > 0)
                    {
                        if (previousSessionStatusId.Equals(currentSession.Id) || previousSessionStatusId == 0)
                        {
                            if (currentSession.RecordingStatus == RecordingStatus.Pending)
                            {
                                string employeeId = userDetail.employee_id.Replace("-", "");
                                string todayDate = DateTime.Now.ToString(AppConstant.DateFormatForFile);
                                string startTime = currentSession.StartTime.ToString(AppConstant.TimeFormatForFile);
                                string endTime = currentSession.EndTime.ToString(AppConstant.TimeFormatForFile);
                                string fileName = $"{employeeId}-{todayDate}-{startTime}-{endTime}";
                                StartRecording(fileName, $"{currentSession.StartTime.ToString(AppConstant.SessionTimeFormat)}-{currentSession.EndTime.ToString(AppConstant.SessionTimeFormat)}", loginResponse.AccessToken, userDetail.employee_id);
                                previousSessionStatusId = currentSession.Id;
                                currentSession.RecordingStatus = RecordingStatus.Started;
                            }
                        }
                        else
                        {
                            var todatScssion = todayScheduledSessions.First(f => f.Id == previousSessionStatusId);
                            todatScssion.RecordingStatus = RecordingStatus.Completed;
                            StopRecording();
                            previousSessionStatusId = 0;
                        }
                    }
                    else
                    {
                        if (previousSessionStatusId > 0)
                        {
                            var todatScssion = todayScheduledSessions.First(f => f.Id == previousSessionStatusId);
                            if (todatScssion.RecordingStatus == RecordingStatus.Started)
                            {
                                StopRecording();
                                todatScssion.RecordingStatus = RecordingStatus.Completed;
                                previousSessionStatusId = 0;
                            }
                        }
                    }
                    UpdateTodaySessionGridData();
                    await Task.Delay(AppConstant.RecordingTaskDelayTime);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error occured while processing ScheduleSessionRecordingAsync, ErrorMessage : {ex.Message}");
                }
            }
        }
        private void btnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }
            else
            {
                DisconnectLBSocket();
                var recordStatus = GetRecordingStatus();
                if (recordStatus == RecordingStatus.Started)
                {
                    ClickedEventType = ClickEventType.Logout;
                    FileUploadedStatus = FileUploadStatus.Started;
                    isKeepCheckForSession = false;
                    StopScreenRecording();
                    while (true)
                    {
                        if (FileUploadedStatus == FileUploadStatus.Completed || FileUploadedStatus == FileUploadStatus.Pending)
                        {
                            break;
                        }
                    }
                    Task.Run(() => Logout());
                    todayScheduledSessions = new List<TodayScheduledSessions>();
                    ShowLoginPage();
                }
                else
                {
                    ShowLoginPage();
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                DisconnectLBSocket();
                var recordStatus = GetRecordingStatus();
                if (recordStatus == RecordingStatus.Started)
                {
                    ClickedEventType = ClickEventType.WindowClose;
                    FileUploadedStatus = FileUploadStatus.Started;
                    isKeepCheckForSession = false;
                    StopScreenRecording();
                    while (true)
                    {
                        if (FileUploadedStatus == FileUploadStatus.Completed || FileUploadedStatus == FileUploadStatus.Pending)
                        {
                            break;
                        }
                    }
                }
                todayScheduledSessions = new List<TodayScheduledSessions>();
                Task.Run(() => Logout());
            }
        }
        public void StopScreenRecording()
        {
            var currentSession = todayScheduledSessions?.FirstOrDefault(f => f.Id == previousSessionStatusId);
            if (currentSession != null && currentSession.RecordingStatus == RecordingStatus.Started)
            {
                StopRecording(true);
                currentSession.RecordingStatus = RecordingStatus.Completed;
                previousSessionStatusId = 0;
            }
            else
            {
                currentRecordingStatus = RecordingStatus.NoRecording;
                FileUploadedStatus = FileUploadStatus.Pending;
            }
        }
        public async Task ConnectLBSocketAsync(string accessToken)
        {
            try
            {
                SessionRoomOptions roomOptions = new SessionRoomOptions()
                {
                    roomId = $"{SocketEvent.Tutor}-{userDetail.id}",
                    connection = new Connection()
                    {
                        role = SocketEvent.AppRole,
                        data = new { identity = $"r-{userDetail.id}" }
                    }
                };

                socket = new SocketIOClient.SocketIO(APIURL.SocketBaseUrl, new SocketIOOptions()
                {
                    Auth = new { token = accessToken }
                });

                socket.On(SocketEvent.MuteUnMuteEventName, OnAudioStateChangeEvent);

                socket.OnError += (sender, e) =>
                {
                    logger.Error($"Error occured while connecting lb socket, Message : {e}, Sender : {sender}");
                };

                socket.OnConnected += async (sender, e) =>
                {
                    if (socket.Connected)
                    {
                        await socket.EmitAsync(SocketEvent.JoinMeetingEventName, roomOptions);
                    }
                };
                await socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while connecting lb socket async, ErrorMessage : {ex.Message}");
            }
        }

        #region Screen Recording
        private void SetupScreenRecording()
        {
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
        }
        private static void Rec_OnStatusChanged(object sender, RecordingStatusEventArgs e)
        {
            switch (e.Status)
            {
                case RecorderStatus.Idle:
                    break;
                case RecorderStatus.Recording:
                    _stopWatch = new Stopwatch();
                    _stopWatch.Start();
                    _isRecording = true;
                    break;
                case RecorderStatus.Paused:
                    break;
                case RecorderStatus.Finishing:
                    break;
                default:
                    break;
            }
        }
        private void Rec_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            _isRecording = false;
            _stopWatch?.Stop();
            if (isLogoutOrFormClosed)
            {
                FileUploadedStatus = FileUploadStatus.Started;
                Task.Run(() => UploadFile(e.FilePath, currentSessionTime, accessToken, employee_id)).Wait();
                FileUploadedStatus = FileUploadStatus.Completed;
            }
            else
            {
                Task.Run(() => UploadFile(e.FilePath, currentSessionTime, accessToken, employee_id));
            }
        }
        public async Task<bool> UploadFile(string filePath, string currentSessionTime, string accessToken, string employeeId)
        {
            LilbrahmasFileService lilbrahmasFileService = new LilbrahmasFileService();
            return await lilbrahmasFileService.UploadFileToLilBrahmasServer(accessToken, filePath, currentSessionTime, employee_id);
        }
        private static void Rec_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            _isRecording = false;
            _stopWatch?.Stop();
        }
        public void StartRecording(string fileName, string currentSessionDateTime, string userAccessToken, string employeeId)
        {
            currentSessionTime = currentSessionDateTime;
            accessToken = userAccessToken;
            employee_id = employeeId;
            string videoFormat = AppConstant.VideoFileType;
            string filePath = Path.Combine(Path.GetTempPath(), AppConstant.TempFolderName, fileName + videoFormat);
            rec.Record(filePath);
        }
        public void StopRecording(bool isLogoutOrFormClosedEvent = false)
        {
            isLogoutOrFormClosed = isLogoutOrFormClosedEvent;
            if (rec.Status == RecorderStatus.Recording || rec.Status == RecorderStatus.Paused)
            {
                rec.Stop();
            }
            else
            {
                FileUploadedStatus = FileUploadStatus.Pending;
            }
        }
        #endregion

        public void OnAudioStateChangeEvent(SocketIOResponse socketResponse)
        {
            try
            {
                var json = socketResponse?.ToString();
                if (string.IsNullOrEmpty(json))
                {
                    return;
                }
                var videoState = JsonConvert.DeserializeObject<VideoStateChange>(json);
                if (videoState == null)
                {
                    return;
                }
                if (rec != null && (rec.Status == RecorderStatus.Recording || rec.Status == RecorderStatus.Paused))
                {
                    if (string.Equals(videoState.Role, SocketEvent.MutedRole, StringComparison.OrdinalIgnoreCase))
                    {
                        if (videoState.Muted)
                            rec.Pause();
                        else
                            rec.Resume();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while processing video-state-change, ErrorMessage : {ex.Message}");
            }
        }
        public RecordingStatus GetRecordingStatus()
        {
            var currentSession = todayScheduledSessions?.FirstOrDefault(f => f.Id == previousSessionStatusId);
            return currentSession != null ? currentSession.RecordingStatus : RecordingStatus.NoRecording;
        }
        public void ShowLoginPage()
        {
            this.Hide();
            Login login = new Login(_userService);
            login.Show();
        }
        public void DisconnectLBSocket()
        {
            if (socket.Connected)
                socket.DisconnectAsync();
        }
    }
}