using LBScreenRecording.Contracts;
using LBScreenRecording.Services;
using NLog;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mod = LBScreenRecording.Model;

namespace LBScreenRecording
{
    public partial class Login : Form
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IUserService _userService;
        private Mod.LoginResponse loginResponse = new Mod.LoginResponse();

        public Login(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
            this.AcceptButton = btnLogin;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            _logger.Info($"[{textUsername.Text}]::Received request to Login");
            if (!IsValidLoginRequest())
            {
                return;
            }
            else
            {
                Task.Run(() => UserLogin()).Wait();
                if (loginResponse.IsSuccess)
                {
                    this.Hide();
                    Home home = new Home(_userService)
                    {
                        loginResponse = loginResponse
                    };
                    home.Show();
                }
                else
                {
                    MessageBox.Show("Invalid username or password..", "Invalid Credientials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }
        private async Task UserLogin()
        {
            _logger.Info($"[{textUsername.Text}]::Received request to UserLogin");
            loginResponse = await _userService.Login(textUsername.Text, textPassword.Text);
        }

        private void Login_Load(object sender, EventArgs e)
        {
            textUsername.Text = "ajay.009@lilbrahmas.com";
            textPassword.Text = "lilbrahmasdev";
            //textUsername.Text = "";
            //textPassword.Text = "";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _logger.Info($"[{textUsername.Text}]::Received request to clear the data");
            textUsername.Text = string.Empty;
            textPassword.Text = string.Empty;
        }

        public bool IsValidLoginRequest()
        {
            if (string.IsNullOrWhiteSpace(textUsername.Text) && string.IsNullOrWhiteSpace(textPassword.Text))
            {
                string message = "Please enter the Username and Password.";
                DialogResult result = MessageBox.Show(message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    return false;
                }
            }
            if (string.IsNullOrWhiteSpace(textUsername.Text))
            {
                string message = "Please enter the Username.";
                DialogResult result = MessageBox.Show(message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    return false;
                }
            }
            if (string.IsNullOrWhiteSpace(textPassword.Text))
            {
                string message = "Please enter the Password.";
                DialogResult result = MessageBox.Show(message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    return false;
                }
            }
            if (!IsValidEmail(textUsername.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
    }
}
