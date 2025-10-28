using Microsoft.Maui.Controls;
using System.Net;
using System.Net.Sockets;

namespace Cashere.Pages
{
    public partial class ServerSetupPage : ContentPage
    {
        public ServerSetupPage()
        {
            InitializeComponent();
        }

        private async void OnShowIPClicked(object sender, EventArgs e)
        {
            try
            {
                string ipAddress = Dns.GetHostAddresses(Dns.GetHostName())
                    .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                    .ToString() ?? "Unavailable";

                IpLabel.Text = $"Your IP: {ipAddress}";
            }
            catch (Exception ex)
            {
                IpLabel.Text = $"Error getting IP: {ex.Message}";
            }
        }

        private async void OnStartServerClicked(object sender, EventArgs e)
        {
            int port = 7102;
            if (int.TryParse(PortEntry.Text, out int customPort))
                port = customPort;

            StatusLabel.TextColor = Colors.Gray;
            StatusLabel.Text = "Starting server...";

            try
            {
                // TODO: Replace this with your real server start logic
                await Task.Delay(1500); // simulate async startup
                bool serverStarted = true; // simulate result

                if (serverStarted)
                {
                    StatusLabel.Text = $"Server running on port {port}";
                    StatusLabel.TextColor = Colors.Green;

                    // Wait briefly, then navigate to login
                    await Task.Delay(1500);
                    await Shell.Current.GoToAsync("//login");
                }
                else
                {
                    StatusLabel.Text = "? Failed to start server";
                    StatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
            }
        }
    }
}