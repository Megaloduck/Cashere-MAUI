using System.Net.Http;

namespace Cashere.Pages;

public partial class ServerConnectionPage : ContentPage
{
    public ServerConnectionPage()
    {
        InitializeComponent();
    }

    private async void OnCheckConnectionClicked(object sender, EventArgs e)
    {
        string baseUrl = ServerEntry.Text?.Trim();
        if (string.IsNullOrEmpty(baseUrl))
        {
            await DisplayAlert("Error", "Please enter a valid server IP", "OK");
            return;
        }

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var res = await http.GetAsync($"{baseUrl}/api/health");

            if (res.IsSuccessStatusCode)
            {
                StatusLabel.Text = "?? Server Online";
                StatusLabel.TextColor = Colors.Green;
            }
            else
            {
                StatusLabel.Text = "?? Server Offline";
                StatusLabel.TextColor = Colors.Red;
            }
        }
        catch
        {
            StatusLabel.Text = "?? Server Offline";
            StatusLabel.TextColor = Colors.Red;
        }
    }
}