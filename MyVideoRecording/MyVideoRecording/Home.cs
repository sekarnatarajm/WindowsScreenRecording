using Microsoft.Extensions.Logging;
using MyVideoRecording.Client;
using MyVideoRecording.Contracts;
using MyVideoRecording.Enums;
using MyVideoRecording.Model;
using MyVideoRecording.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mod = MyVideoRecording.Model;

namespace MyVideoRecording
{
    public partial class Home : Form
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IUserService _userService;
        private readonly IScreenRecording _screenRecording;
        public UserDetail userDetail { get; set; } = new UserDetail();
        public List<ScheduledClass> scheduledClassList { get; set; } = new List<ScheduledClass>();
        public List<TodayScheduledSessions> todayScheduledSessions { get; set; } = new List<TodayScheduledSessions>();
        public Mod.LoginResponse loginResponse { get; set; } = new Mod.LoginResponse();
        public GoogleDriveCrediantials googleDriveCrediantials { get; set; }
        public string bearrerToken { get; set; }
        public bool isLogout { get; set; } = false;
        public bool isKeepCheckForSession { get; set; } = true;
        public int previousSessionStatusId = 0;
        public Home(IUserService userService, IScreenRecording screenRecording)
        {
            InitializeComponent();
            _userService = userService;
            _screenRecording = screenRecording;
        }

        private void Home_Load(object sender, EventArgs e)
        {
            try
            {
                this.FormClosing += MainForm_FormClosing;
                logger.Info("Received request for Home_Load");
                Task.Run(() => ProcessApiCallsAsync()).Wait();
                Task.Run(() => GetScheduledDataAsync());
                if (userDetail != null)
                {
                    lbl_employeeId.Text = userDetail.employee_id;
                    lbl_name.Text = userDetail.name;
                    lbl_email.Text = userDetail.email;
                    lbl_dob.Text = userDetail.date_of_birth;
                    lbl_mobileno.Text = userDetail.contact_no;
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
                Task task1 = GetUserDataAsync();
                Task task2 = GetGoogleDriveCrediantialsAsync();
                Task task3 = GetTodaySessionsAsync();
                await Task.WhenAll(task1, task2, task3);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while processing ProcessApiCallsAsync, ErrorMessage : {ex.Message}");
                MessageBox.Show($"While processing the ProcessApiCallsAsync error occurred, ErrorMessage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public async Task GetScheduledDataAsync()
        {
            try
            {
                logger.Info("Received request for GetScheduledDataAsync");
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
                    GridDataUpdate();
                    await ScheduleTheRecordingAsync();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while processing GetScheduledDataAsync, ErrorMessage : {ex.Message}");
                MessageBox.Show($"An error occurred in GetScheduledDataAsync, ErrorMessage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        public async Task GetGoogleDriveCrediantialsAsync()
        {
            try
            {
                logger.Info("Received request for GetGoogleDriveCrediantialsAsync");
                googleDriveCrediantials = await _userService.GetGoogleDriveCrediantials(loginResponse.AccessToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured while processing GetGoogleDriveCrediantialsAsync, ErrorMessage : {ex.Message}");
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
        private (int hour, int minute) GetTime(string time)
        {
            if (!string.IsNullOrEmpty(time) && time.Contains(':'))
            {
                return (Convert.ToInt32(time.Split(':')[0]), Convert.ToInt32(time.Split(':')[1]));
            }
            return (default, default);
        }
        public void GridDataUpdate()
        {
            logger.Info("Received request for GridDataUpdate");
            if (dataTodaySessions.InvokeRequired)
            {
                dataTodaySessions.Invoke(new MethodInvoker(delegate
                {
                    SessionDataBinding();
                }));
            }
            else
            {
                if (todayScheduledSessions != null && todayScheduledSessions.Any())
                {
                    SessionDataBinding();
                }
            }
        }

        private void SessionDataBinding()
        {
            logger.Info("Received request for SessionDataBinding");
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
            //dataTodaySessions.Columns["RecordingStatus"].Visible = true;

            dataTodaySessions.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
            dataTodaySessions.DefaultCellStyle.Font = new Font("Arial", 10);

            dataTodaySessions.Refresh();
            dataTodaySessions.Update();

        }

        public async Task ScheduleTheRecordingAsync()
        {
            logger.Info("Received request for ScheduleTheRecordingAsync");
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
                                _screenRecording.StartRecording(fileName);
                                previousSessionStatusId = currentSession.Id;
                                currentSession.RecordingStatus = RecordingStatus.Started;
                            }
                        }
                        else
                        {
                            var todatScssion = todayScheduledSessions.First(f => f.Id == previousSessionStatusId);
                            todatScssion.RecordingStatus = RecordingStatus.Completed;
                            _screenRecording.StopRecording(googleDriveCrediantials);
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
                                _screenRecording.StopRecording(googleDriveCrediantials);
                                todatScssion.RecordingStatus = RecordingStatus.Completed;
                                previousSessionStatusId = 0;
                            }
                        }
                    }
                    GridDataUpdate();
                    await Task.Delay(AppConstant.RecordingTaskDelayTime);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error occured while processing ScheduleTheRecordingAsync, ErrorMessage : {ex.Message}");
                }
            }
        }
        private void btnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout? This may take up to 10 seconds.", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }
            else
            {
                isKeepCheckForSession = false;
                isLogout = false;
                Task.Run(() => Logout()).Wait();
                if (isLogout)
                {
                    StopScreenRecording();
                    Thread.Sleep(AppConstant.ForceStopSleepTime);
                    this.Hide();
                    Login login = new Login(_userService);
                    login.Show();
                }
                else
                {
                    MessageBox.Show($"An error occurred while logout", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }            
        }

        private async Task Logout()
        {
            isLogout = await _userService.Logout(loginResponse.AccessToken, userDetail.employee_id);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit? This may take up to 10 seconds.", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                isKeepCheckForSession = false;
                StopScreenRecording();
                Thread.Sleep(AppConstant.ForceStopSleepTime);
            }
        }
        public void StopScreenRecording()
        {
            var todatScssion = todayScheduledSessions?.FirstOrDefault(f => f.Id == previousSessionStatusId);
            if (todatScssion != null && todatScssion.RecordingStatus == RecordingStatus.Started)
            {
                _screenRecording.StopRecording(googleDriveCrediantials);
                todatScssion.RecordingStatus = RecordingStatus.Completed;
                previousSessionStatusId = 0;
            }
        }
    }
}
