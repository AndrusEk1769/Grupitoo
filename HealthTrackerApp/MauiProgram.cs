using Microsoft.Extensions.Logging;
using HealthTrackerApp.Services;

namespace HealthTrackerApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registreerib teenused
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddTransient<ViewModels.MainViewModel>();
            builder.Services.AddTransient<ViewModels.FoodViewModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<Views.FoodPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}