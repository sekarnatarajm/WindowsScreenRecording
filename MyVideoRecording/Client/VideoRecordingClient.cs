using LBScreenRecording.Enums;
using LBScreenRecording.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LBScreenRecording.Client
{
    public class VideoRecordingClient
    {
        public async Task SaveSessionStatusData(SessionStatus sessionStatus)
        {
            string apiUrl = $"api/Videos/SaveSessionStatus";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7018/");
                client.DefaultRequestHeaders.Clear();
                var strSessionStatus = JsonConvert.SerializeObject(sessionStatus);
                var data = new StringContent(strSessionStatus, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, data);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Success");
                }
                else
                {
                    Console.WriteLine($"Error : {response?.Content}");
                }
            }
        }
        public async Task<SessionStatus> GetSessionStatusByEmail(string emailId)
        {
            string apiUrl = $"api/Videos/GetSessionStatusByEmail?teacherEmail={emailId}";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7018/");
                client.DefaultRequestHeaders.Clear();
                while (true)
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var sessionStatus = JsonConvert.DeserializeObject<SessionStatus>(content);
                        return sessionStatus;
                    }
                    else
                    {
                        return new SessionStatus();
                    }
                }
            }
        }
        public async Task<SessionStatus> GetSessionStatusById(int sessionStatusId)
        {
            string apiUrl = $"api/Videos/GetSessionStatusById?sessionStatusId={sessionStatusId}";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7018/");
                client.DefaultRequestHeaders.Clear();
                while (true)
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var sessionStatus = JsonConvert.DeserializeObject<SessionStatus>(content);
                        return sessionStatus;
                    }
                    else
                    {
                        return new SessionStatus();
                    }
                }
            }
        }
        public async Task UpdateSessionStatus(RecordingStatus recordingStatus, int id, string fileName = "", string errorMessage = "")
        {
            var sessionStatus = await GetSessionStatusById(id);
            sessionStatus.RecordingStatus = recordingStatus.ToString();
            sessionStatus.RecordedFileName = fileName;
            sessionStatus.ErrorMessage = errorMessage;
            sessionStatus.ModifiedBy = "Exe";
            sessionStatus.ModifiedDate = DateTime.UtcNow;
            await SaveSessionStatusData(sessionStatus);
        }
    }
}
