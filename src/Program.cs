using Avalonia;
using System;

namespace SpaceTraffic
{
    public class Program
    {
        // Sem si uložíme název scénáře (výchozí je "default")
        public static string ScenarioName { get; private set; } = "default";

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Pokud byl zadán nějaký argument, přepíšeme "default" na zadaný název
            if (args.Length > 0)
            {
                ScenarioName = args[0];
            }

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
