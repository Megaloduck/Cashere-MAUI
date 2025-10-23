using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using Cashere.PageModels;
using Cashere.Pages;
using Cashere.Services;
using Microcharts.Maui;

namespace Cashere
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMicrocharts()                                // ✅ Enables Microcharts for data visualization
                .UseMauiCommunityToolkit()             // ✅ Enables toolkit features like Toast, Popup, etc.
                .ConfigureSyncfusionToolkit()            // ✅ Initializes Syncfusion controls properly
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // 🔧 Register Services (singleton: one shared instance)
            builder.Services.AddSingleton<ApiService>();

            // 🔧 Register ViewModels (transient: new instance per page)
            builder.Services.AddTransient<CashierPageModel>();
            builder.Services.AddTransient<LoginPageModel>();
            builder.Services.AddTransient<CheckoutPageModel>();

            // 🔧 Register Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<CheckoutPage>();

            return builder.Build();
        }
    }
}
