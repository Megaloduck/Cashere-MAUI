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
           // Routing.RegisterRoute("main", typeof(MainPage));
            Routing.RegisterRoute("checkout", typeof(CheckoutPage));
            Routing.RegisterRoute("admin", typeof(AdminPanelPage));
            Routing.RegisterRoute("dashboard", typeof(ReportPage));
            Routing.RegisterRoute("addEditMenuItem", typeof(AddEditMenuItemPage));

            // Debug: Print all registered routes
            System.Diagnostics.Debug.WriteLine("=== Registered Routes ===");
            // Routes are internal, so we can't list them easily, but check for duplicates manually
        }
    }
}
