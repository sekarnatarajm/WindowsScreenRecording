using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using MyVideoRecording.Contracts;
using MyVideoRecording.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyVideoRecording.Services
{
    public class GoogleDriveOAuthFileUpload : IFileUpload
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private DriveService _driveService;
        string filePath = string.Empty;
        public void UploadFile(OAuthFileDetail oAuthFileDetail)
        {
            try
            {
                _logger.Info($"Received request to upload the file, FilePath : {oAuthFileDetail?.FilePath}");
                filePath = string.Empty;
                if (File.Exists(oAuthFileDetail.FilePath))
                {
                    filePath = oAuthFileDetail.FilePath;
                    Authorize(oAuthFileDetail.OAuthCredientials);
                    Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
                    body.Name = Path.GetFileName(oAuthFileDetail.FilePath);
                    body.Description = oAuthFileDetail.Description;
                    body.MimeType = AppConstant.VideoMimeType;
                    body.Parents = new List<string> { oAuthFileDetail.ParentFolderId };// UN comment if you want to upload to a folder(ID of parent folder need to be send as paramter in above method)
                    byte[] byteArray = File.ReadAllBytes(oAuthFileDetail.FilePath);
                    MemoryStream stream = new MemoryStream(byteArray);
                    FilesResource.CreateMediaUpload request = _driveService.Files.Create(body, stream, AppConstant.VideoMimeType);
                    request.SupportsTeamDrives = true;
                    // You can bind event handler with progress changed event and response recieved(completed event)
                    request.ProgressChanged += Request_ProgressChanged;
                    request.ResponseReceived += Request_ResponseReceived;
                    request.Upload();
                    //return request.ResponseBody;
                }
                else
                {
                    _logger.Warn($"File Not Exists in this File Path : {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error occured while Uploading the File, FilePath : {filePath}, ErrorMessage : {oAuthFileDetail?.FilePath}");
            }
        }
        private void Request_ResponseReceived(Google.Apis.Drive.v3.Data.File file)
        {
            _logger.Info($"Received request Request_ResponseReceived, FileId : {file?.Id}, FileName : {file?.Name}");
        }
        private void Request_ProgressChanged(IUploadProgress progress)
        {
            _logger.Info($"Received request Request_ProgressChanged, FilePath : {filePath}, Status : {progress?.Status.ToString()}, BytesSent : {progress?.BytesSent}");
            if (progress.Status == UploadStatus.Completed)
            {
                _logger.Info($"File uploading completed : {progress.Status}");
                DeleteFile(filePath);
            }
            else if (progress.Status == UploadStatus.Failed)
            {
                _logger.Error($"File uploading failed : {progress.Status}, Exception : {progress?.Exception}, ErrorNessage : {progress?.Exception?.Message}");
            }
            else
            {
                _logger.Info($"File uploading status : {progress.Status}");
            }
        }
        public void Authorize(OAuthCredientials oAuthCredientials)
        {
            _logger.Info($"Received request to Authorize");
            string[] scopes = new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile };

            //Here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%  
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = oAuthCredientials.ClientId,
                ClientSecret = oAuthCredientials.ClientSecret
            },
            scopes,
            Environment.UserName, CancellationToken.None, new FileDataStore("MyAppsToken")).Result;

            //Once consent is recieved, your token will be stored locally on the AppData directory, so that next time you wont be prompted for consent.   
            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Screen Recording"
            });
            _driveService.HttpClient.Timeout = TimeSpan.FromMinutes(AppConstant.FileUploadTimeout);
            _logger.Info($"Authorize completed successfully");
        }
        public void DeleteFile(string filePath)
        {
            _logger.Info($"Received request DeleteFile, FilePath : {filePath}");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.Info($"File deleted successfully, FilePath : {filePath}");
            }
        }
    }
}
