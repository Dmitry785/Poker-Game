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
            var gs = new GameService(sb);
            _pages = new List<BaseViewModel>() {
                new GameViewModel(gs, sb),
                new GameInfoViewModel(gs, sb),
                new SettingsViewModel(gs, sb)
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
