using MyVideoRecording.Common;
using NLog;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MyVideoRecording.Services
{
    public class LilbrahmasFileService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        HttpClient _client = new HttpClient();
        public LilbrahmasFileService()
        {
            _client.BaseAddress = new Uri(APIURL.BaseUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
        }
        public async Task<bool> UploadFileToLilBrahmasServer(string accessToken, string videoPath, string sessionTime, string employee_id)
        {
            string trackingIdentifier = $"{sessionTime}-{videoPath}";
            try
            {
                _logger.Info($"[{trackingIdentifier}]::Received request to UploadFileToLilBrahmasServer");
                var contents = new MultipartFormDataContent();
                // Convert IFormFile to StreamContent
                var fileStreams = File.OpenRead(videoPath);
                var fileContents = new StreamContent(fileStreams);
                fileContents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(AppConstant.MediaType);
                // Add file content to the request
                contents.Add(fileContents, "video", fileStreams.Name);
                // Optional: Add other form fields
                contents.Add(new StringContent(DateTime.Now.ToString(AppConstant.TodayDateForVideoUpload)), "date");
                contents.Add(new StringContent(sessionTime), "session");
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await _client.PostAsync(APIURL.FileUpload, contents);
                if (response.IsSuccessStatusCode)
                {
                    DeleteFile(videoPath);
                    _logger.Info($"[{trackingIdentifier}]::File deleted successfully, Path : {videoPath}");
                    return true;
                }
                else
                {
                    int stais = (int)response.StatusCode;
                    string respo = response.ReasonPhrase;
                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger.Info($"[{trackingIdentifier}]::File upload failed, StatusCode : {(int)response.StatusCode}, ReasonPhrase  {response.ReasonPhrase}, ResponseBody : {responseBody}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"[{trackingIdentifier}]::While UploadFileToLilBrahmasServer error occured, ErrorMessage : {ex.Message}", ex);
                EmailService emailService = new EmailService();
                string subject = "Lilbrahmas Screen Recording Error";
                string body = $"While Uploading File To Lil-Brahmas Server error occured, Video File Path : {videoPath}. </br> Employee Id : {employee_id}";
                await Task.Run(() => emailService.SendEmail(subject, body));
                return false;
            }
        }
        private void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
