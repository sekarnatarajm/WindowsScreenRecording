
using MyVideoRecording.Model;

namespace MyVideoRecording.Contracts
{
    public interface IScreenRecording
    {
        void StartRecording(string fileName, string currentSessionTime, string userAccessToken,string employeeId);
        void StopRecording();
    }
}
