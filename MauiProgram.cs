using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Resto2025
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseFFImageLoading()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Lexend-Light.ttf", "FontRegular");
                    fonts.AddFont("Lexend-SemiBold.ttf", "FontBold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
#if ANDROID
            // Registrasi khusus untuk Android
            builder.Services.AddSingleton<IBluetoothService, Resto2025.Platforms.Android.AndroidBlueToothService>();
#endif
            //builder.Services.AddSingleton<PrintPageViewModel>();
            //builder.Services.AddSingleton<Struk.Print1>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
