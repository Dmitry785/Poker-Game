using Poker.Connection;
using Poker.Services;
using Poker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Poker.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly List<BaseViewModel> _pages;
        private int currentPageIndex;
        private BaseViewModel currentPageViewModel;
        public BaseViewModel CurrentPageViewModel
        {
            get => currentPageViewModel;
            set
            {
                currentPageViewModel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPageName));
            }
        }
        public string CurrentPageName
        {
            get => (currentPageIndex == 0) ? "Game" : (currentPageIndex == 1) ? "Game info" : "Settings";
        }
        public RelayCommand SelectPageCommand { get; }
        public MainWindowViewModel()
        {
            SelectPageCommand = new RelayCommand(OnPageSelected);
            var sb = new SignalBus();
            var gameConfig = new GameConfig(1000, -1, -1, 10, 20, 6);
            var connection = new TcpConnection();
            var connectionManager = new ConnectionManager(connection);

            var gs = new GameService(sb, gameConfig, connectionManager);
            var gvm = new GameViewModel(gs, sb);
            var givm = new GameInfoViewModel(gs, sb);
            var svm = new SettingsViewModel(gs, sb);

            var connectionLogger = new HubbedLogger();
            var gsLogger = new HubbedLogger();
            var gsGameLogger = new ListBoxLogger(givm.GameLog);
            var gvmLogger = new HubbedLogger();
            var hubLogger = new ListBoxLoggerHub(svm.SettingsLog,
                new Dictionary<IHubbedLogger, string>()
                {
                    { connectionLogger, "connection"},
                    { gvmLogger, "game"},
                    { gsLogger, "gameService"}
                });
            gs.GameLogger = gsGameLogger;
            connection.Logger = connectionLogger;
            gs.Logger = gsLogger;
            gvm.Logger = gvmLogger;
            _pages = new List<BaseViewModel>() {
                gvm, givm, svm
            };
            currentPageViewModel = _pages[currentPageIndex];
        }
        private void OnPageSelected(object? parameter)
        {
            if (!int.TryParse(parameter?.ToString(), out int pageNumber))
                return;
            if (pageNumber < 0 || pageNumber >= _pages.Count)
                return;
            currentPageIndex = pageNumber;
            CurrentPageViewModel = _pages[currentPageIndex];
        }
    }
}
