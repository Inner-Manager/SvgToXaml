using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using SvgToXaml.ViewModels;
using SvgToXaml.Views;
using System.Reflection;
using System;

namespace SvgToXaml
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Get the version info
            var ver = Assembly.GetExecutingAssembly().GetName().Version;

            if (ver != null)
                Version = $"{ver.Major}.{ver.Minor}";

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// App version
        /// </summary>
        public static string Version { get; private set; } = "";
    }
}