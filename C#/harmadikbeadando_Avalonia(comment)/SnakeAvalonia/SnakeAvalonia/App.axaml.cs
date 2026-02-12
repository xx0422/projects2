using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SnakeAvalonia.ViewModels;
using SnakeAvalonia.Views;

namespace SnakeAvalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Létrehozzuk a közös ViewModel-t (a játék logikáját)
            var mainViewModel = new MainViewModel();

            // 1. ASZTALI GÉP (Desktop)
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // TRÜKK: Nem kell külön MainWindow fájl!
                // Létrehozunk egy egyszerű ablakot, és beletesszük a MainView-t.
                desktop.MainWindow = new Window
                {
                    Title = "Snake Avalonia",
                    Content = new MainView
                    {
                        DataContext = mainViewModel
                    }
                };
            }
            // 2. MOBIL (Android/iOS)
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                // Mobilon nincs ablak, ott közvetlenül a MainView a főszereplő
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = mainViewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}