using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Cashere.Pages;
using Font = Microsoft.Maui.Font;

namespace Cashere
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("main", typeof(MainPage));
            Routing.RegisterRoute("checkout", typeof(CheckoutPage));
            Routing.RegisterRoute("admin", typeof(AdminPanelPage));
        }
    }
}
