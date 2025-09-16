using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Resto2025
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Lexend-Light.ttf", "FontRegular");
                    fonts.AddFont("Lexend-SemiBold.ttf", "FontBold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
