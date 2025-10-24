using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LBScreenRecording.Client;
using LBScreenRecording.Contracts;
using LBScreenRecording.Services;
using NLog;
using System;
using System.Windows.Forms;

namespace LBScreenRecording
{
    internal static class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static IServiceProvider ServiceProvider { get; private set; }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                logger.Info("Application started.");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var host = CreateHostBuilder().Build();
                ServiceProvider = host.Services;

                //Application.Run(new Login());
                Application.Run(ServiceProvider.GetRequiredService<Login>());
            }
            catch (Exception ex)
            {

                logger.Error(ex, "Unhandled exception in Program file");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IUserClient, UserClient>();
                    services.AddTransient<IUserService, UserService>();
                    services.AddTransient<Login>();
                    services.AddTransient<Home>();
                });

        }
    }
}
