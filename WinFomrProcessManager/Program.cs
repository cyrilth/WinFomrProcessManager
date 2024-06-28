using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace WinFomrProcessManager
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using IHost host = CreateHostBuilder().Build();
            host.Start();

            Application.Run(new ProcessKillerContext(host.Services.GetRequiredService<IHostApplicationLifetime>()));
        }

        static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ProcessKillerService>();
                });
    }

    public class ProcessKillerContext : ApplicationContext
    {
        private readonly NotifyIcon trayIcon;
        private IHostApplicationLifetime hostApplicationLifetime;
        private readonly string password = "1234";

        public ProcessKillerContext(IHostApplicationLifetime hostApplicationLifetime)
        {
            this.hostApplicationLifetime = hostApplicationLifetime;

            trayIcon = new NotifyIcon
            {
                Icon = new Icon("assests/poweroff.ico"),
                Visible = true,
                Text = "Process Killer App"
            };


            ContextMenuStrip contextMenuStrip = new();
            ToolStripMenuItem exitMenuItem = new("Exit");
            exitMenuItem.Click += Exit;
            contextMenuStrip.Items.Add(exitMenuItem);

            trayIcon.ContextMenuStrip = contextMenuStrip;
        }

        public void Exit(object? sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter password to exit", "Password Required", "", -1, -1);

            if (input == password)
            {
                trayIcon.Visible = false;
                hostApplicationLifetime.StopApplication();
                Application.Exit();
            }
            else
            {
                MessageBox.Show("Incorrect password. The application will continue running.", "Exit Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    public class ProcessKillerService : BackgroundService
    {
        private readonly string targetProcess = "notepad";
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                CheckAndKillProcess();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private void CheckAndKillProcess()
        {
            Process[] processes = Process.GetProcesses().Where(x => x.ProcessName
                .StartsWith(targetProcess, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
            foreach (Process process in processes)
            {
                process.Kill();
                ShowNotification($"Process {targetProcess} has been terminated.");
            }
        }

        private void ShowNotification(string message)
        {
            Task.Run(() =>
            {
                MessageBox.Show(message, "Process Killer Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
    }
}