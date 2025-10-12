using MyVideoRecording.Model;

namespace MyVideoRecording.Contracts
{
    public interface IFileUpload
    {
        void UploadFile(OAuthFileDetail oAuthFileDetail);
    }
}
