using MyVideoRecording.Contracts;
using MyVideoRecording.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MyVideoRecording.Services
{
    public class UserService : IUserService
    {
        private readonly IUserClient _userClient;

        public UserService(IUserClient userClient)
        {
            _userClient = userClient;
        }

        public async Task<LoginResponse> Login(string username, string password)
        {
            return await _userClient.Login(username, password);
        }
        public async Task<UserDetail> GetLoginUserData(string accessToken)
        {
            return await _userClient.GetLoginUserData(accessToken);
        }
        public async Task<GoogleDriveCrediantials> GetGoogleDriveCrediantials(string accessToken)
        {
            return await _userClient.GetGoogleDriveCrediantials(accessToken);
        }
        public async Task<List<ScheduledClass>> GetTodaySessions(string accessToken)
        {
            return await _userClient.GetTodaySessions(accessToken);
        }
        public async Task<bool> Logout(string accessToken, string trackingIdentifier)
        {
            return await _userClient.Logout(accessToken, trackingIdentifier);
        }
    }
}
