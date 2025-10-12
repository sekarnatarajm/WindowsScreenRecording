using MyVideoRecording.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyVideoRecording.Contracts
{
    public interface IUserService
    {
        Task<LoginResponse> Login(string username, string password);
        Task<UserDetail> GetLoginUserData(string accessToken);
        Task<GoogleDriveCrediantials> GetGoogleDriveCrediantials(string accessToken);
        Task<List<ScheduledClass>> GetTodaySessions(string accessToken);
        Task<bool> Logout(string accessToken, string trackingIdentifier);
    }
}
