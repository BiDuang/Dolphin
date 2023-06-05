using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DolphinWeb;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main(string[] args) => await BuildAvaloniaApp()
        .UseReactiveUI()
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                FontFallbacks = new[]
                {
                    new FontFallback
                    {
                        FontFamily =
                            new FontFamily("avares://DolphinWeb/Assets/Fonts/JetBrainsMono-Regular.ttf#JetBrains Mono")
                    },
                    new FontFallback
                    {
                        FontFamily =
                            new FontFamily("avares://DolphinWeb/Assets/Fonts/MicrosoftYaHei.ttf#Microsoft YaHei")
                    }
                }
            });
}