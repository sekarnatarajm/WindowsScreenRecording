using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using MyVideoRecording.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVideoRecording.Services
{
    public class GoogleDriveService
    {
        public (string fileName, string folderId, string errorMessage) UploadFilesToDriveNew(GoogleDriveCrediantials googleDriveCrediantial, string fileToUpload)
        {
            try
            {
                string credentialsPath = GetDriveCredientialFilePath(googleDriveCrediantial);
                string folderId = googleDriveCrediantial.folder_id;
                GoogleCredential credential;
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(new[]
                    {
                        DriveService.ScopeConstants.DriveFile
                    });
                    var service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "LB Screen Recording" //Video File Upload Console App
                    });

                    var fileMetaData = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = Path.GetFileName(fileToUpload),
                        Parents = new List<string> { folderId }
                    };

                    FilesResource.CreateMediaUpload request;
                    using (var streamFile = new FileStream(fileToUpload, FileMode.Open))
                    {
                        request = service.Files.Create(fileMetaData, streamFile, string.Empty);
                        request.Fields = "id";
                        request.Upload();
                    }
                    var uploadedFile = request.ResponseBody;
                    //Console.WriteLine($"File '{fileMetaData.Name}' uploaded with ID: {uploadedFile.Id}");
                    //Console.WriteLine("************** Video Uploading Completed *****************");
                    DeleteFile(credentialsPath);
                    DeleteFile(fileToUpload);
                    return (fileMetaData.Name, fileMetaData.Parents.FirstOrDefault(), string.Empty);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Video Uploading Failed, Error : {ex.Message}");
                return ("", "", ex.Message);
            }
        }
        public string GetDriveCredientialFilePath(GoogleDriveCrediantials googleDriveCrediantials)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), AppConstant.TempFolderName, "Credentials.json");
            var jsonString = JsonConvert.SerializeObject(GetCredientials(googleDriveCrediantials));
            File.WriteAllText(tempFile, jsonString);
            return tempFile;
        }
        public void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public GoogleDriveUploadCrediantials GetCredientials(GoogleDriveCrediantials googleDriveCrediantials)
        {
            return new GoogleDriveUploadCrediantials()
            {
                type = googleDriveCrediantials.type,
                project_id = googleDriveCrediantials.project_id,
                private_key_id = googleDriveCrediantials.private_key_id,
                private_key = googleDriveCrediantials.private_key,
                client_email = googleDriveCrediantials.client_email,
                client_id = googleDriveCrediantials.client_id,
                auth_uri = googleDriveCrediantials.auth_uri,
                token_uri = googleDriveCrediantials.token_uri,
                auth_provider_x509_cert_url = googleDriveCrediantials.auth_provider_x509_cert_url,
                client_x509_cert_url = googleDriveCrediantials.client_x509_cert_url,
                universe_domain = googleDriveCrediantials.universe_domain
            };
        }
        public (string fileName, string folderId, string errorMessage) UploadFilesToDrive(GoogleDriveCrediantials googleDriveCrediantials, string fileToUpload)
        {
            try
            {
                string credentialsPath = GetDriveCredientialFilePath(googleDriveCrediantials);
                string folderId = googleDriveCrediantials.folder_id;
                //string fileToUpload = "C:\\Users\\Sekar_Nataraj\\AppData\\Local\\Temp\\ScreenRecorder\\2025-04-09 22-36-52\\2025-04-09 22-36-52.mp4";

                GoogleCredential credential;
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(new[]
                    {
                        DriveService.ScopeConstants.DriveFile
                    });
                    var service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Video File Upload Console App"
                    });

                    var fileMetaData = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = Path.GetFileName(fileToUpload),
                        Parents = new List<string> { folderId }
                    };

                    FilesResource.CreateMediaUpload request;
                    using (var streamFile = new FileStream(fileToUpload, FileMode.Open))
                    {
                        request = service.Files.Create(fileMetaData, streamFile, "");
                        request.Fields = "id";
                        request.Upload();
                    }
                    var uploadedFile = request.ResponseBody;
                    Console.WriteLine($"File '{fileMetaData.Name}' uploaded with ID: {uploadedFile.Id}");
                    Console.WriteLine("************** Video Uploading Completed *****************");
                    DeleteFile(credentialsPath);
                    return (fileMetaData.Name, fileMetaData.Parents.FirstOrDefault(), string.Empty);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Video Uploading Failed, Error : {ex.Message}");
                return ("", "", ex.Message);
            }
        }
        //public GoogleDriveUploadCrediantials GetCredientialsFromDevice()
        //{
        //    return new GoogleDriveUploadCrediantials()
        //    {
        //        type = "service_account",
        //        project_id = "lb-screen-recording-466604",
        //        private_key_id = "eeac10b8814fc15cf696a80541da757c3e63ecdf",
        //        private_key = "-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCylUSDac1T9hlr\nvvwWLafnFJpuXwnSG3bJPZHmNbUKX3rQPegZz08yQN4X6jbw433keE/LvvQ+R/DK\n25orS7FrpXUPJgDaXv4UG8VeYWnZmls6uBiXqEPj/ff88c7q8Hue+xOV7iTQR2v4\nxrYXlizbWn0OdxsWl+ePM59eefKiGgd0SDeh9daehG5slva2TS9qsNnJSlS4FM88\nQe7hXQq4tfUpyBDrpVVMFZILSb0fOcELDiljSA7yIRL7iDPQjpunGsUPLoE5JlyT\nM/wlB+R5I3IlRHchOU/T/2TG/8Gv9usYfgRdrsjBQmcbshLZzjv0oG8cJzWcyOPM\n/rv9qIYhAgMBAAECggEABCqd0oWTrtJSi0KfHpRyQuuAHPqOnKy7aTkZWAveFV4y\nW+mrClaD5RRuaHphNATqVUjJaGoXYd0bXqlzN9bTiCM8+Q1uG2orZu3Z1w7/Hj6Y\ng1hG3ZuLxiXXa3SH5/Z6oeD+46jh1vJCcDRQ2Wz6TxdV2XJF5LGGvrIjaGimtI/m\nK6Qeah/d6hK7ASYJyizyOKPf0ut4J5KU2Ve7SEuG7bLfxFS7dzDhYfLUaz3cvnJ+\n8WHvMxzAcS5llVwVpGv9zh65r/xcFPnmBWg4fXa3xfjsiqfhOw+D/gT1cPWnXMjY\nTeKo0aGn3uiiv19WZPzmTlDkHGBIvySOid3zy8u2OQKBgQDdP+n7OTD3Vob3dXVX\nZv9kFOkH7PR/k9YurbmIvGeOM9jny4xbTC7+3FzOdiJnkeXEBHvTpwXPWrjdOC0R\nsmy79Bsma1IE3H4q8TljUAtcsRLnmDtnYjSccp8E5b+0WMpRYZ9v3Ggbq9gqcVb3\niUT0bNls524Me1ujXLlXKEz9WQKBgQDOoczXagx8nBxGZwlbEHIfTU86aEFPcbLd\nKUnulrUMVq/LSbtglTZTjUQugq1S/0jl4v42CJ0VzOKVh72Yw4T3EwRXAj+qow4Q\ntRAQ15BA3G/OCBP79Cbkmz5Ek+3YkcsY2ExS1VH+ab4gDQKywJEZaxsj7mV8OFWa\nAgwMRwbOCQKBgFBBqAGebFm8RHctX2RWE1xdjW53kPVaTj5efEfSeAoIWq0yk/Zm\nO0Ht86hdB/vj26HwMm7DToM6GIb+orKhs3m2gca89WKYDRhMqpGQ7p4wCXiDK0FE\nSWta1L41DQZBkxpUPD6aiBVJj79Nn8tpOt8jQPeVN8FFWid3MjAgiVT5AoGBAJrK\nvNA2wonqzIe1El4ksMlgOdTwWtSvwSVKk/bm8VP/8Itifbs5rEvlDMmm3T7KvQpy\nBKvnwf2d0bPgzxiMh6Qrm9mudpFWuuerLBDh20+rkxoOFSJu4V/qKDhpdQkDFtlS\n18JJybXD80jBVl8gQNKA2QEOyvnGneHUXMApySA5AoGBAMzd0qCZuPBX5sq1HiGy\nLWtM2MqEHc9Kx1dXI4tOo5S0LQvh6ZxXEprvFT2uuOCEUMzFZ/ZfqeM1Udt97a9s\n959FN2neGmifxvDXL7QJ8/Z2urdzQozSMt6HeHnB70/rhea21z/cwnht8L1U38ZU\n6U4uOYm7bmRatWuQ+R7G8xKN\n-----END PRIVATE KEY-----\n",
        //        client_email = "lb-sereen-record-videos@lb-screen-recording-466604.iam.gserviceaccount.com",
        //        client_id = "104445928314596934536",
        //        auth_uri = "https://accounts.google.com/o/oauth2/auth",
        //        token_uri = "https://oauth2.googleapis.com/token",
        //        auth_provider_x509_cert_url = "https://www.googleapis.com/oauth2/v1/certs",
        //        client_x509_cert_url = "https://www.googleapis.com/robot/v1/metadata/x509/lb-sereen-record-videos%40lb-screen-recording-466604.iam.gserviceaccount.com",
        //        universe_domain = "googleapis.com"
        //    };
        //}
    }
}
