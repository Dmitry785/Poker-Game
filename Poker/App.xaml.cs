using Poker.Models;
using Poker.ViewModels;
using Poker.Views;
using System.Configuration;
using System.Data;
using System.Net;
using System.Text.Json;
using System.Windows;

namespace Poker
{
    public record c(List<PlayerInfo> p);
    public partial class App : Application
    {
        
        protected override void OnStartup(StartupEventArgs e)
        {
           /* List<PlayerInfo> players = new List<PlayerInfo>()
            {
                new PlayerInfo("hello", 101),
                new ConnectedPlayerInfo("bye", 0, new IPEndPoint(IPAddress.Any, 1234))
            };
            var c = new c(players);
            var str = JsonSerializer.Serialize(c);
            MessageBox.Show(str);
            var p = JsonSerializer.Deserialize<c>(str);
            MessageBox.Show(p.ToString());
            MessageBox.Show(p.p.Count.ToString());
            return;*/
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;
            MainWindowViewModel mainWindowVM = new MainWindowViewModel();
            MainWindow window = new MainWindow(mainWindowVM);
            window.Show();
        }
    }

}
