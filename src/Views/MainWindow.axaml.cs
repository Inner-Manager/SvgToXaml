using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using SvgToXaml.ViewModels;

namespace SvgToXaml.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Control_OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel mvm) return;

            mvm.NotificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(this));
        }
    }
}