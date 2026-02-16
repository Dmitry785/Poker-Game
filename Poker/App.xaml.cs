using Poker.ViewModels;
using Poker.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Poker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;
            MainWindowViewModel mainWindowVM = new MainWindowViewModel();
            MainWindow window = new MainWindow(mainWindowVM);
            window.Show();
        }
    }

}
