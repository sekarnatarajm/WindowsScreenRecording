
using MyVideoRecording.Model;

namespace MyVideoRecording.Contracts
{
    public interface IScreenRecording
    {
        void StartRecording(string fileName);
        void StopRecording(GoogleDriveCrediantials googleDriveCrediantials);
    }
}
