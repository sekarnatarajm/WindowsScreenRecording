using LBScreenRecording.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LBScreenRecording.Contracts
{
    public interface IUserClient
    {
        Task<LoginResponse> Login(string username, string password);
        Task<UserDetail> GetLoginUserData(string accessToken);
        Task<List<ScheduledClass>> GetTodaySessions(string accessToken);
        Task<bool> Logout(string accessToken, string trackingIdentifier);
    }
}
