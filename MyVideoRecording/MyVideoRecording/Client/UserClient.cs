using MyVideoRecording.Common;
using MyVideoRecording.Contracts;
using MyVideoRecording.Model;
using MyVideoRecording.Services;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Mod = MyVideoRecording.Model;

namespace MyVideoRecording.Client
{
    public class UserClient : IUserClient
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        static HttpClient _client = new HttpClient();
        public UserClient()
        {
            _client.BaseAddress = new Uri(APIURL.BaseUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
        }
        public async Task<LoginResponse> Login(string username, string password)
        {
            Mod.LoginResponse loginResponse = new Mod.LoginResponse();
            try
            {
                _logger.Info($"[{username}]::Received request to Login");
                Mod.Login login = new Mod.Login()
                {
                    email = username,
                    password = password
                };
                var jsonString = JsonConvert.SerializeObject(login);
                var requestContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(APIURL.LoginUrl, requestContent);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response?.Content?.ReadAsStringAsync();
                    loginResponse = JsonConvert.DeserializeObject<Mod.LoginResponse>(content);
                    loginResponse.IsSuccess = true;
                }
                else
                {
                    loginResponse.IsError = true;
                    loginResponse.ErrorMessage = response.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error occured while processing ProcessApiCallsAsync, ErrorMessage : {ex.Message}");
            }
            return loginResponse;
        }
        public async Task<UserDetail> GetLoginUserData(string accessToken)
        {

            UserDetail userDetail = null;
            try
            {
                _logger.Info("Received request to Login");
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _client.GetAsync(APIURL.LoginUserDataUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    userDetail = JsonConvert.DeserializeObject<UserDetail>(content);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error occured while processing ProcessApiCallsAsync, ErrorMessage : {ex.Message}");
            }
            return userDetail;
        }
        public async Task<GoogleDriveCrediantials> GetGoogleDriveCrediantials(string accessToken)
        {
            GoogleDriveCrediantials googleDriveCrediantials = null;
            try
            {
                _logger.Info("Received request to Login");
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _client.GetAsync(APIURL.GoogleDriveCrediantialUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    googleDriveCrediantials = JsonConvert.DeserializeObject<GoogleDriveCrediantials>(content);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error occured while processing ProcessApiCallsAsync, ErrorMessage : {ex.Message}");
            }
            return googleDriveCrediantials;
        }
        public async Task<List<ScheduledClass>> GetTodaySessions(string accessToken)
        {
            List<ScheduledClass> scheduledClass = null;
            try
            {
                _logger.Info("Received request to Login");
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _client.GetAsync(APIURL.TodaySessionUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(content))
                    {
                        scheduledClass = JsonConvert.DeserializeObject<List<ScheduledClass>>(content);
                    }
                    else
                    {
                        scheduledClass = new List<ScheduledClass>();
                    }
                    // Testing Code
                    //if (scheduledClass == null || scheduledClass.Count == 0)
                    //    scheduledClass = GetTodayScheduledClasses();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error occured while processing ProcessApiCallsAsync, ErrorMessage : {ex.Message}");
            }
            return scheduledClass;
        }
        public async Task<bool> Logout(string accessToken, string trackingIdentifier)
        {
            var requestContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            try
            {
                _logger.Info("Received request for Logout");
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await _client.PostAsync(APIURL.LogoutUrl, requestContent);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error occured while processing ProcessApiCallsAsync, ErrorMessage : {ex.Message}");
            }
            return false;
        }
        public List<ScheduledClass> GetTodayScheduledClasses()
        {
            return new List<ScheduledClass>()
            {
                new ScheduledClass()
                {
                        id = 1,
                        start_time = "10:21:00",
                        end_time = "10:22:00",
                        active_students_count = 4,
                        user_id = 89
                },
                new ScheduledClass()
                {
                        id = 2,
                        start_time = "10:22:00",
                        end_time = "10:23:00",
                        active_students_count = 4,
                        user_id = 89
                },
                new ScheduledClass()
                {
                        id = 3,
                        start_time = "10:17:00",
                        end_time = "10:18:00",
                        active_students_count = 4,
                        user_id = 89
                },
                new ScheduledClass()
                {
                        id = 4,
                        start_time = "10:18:00",
                        end_time = "10:19:00",
                        active_students_count = 4,
                        user_id = 89
                }
                //new ScheduledClass()
                //{
                //        id = 5,
                //        start_time = "18:20:00",
                //        end_time = "18:25:00",
                //        active_students_count = 4,
                //        user_id = 89
                //},
                //new ScheduledClass()
                //{
                //        id = 6,
                //        start_time = "18:25:00",
                //        end_time = "18:30:00",
                //        active_students_count = 4,
                //        user_id = 89
                //},
                //new ScheduledClass()
                //{
                //        id = 7,
                //        start_time = "18:30:00",
                //        end_time = "18:35:00",
                //        active_students_count = 4,
                //        user_id = 89
                //},
                //new ScheduledClass()
                //{
                //        id = 8,
                //        start_time = "18:35:00",
                //        end_time = "18:40:00",
                //        active_students_count = 4,
                //        user_id = 89
                //},
                //new ScheduledClass()
                //{
                //        id = 9,
                //        start_time = "18:40:00",
                //        end_time = "18:45:00",
                //        active_students_count = 4,
                //        user_id = 89
                //},
                //new ScheduledClass()
                //{
                //        id = 10,
                //        start_time = "18:45:00",
                //        end_time = "18:50:00",
                //        active_students_count = 4,
                //        user_id = 89
                //},
                //new ScheduledClass()
                //{
                //        id = 11,
                //        start_time = "18:50:00",
                //        end_time = "19:00:00",
                //        active_students_count = 4,
                //        user_id = 89
                //}
            };
        }
    }
}
