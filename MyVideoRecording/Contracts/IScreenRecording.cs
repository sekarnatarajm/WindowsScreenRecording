
using LBScreenRecording.Model;

namespace LBScreenRecording.Contracts
{
    public interface IScreenRecording
    {
        void StartRecording(string fileName, string currentSessionTime, string userAccessToken,string employeeId);
        void StopRecording();
    }
}
